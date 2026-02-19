using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.DTOs;

public class WorkOrderDto
{
  public Guid WorkOrderId { get; set; }
  // Nullable because the invoice is generated after the work order 
  // is completed and pricing is finalized.
  public Guid? InvoiceId { get; set; }
  public Spot Spot { get; set; }
  public VehicleDto? Vehicle { get; set; }
  public DateTimeOffset StartAtUtc { get; set; }
  public DateTimeOffset EndAtUtc { get; set; }
  // Nullable because the assigned labor may be deleted or unassigned.
  // The work order remains for historical purposes.
  public LaborDto? Labor { get; set; } 
  public WorkOrderState State { get; set; }
  public List<RepairTaskDto> RepairTasks { get; set; } = [];
  public decimal TotalPartsCost { get; set; }
  public decimal TotalLaborCost { get; set; }
  public decimal Total { get; set; }
  public int TotalDurationInMins { get; set; }
  public DateTimeOffset CreateAt { get; set; }
}