using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.RemoveRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
  private readonly IAppDbContext _context = factory.CreateDbContext();
  private readonly ISender _sender = factory.CreateSender();

  [Fact]
  public async Task Handle_WhenValidCommand_ReturnsDeletedResult()
  {
      var existing = RepairTaskFactory.Create().Value;
      _context.RepairTasks.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var command = new RemoveRepairTaskCommand(existing.Id);

      var result = await _sender.Send(command);

      Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_WhenValidCommand_RepairTaskIsRemovedFromDatabase()
  {
      var existing = RepairTaskFactory.Create().Value;
      _context.RepairTasks.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var command = new RemoveRepairTaskCommand(existing.Id);

      await _sender.Send(command);

      var stillExists = await _context.RepairTasks
          .AsNoTracking()
          .AnyAsync(rt => rt.Id == existing.Id);

      Assert.False(stillExists);
  }

  [Fact]
  public async Task Handle_WhenRepairTaskIsNotExists_ReturnRepairTaskNotFoundError()
  {
      // Given
      var command = new RemoveRepairTaskCommand(Guid.NewGuid());

      // When
      var result = await _sender.Send(command);

      // Then
      Assert.False(result.IsSuccess);
      Assert.Contains(result.Errors , e => e.Code == ApplicationErrors.RepairTaskNotFound.Code);
  }

  [Fact]
  public async Task Handle_WhenRepairTaskInWorkOrder_ReturnInUseError()
  {
      var repairTask = RepairTaskFactory.Create().Value;
      var employee = EmployeeFactory.Create().Value;
      var vehicle = VehicleFactory.Create().Value;
      var customer = CustomerFactory.Create(vehicles: [vehicle]).Value;

      var workOrder = WorkOrderFactory.Create(
          vehicleId: vehicle.Id,
          laborId: employee.Id,
          repairTasks: [repairTask]).Value;

      _context.Employees.Add(employee);
      _context.Vehicles.Add(vehicle);
      _context.RepairTasks.Add(repairTask);
      _context.WorkOrders.Add(workOrder);
      _context.Customers.Add(customer);
      await _context.SaveChangesAsync(CancellationToken.None);

      var command = new RemoveRepairTaskCommand(repairTask.Id);

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
      Assert.Contains(result.Errors, e => e.Code == RepairTaskErrors.InUse.Code);
  }

  public Task DisposeAsync() => factory.ResetDatabaseAsync();

  public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
