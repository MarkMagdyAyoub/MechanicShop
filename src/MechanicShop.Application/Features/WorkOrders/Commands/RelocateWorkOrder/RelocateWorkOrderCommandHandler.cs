using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandHandler(
  IAppDbContext context,
  ILogger<RelocateWorkOrderCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders.FindAsync(request.WorkOrderId , cancellationToken);

    if (workOrder is null)
    {
        _logger.LogWarning("WorkOrder With Id `{WorkOrderId}` Does Not Found.", request.WorkOrderId);

        return ApplicationErrors.WorkOrderNotFound;
    }
    
    var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
    var newEndAt = request.NewStartAt.Add(duration);

    // check if the new spot is available
    var hasSpotConflict = await _context.WorkOrders.AnyAsync(
                                                      wo => 
                                                        wo.Spot == request.NewSpot &&
                                                        wo.Id != request.WorkOrderId &&
                                                        wo.StartAtUtc < newEndAt &&
                                                        wo.EndAtUtc > request.NewStartAt,
                                                      cancellationToken
                                                    );

    if (hasSpotConflict)
    {
      _logger.LogWarning("Spot: `{Spot}` Is Not Available From `{newStartAt}` To `{newEndAt}`.", request.NewSpot.ToString() , request.NewStartAt , newEndAt);

      return ApplicationErrors.SpotAvailability(request.NewStartAt , newEndAt);
    }

    // check if the labor is available 
    var isLaborOccupied = await _context.WorkOrders
                          .AnyAsync(
                            wo => 
                              wo.LaborId == workOrder.LaborId && 
                              wo.Id != request.WorkOrderId &&
                              wo.StartAtUtc < newEndAt &&
                              wo.EndAtUtc > request.NewStartAt,
                            cancellationToken
                          );

    if (isLaborOccupied)
    {
      _logger.LogWarning("Labor With Id `{LaborId}` Is Already Occupied During The Requested Time.", workOrder.LaborId);

      return ApplicationErrors.LaborOccupied;
    }

    // check if the vehicle already maintained in another spot within this time
    var doesVehicleUnderMaintenance = await _context.WorkOrders
                                            .AsNoTracking()
                                            .AnyAsync(
                                              wo => 
                                                wo.Id != request.WorkOrderId &&
                                                wo.VehicleId == workOrder.VehicleId &&
                                                wo.StartAtUtc < newEndAt &&
                                                wo.EndAtUtc > request.NewStartAt,
                                              cancellationToken
                                            );

    if (doesVehicleUnderMaintenance)
    {
      _logger.LogWarning("Vehicle With Id `{vehicleId}` Is Already Under Maintenance" , workOrder.VehicleId);

      return ApplicationErrors.VehicleSchedulingConflict;
    }

    var updateTimingResult = workOrder.UpdateTiming(request.NewStartAt , newEndAt);
    if (updateTimingResult.IsError)
    {
        _logger.LogWarning(
            "Failed To Update Timing For Work Order With Id `{workOrderId}`: {Error}", 
            request.WorkOrderId , 
            updateTimingResult.TopError.Description);

        return updateTimingResult.Errors;
    }

    var updateWorkOrderSpotResult = workOrder.UpdateSpot(request.NewSpot);
    if (updateWorkOrderSpotResult.IsError)
    {
        _logger.LogWarning(
            "Failed To Update Spot For Work Order With Id `{workOrderId}`: {Error}",
            request.WorkOrderId,
            updateWorkOrderSpotResult.TopError.Description);

        return updateWorkOrderSpotResult.Errors;
    }

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("work-order" , cancellationToken);

    return Result.Updated;
  }
}