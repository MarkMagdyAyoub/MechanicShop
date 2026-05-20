using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTaskGenerator;

namespace MechanicShop.Tests.Common.WorkOrderGenerator;

public static class WorkOrderFactory
{
  public static Result<WorkOrder> Create(
    Guid? id = null, 
    Guid? vehicleId = null, 
    DateTimeOffset? startAt = null, 
    DateTimeOffset? endAt = null, 
    Guid? laborId = null, 
    Spot? spot = null, 
    List<RepairTask>? repairTasks = null
  )
  {
    return WorkOrder.Create(
      id ?? Guid.NewGuid(),
      vehicleId ?? Guid.NewGuid(),
      startAt ?? DateTimeOffset.UtcNow,
      endAt ?? DateTimeOffset.UtcNow.AddMinutes(120),
      laborId ?? Guid.NewGuid(),
      spot ?? Spot.A,
      repairTasks ?? [RepairTaskFactory.Create().Value]
    );
  }
}