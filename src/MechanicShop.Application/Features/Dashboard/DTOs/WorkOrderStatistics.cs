namespace MechanicShop.Application.Features.Dashboard.DTOs;

public sealed class WorkOrderStatisticsDto
{
  public DateOnly Date { get; set; }
  public int TotalOrders { get; set; }
  public int Scheduled { get; set; }
  public int InProgress { get; set; }
  public int Completed { get; set; }
  public int Cancelled { get; set; }
  public decimal TotalRevenue { get; set; }
  public decimal TotalPartsCost { get; set; }
  public decimal TotalLaborCost { get; set; }
  public decimal NetProfit { get; set; }
  public int UniqueVehicles { get; set; }
  public int UniqueCustomers { get; set; }
  public decimal ProfitMargin { get; set; }
  public decimal CompletionRate { get; set; }
  public decimal AverageRevenuePerWorkOrder { get; set; }
  public decimal OrdersPerVehicle { get; set; }
  public decimal PartsCostRatio { get; set; }
  public decimal LaborCostRatio { get; set; }
  public decimal CancellationRate { get; set; }
}