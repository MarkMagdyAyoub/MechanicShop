using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;

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

  public static async Task<WorkOrder> GetRandomWorkOrderAsync(IAppDbContext _context)
  {
      var customer = CustomerFactory.Create(
          email: $"customer-{Guid.NewGuid()}@example.com",
          vehicles: [VehicleFactory.Create(licensePlate: Guid.NewGuid().ToString("N")[..8].ToUpper()).Value]
      ).Value;

      var employee = EmployeeFactory.Create(role: Domain.Identity.Role.Labor).Value;

      var repairTask = RepairTaskFactory.Create(name: $"Repair Task {Guid.NewGuid()}").Value;

      _context.Customers.Add(customer);
      _context.Employees.Add(employee);
      _context.RepairTasks.Add(repairTask);
      await _context.SaveChangesAsync(CancellationToken.None);

      var vehicle = customer.Vehicles.First();

      var startAt = DateTimeOffset.UtcNow.AddDays(-1);
      var endAt = startAt.AddMinutes((int)repairTask.EstimatedDurationInMins);

      var workOrder = WorkOrder.Create(
          Guid.NewGuid(),
          vehicle.Id,
          startAt,
          endAt,
          employee.Id,
          Spot.A,
          [repairTask]
      ).Value;

      _context.WorkOrders.Add(workOrder);
      await _context.SaveChangesAsync(CancellationToken.None);

      return workOrder;
  }

  public static async Task<WorkOrder> GetRandomCompletedWorkOrderAsync(IAppDbContext _context)
  {
    var workOrder = await GetRandomWorkOrderAsync(_context);

    workOrder.UpdateState(WorkOrderState.InProgress);
    workOrder.UpdateState(WorkOrderState.Completed);

    await _context.SaveChangesAsync(CancellationToken.None);

    return workOrder;
  }
}