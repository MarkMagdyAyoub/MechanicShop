using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandHandler(
  IAppDbContext context,
  ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
  HybridCache cache,
  IWorkOrderPolicy workOrderPolicy
) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;
  private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;

  public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand request, CancellationToken cancellationToken)
  {
    // check if repair tasks exists
    var workOrder = await _context.WorkOrders.FindAsync(request.WorkOrderId , cancellationToken);
    if(workOrder is null)
    {
      _logger.LogWarning("Work Order With Id `{workOrderId}` Is Not Found" , request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    // check if the number of repair tasks in the db equal the number of repair tasks in the request
    var repairTasks = await _context.RepairTasks
                            .Where(rt => request.RepairTaskIds.Contains(rt.Id))
                            .ToListAsync(cancellationToken);

    if(repairTasks.Count != request.RepairTaskIds.Count)
    {
      var missingIds = request.RepairTaskIds.Except(repairTasks.Select(rt => rt.Id)).ToList();
      _logger.LogWarning("One Or More RepairTasks Not Found. {ids}", string.Join(", ", missingIds));
      return ApplicationErrors.RepairTaskMissing(missingIds);
    }
    
    // clear old repair tasks and add new ones
    var clearExistingRepairTasksResult = workOrder.ClearRepairTasks();

    if (clearExistingRepairTasksResult.IsError)
    {
      return clearExistingRepairTasksResult.Errors;
    }

    foreach(var task in repairTasks)
    {
      var addRepairTaskResult = workOrder.AddRepairTask(task);

      if (addRepairTaskResult.IsError)
      {
        return addRepairTaskResult.Errors;
      }
    }

    var duration = TimeSpan.FromMinutes(repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins));
    var newEndAt = workOrder.StartAtUtc.Add(duration);

    if(_workOrderPolicy.IsOutSideWorkingHours(workOrder.StartAtUtc , duration))
    {
      return ApplicationErrors.WorkOrderOperatingHour(workOrder.StartAtUtc , newEndAt);
    }

    var isLaborOccupied = await _context.WorkOrders.AnyAsync(
                                                      wo => 
                                                        wo.Spot == workOrder.Spot &&
                                                        wo.LaborId == workOrder.LaborId &&
                                                        wo.Id != workOrder.Id &&
                                                        wo.StartAtUtc < newEndAt &&
                                                        wo.EndAtUtc > workOrder.StartAtUtc,
                                                      cancellationToken
                                                    );
    if (isLaborOccupied)
    {
      _logger.LogWarning("Labor With Id `{laborId}` Is Already Occupied."  , workOrder.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    // check the updated work order overlapped with another work order
    var isThereOverlappingWorkOrder = await _context.WorkOrders.AnyAsync(
      wo => 
        wo.Spot == workOrder.Spot &&
        wo.Id != workOrder.Id &&
        wo.StartAtUtc < newEndAt &&
        wo.EndAtUtc > workOrder.StartAtUtc,
      cancellationToken
    );

    if (isThereOverlappingWorkOrder)
    {
      _logger.LogWarning("Work Order With Id `{workOrderId} Is Overlapping With Another With Order`" , request.WorkOrderId);
      return ApplicationErrors.WorkOrderOverlapping;
    }

    workOrder.UpdateTiming(workOrder.StartAtUtc , newEndAt);

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    await _context.SaveChangesAsync(cancellationToken);
    await _cache.RemoveByTagAsync("work-order" , cancellationToken);

    return Result.Updated;
  }
}