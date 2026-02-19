using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler(
  IAppDbContext context,
  ILogger<CreateWorkOrderCommandHandler> logger,
  IWorkOrderPolicy workOrderPolicy,
  HybridCache cache
) : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
  private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
  private readonly HybridCache _cache = cache;

  public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var repairTasks = await _context.RepairTasks.AsNoTracking()
                            .Where(rt => request.RepairTaskIds.Contains(rt.Id))
                            .ToListAsync(cancellationToken); 

    if(repairTasks.Count != request.RepairTaskIds.Count)
    {
      var missingIds = request.RepairTaskIds.Except(repairTasks.Select(rt => rt.Id)).ToList();
      
      _logger.LogWarning("Some Repair Tasks Ids Not Found: {missingIds}" , string.Join(", " , missingIds));
      
      return ApplicationErrors.RepairTaskMissing(missingIds);
    }
    
    var labor = await _context.Employees.FirstOrDefaultAsync(l => l.Id == request.LaborId && l.Role == Role.Labor , cancellationToken);
    
    if(labor is null)
    {
      _logger.LogWarning("Labor Id `{LaborId}` Not Found" , request.LaborId);
      return ApplicationErrors.LaborNotFound;
    }

    var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId , cancellationToken);

    if(vehicle is null)
    {
      _logger.LogWarning("Vehicle Id `{VehicleId}` Not Found" , request.VehicleId);
      return ApplicationErrors.VehicleNotFound;
    }

    var totalEstimatedWorkingDuration = TimeSpan.FromMinutes(repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins));
    var endAt = request.StartAt.Add(totalEstimatedWorkingDuration);

    if(_workOrderPolicy.IsOutSideWorkingHours(request.StartAt , totalEstimatedWorkingDuration))
    {
      _logger.LogWarning("The Work Order Time ({startAt} - {endAt}) Is OutSide Of Working Hours" , request.StartAt , endAt);
      
      return ApplicationErrors.WorkOrderOperatingHour(request.StartAt , endAt);
    }

    var checkMinimumRequirement = _workOrderPolicy.ValidateMinimumRequirement(request.StartAt , endAt);
    if(checkMinimumRequirement.IsError)
    {
      _logger.LogWarning("WorkOrder Duration Is Shorter Than The Configured Minimum.");
      return checkMinimumRequirement.Errors;
    }

    // is the spot available within this segment (endAt-StartAt)
    var isOccupied = await _context.WorkOrders.AsNoTracking().AnyAsync(
      wo => 
        wo.Spot == request.Spot &&
        wo.StartAtUtc < endAt &&
        wo.EndAtUtc > request.StartAt,
      cancellationToken
    );

    if (isOccupied)
    {
      _logger.LogWarning("Spot {Spot}: Is Not Available From {startAt} To {EndAt}" , request.Spot.ToString() , request.StartAt , endAt);
      return ApplicationErrors.SpotAvailability(request.StartAt , endAt);
    }


    // check if the vehicle already maintained in another spot within this time
    var doesVehicleUnderMaintenance = await _context.WorkOrders.AsNoTracking().AnyAsync(
      wo => 
        wo.VehicleId == request.VehicleId &&
        wo.StartAtUtc < endAt &&
        wo.EndAtUtc > request.StartAt,
      cancellationToken
    );

    if (doesVehicleUnderMaintenance)
    {
      _logger.LogWarning("Vehicle With Id `{vehicleId}` Is Already Under Maintenance" , request.VehicleId);
      return ApplicationErrors.VehicleAlreadyUnderMaintenance;
    }


    // is labor occupied within this time (request.StartAt to endAt)
    var isLaborOccupied = await _context.WorkOrders.AsNoTracking().AnyAsync(
      wo => 
        wo.LaborId == request.LaborId &&
        wo.StartAtUtc < endAt &&
        wo.EndAtUtc > request.StartAt,
        cancellationToken
    );

    if (isLaborOccupied)
    {
      _logger.LogWarning("Labor With Id `{laborId}` Is Already Occupied During The Requested Time."  , request.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    var createWorkOrderResult = WorkOrder.Create(
      Guid.NewGuid(),
      request.VehicleId,
      request.StartAt,
      endAt,
      request.LaborId,
      request.Spot,
      repairTasks
    );

    if (createWorkOrderResult.IsError)
    {
      _logger.LogWarning("WorkOrder Creation Failed: {Error}" , createWorkOrderResult.Errors);
      return createWorkOrderResult.Errors;
    }

    var workOrder = createWorkOrderResult.Value;

    _context.WorkOrders.Add(workOrder);

    workOrder.Vehicle = vehicle;
    workOrder.Labor = labor;

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("work-order" , cancellationToken);

    _logger.LogInformation("WorkOrder With Id `{WorkOrderId}` Is Created Successfully" , workOrder.Id);

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    return workOrder.ToDto();
  }
}