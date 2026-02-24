using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public sealed record GetWorkOrdersQuery(
  int PageNumber,
  int PageSize,
  string? SearchTerm,
  string SearchColumn = "CreatedAt",
  string SortDirection = "DESC",
  WorkOrderState? State = null,
  Guid? VehicleId = null,
  Guid? LaborId = null,
  DateTime? StartDateFrom = null,
  DateTime? StartDateTo = null,
  DateTime? EndDateFrom = null,
  DateTime? EndDateTo = null,
  Spot? Spot = null
) : ICachedQuery<Result<PaginatedList<WorkOrderListItemDto>>>
{
    public string CacheKey =>
      $"work-orders:p={PageNumber}:ps={PageSize}" +
      $":q={SearchTerm ?? "-"}" +
      $":sort={SearchColumn}:{SortDirection}" +
      $":state={State?.ToString() ?? "-"}" +
      $":veh={VehicleId?.ToString() ?? "-"}" +
      $":lab={LaborId?.ToString() ?? "-"}" +
      $":sdfrom={StartDateFrom?.ToString("yyyyMMdd") ?? "-"}" +
      $":sdto={StartDateTo?.ToString("yyyyMMdd") ?? "-"}" +
      $":edfrom={EndDateFrom?.ToString("yyyyMMdd") ?? "-"}" +
      $":edto={EndDateTo?.ToString("yyyyMMdd") ?? "-"}" +
      $":spot={Spot?.ToString() ?? "-"}";
  public string[] Tags => ["work-order"];
  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}