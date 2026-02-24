using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed record GetWorkOrderByIdQuery(
  Guid WorkOrderId
) : ICachedQuery<Result<WorkOrderDto>>
{
  public string CacheKey => $"work-order:{WorkOrderId}";
  public string[] Tags => ["work-order"];
  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}