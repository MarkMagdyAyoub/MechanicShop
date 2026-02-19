using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Mappers;

public static class WorkOrderMappers
{
  public static WorkOrderDto ToDto(this WorkOrder workOrder)
  {
    ArgumentNullException.ThrowIfNull(workOrder);

    return new WorkOrderDto
    {
      WorkOrderId = workOrder.Id,
      Spot = workOrder.Spot,
      StartAtUtc = workOrder.StartAtUtc,
      EndAtUtc = workOrder.EndAtUtc,
      Labor = workOrder.Labor is null ? null : workOrder.Labor.ToDto(),
      RepairTasks = workOrder.RepairTasks.Select(rt => rt.ToDto()).ToList(),
      Vehicle = workOrder.Vehicle is null ? null : workOrder.Vehicle.ToDto(),
      State = workOrder.State,
      TotalPartsCost = workOrder.TotalPartsCost,
      TotalLaborCost = workOrder.TotalLaborCost,
      Total = workOrder.Total,
      TotalDurationInMins = workOrder.RepairTasks.Sum(rt => (int)rt.EstimatedDurationInMins),
      CreateAt = workOrder.CreatedAtUtc,
      InvoiceId = workOrder.Invoice?.Id
    };
  }
}  