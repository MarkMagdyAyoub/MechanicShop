using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandHandler(
  IAppDbContext context,
  ILogger<UpdateWorkOrderStateCommandHandler> logger,
  HybridCache cache,
  TimeProvider timeProvider
) : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<UpdateWorkOrderStateCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;
  private readonly TimeProvider _timeProvider = timeProvider;

  public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders.FindAsync([request.WorkOrderId] , cancellationToken);
    
    if(workOrder is null)
    {
      _logger.LogWarning("WorkOrder With Id `{workOrderId}` Not Found For State Update" , request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    // check if the work order starting time in the future
    if(workOrder.StartAtUtc > _timeProvider.GetUtcNow()) 
    {
      _logger.LogWarning("State Transition For WorkOrder Id `{workOrderId}` Is Not Allowed Before The Work Order Scheduled Start Time." , request.WorkOrderId);
      return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
    }
    
    var updateWorkOrderStateResult = workOrder.UpdateState(request.State);

    if (updateWorkOrderStateResult.IsError)
    {
      _logger.LogWarning("Failed To Update Work Order State With Id {workOrderId}: {Error}`" , request.WorkOrderId , updateWorkOrderStateResult.Errors);
      return updateWorkOrderStateResult.Errors;
    }

    if(request.State == WorkOrderState.Completed)
    {
      workOrder.AddDomainEvent(new WorkOrderCompleted
      {
        WorkOrderId = request.WorkOrderId
      });
    }

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("work-order" , cancellationToken);

    return Result.Updated;
  }
}