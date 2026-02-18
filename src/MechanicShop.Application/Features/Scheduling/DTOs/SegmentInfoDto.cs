using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Scheduling.DTOs;

// this segment can be empty or reserved
public sealed class SegmentInfoDto
{
  public Spot Spot { get; set; }
  public Guid? WorkOrderId { get; set; }
  public DateTimeOffset StartAt { get; set; }
  public DateTimeOffset EndAt { get; set; }
  public string? Vehicle { get; set; }
  public bool IsAvailable { get; set; }
  public bool IsOccupied { get; set; }
  public bool WorkOrderLocked { get; set; }
  public LaborDto? Labor { get; set; }
  public WorkOrderState? WorkOrderState { get; set; }
  public List<RepairTaskDto>? RepairTasks { get; set; }
} 