using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandHandler(
  IAppDbContext context,
  ILogger<DeleteWorkOrderCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<DeleteWorkOrderCommand ,  Result<Deleted>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<DeleteWorkOrderCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders.FindAsync([request.WorkOrderId] , cancellationToken);

    if(workOrder is null)
    {
      _logger.LogWarning("WorkOrder With Id `{workOrderId}` Not Found For Deletion" , request.WorkOrderId);
      return ApplicationErrors.WorkOrderNotFound;
    }

    if(workOrder.State is not (WorkOrderState.Scheduled or WorkOrderState.Cancelled))
    {
      _logger.LogWarning("Try To Delete WorkOrder With Id `{workOrderId}` But It's State Is `{workOrderState}`" , request.WorkOrderId , workOrder.State.ToString());
      return ApplicationErrors.WorkOrderCannotBeDeleted(workOrder.State);
    }

    workOrder.AddDomainEvent(new WorkOrderCollectionModified());

    _context.WorkOrders.Remove(workOrder);

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("work-order" , cancellationToken);

    return Result.Deleted;
  }
}