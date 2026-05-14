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

  public async Task<Result<WorkOrderStatisticsDto>> Handle(
    GetWorkOrderStatisticsQuery request, CancellationToken cancellationToken)
  {
    var start = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    var end   = request.Date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    var counts = await _context.WorkOrders
                        .AsNoTracking()
                        .Where(wo => wo.StartAtUtc >= start && wo.StartAtUtc < end)
                        .GroupBy(_ => 1)
                        .Select(g => new
                        {
                            Total          = g.Count(),
                            Scheduled      = g.Count(wo => wo.State == WorkOrderState.Scheduled),
                            InProgress     = g.Count(wo => wo.State == WorkOrderState.InProgress),
                            Completed      = g.Count(wo => wo.State == WorkOrderState.Completed),
                            Cancelled      = g.Count(wo => wo.State == WorkOrderState.Cancelled),
                            UniqueVehicles  = g.Select(wo => wo.VehicleId).Distinct().Count(),
                            UniqueCustomers = g.Select(wo => wo.Vehicle!.CustomerId).Distinct().Count()
                        })
                        .FirstOrDefaultAsync(cancellationToken);

      if (counts is null || counts.Total == 0)
      {
          _logger.LogInformation("No Work Orders Found For Date `{Date}`", request.Date);
          return new WorkOrderStatisticsDto { Date = request.Date };
      }

      var invoices = await _context.WorkOrders
                            .AsNoTracking()
                            .Where(wo => wo.StartAtUtc >= start && wo.StartAtUtc < end && wo.Invoice != null)
                            .Select(wo => new
                            {
                                wo.Invoice!.Total,    
                                wo.TotalPartsCost,
                                wo.TotalLaborCost
                            })
                            .ToListAsync(cancellationToken);   

      var totalRevenue    = invoices.Sum(i => i.Total);
      var totalPartsCost  = invoices.Sum(i => i.TotalPartsCost);
      var totalLaborCost  = invoices.Sum(i => i.TotalLaborCost);

      var netProfit       = totalRevenue - totalPartsCost - totalLaborCost;
      var profitMargin    = totalRevenue > 0 ? netProfit / totalRevenue * 100 : 0;
      var completionRate  = (decimal)counts.Completed / counts.Total * 100;
      var cancellationRate = (decimal)counts.Cancelled / counts.Total * 100;

      return new WorkOrderStatisticsDto
      {
          Date                       = request.Date,
          TotalOrders                = counts.Total,
          Scheduled                  = counts.Scheduled,
          InProgress                 = counts.InProgress,
          Completed                  = counts.Completed,
          Cancelled                  = counts.Cancelled,
          UniqueVehicles             = counts.UniqueVehicles,
          UniqueCustomers            = counts.UniqueCustomers,
          TotalRevenue               = totalRevenue,
          TotalPartsCost             = totalPartsCost,
          TotalLaborCost             = totalLaborCost,
          NetProfit                  = netProfit,
          ProfitMargin               = profitMargin,
          CompletionRate             = completionRate,
          AverageRevenuePerWorkOrder = totalRevenue / counts.Total,
          OrdersPerVehicle           = (decimal)counts.Total / counts.UniqueVehicles,
          PartsCostRatio             = totalRevenue > 0 ? totalPartsCost / totalRevenue * 100 : 0,
          LaborCostRatio             = totalRevenue > 0 ? totalLaborCost / totalRevenue * 100 : 0,
          CancellationRate           = cancellationRate
      };
  }
}