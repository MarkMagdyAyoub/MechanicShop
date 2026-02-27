using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;

public sealed record GetWorkOrderStatisticsQuery(
  DateOnly Date
) : ICachedQuery<Result<WorkOrderStatisticsDto>>
{
  public string CacheKey => $"dashboard-{Date}";

  public string[] Tags => ["dashboard"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}