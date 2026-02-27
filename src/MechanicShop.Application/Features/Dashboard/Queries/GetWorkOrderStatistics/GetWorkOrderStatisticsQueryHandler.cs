using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;

public sealed class GetWorkOrderStatisticsQueryHandler(
  IAppDbContext context,
  ILogger<GetWorkOrderStatisticsQueryHandler> logger
) : IRequestHandler<GetWorkOrderStatisticsQuery, Result<WorkOrderStatisticsDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetWorkOrderStatisticsQueryHandler> _logger = logger;

  public async Task<Result<WorkOrderStatisticsDto>> Handle(GetWorkOrderStatisticsQuery request, CancellationToken cancellationToken)
  {
    var start = request.Date.ToDateTime(TimeOnly.MinValue , DateTimeKind.Utc);
    var end   = request.Date.AddDays(1).ToDateTime(TimeOnly.MinValue , DateTimeKind.Utc);
    
    var statistics = await _context.WorkOrders
                      .AsNoTracking()
                      .Where(wo => wo.StartAtUtc >= start && wo.StartAtUtc < end)
                      .GroupBy(_ => 1) // => group all table into only one single group
                      .Select(
                        g => new
                        {
                          Total = g.Count(),
                          Scheduled = g.Count(wo => wo.State == WorkOrderState.Scheduled),
                          InProgress = g.Count(wo => wo.State == WorkOrderState.InProgress),
                          Completed = g.Count(wo => wo.State == WorkOrderState.Completed),
                          Cancelled = g.Count(wo => wo.State == WorkOrderState.Cancelled),
                          TotalRevenue = g.Sum(wo => wo.Invoice != null ? wo.Invoice.Total : 0m),
                          TotalPartsCost = g.Sum(wo => wo.Invoice != null ?  wo.TotalPartsCost : 0m),
                          TotalLaborCost = g.Sum(wo => wo.Invoice != null ? wo.TotalLaborCost : 0m),
                          UniqueVehicles = g.Select(wo => wo.VehicleId).Distinct().Count(),
                          UniqueCustomers = g.Select(wo => wo.Vehicle!.CustomerId).Distinct().Count()
                        })
                      .FirstOrDefaultAsync(cancellationToken);
    
    if (statistics is null)
    {
        _logger.LogInformation("No Work Orders Found For Date `{Date}`" , request.Date);
        return new WorkOrderStatisticsDto
        {
            Date = request.Date
        };
    }

    
    var netProfit = statistics.TotalRevenue - statistics.TotalPartsCost - statistics.TotalLaborCost;
    var profitMargin = statistics.TotalRevenue > 0 ? netProfit / statistics.TotalRevenue * 100 : 0;
    var completionRate = statistics.Total > 0 ? (decimal)statistics.Completed / statistics.Total * 100 : 0;
    var averageRevenuePerWorkOrder = statistics.Total > 0 ? statistics.TotalRevenue / statistics.Total : 0;
    var workOrderPerVehicle = statistics.UniqueVehicles > 0 ? (decimal)statistics.Total / statistics.UniqueVehicles : 0;
    var partsCostRatio = statistics.TotalRevenue > 0 ? statistics.TotalPartsCost / statistics.TotalRevenue * 100: 0;
    var laborCostRatio = statistics.TotalRevenue > 0 ? statistics.TotalLaborCost / statistics.TotalRevenue * 100: 0;
    var cancellationRate = statistics.Total > 0 ? (decimal)statistics.Cancelled / statistics.Total * 100 : 0;
    
    return new WorkOrderStatisticsDto
    {
        Date = request.Date,
        TotalOrders = statistics.Total,
        Scheduled = statistics.Scheduled,
        InProgress = statistics.InProgress,
        Completed = statistics.Completed,
        Cancelled = statistics.Cancelled,
        TotalRevenue = statistics.TotalRevenue,
        TotalPartsCost = statistics.TotalPartsCost,
        TotalLaborCost = statistics.TotalLaborCost,
        UniqueVehicles = statistics.UniqueVehicles,
        UniqueCustomers = statistics.UniqueCustomers,
        NetProfit = netProfit,
        ProfitMargin = profitMargin,
        CompletionRate = completionRate,
        AverageRevenuePerWorkOrder = averageRevenuePerWorkOrder,
        OrdersPerVehicle = workOrderPerVehicle,
        PartsCostRatio = partsCostRatio,
        LaborCostRatio = laborCostRatio,
        CancellationRate = cancellationRate
    };
  }
}