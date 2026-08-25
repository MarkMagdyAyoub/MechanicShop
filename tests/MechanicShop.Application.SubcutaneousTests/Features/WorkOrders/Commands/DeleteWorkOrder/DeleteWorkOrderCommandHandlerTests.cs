using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    private static long _dayOffsetCounter = 0;

    private static DateOnly NextFreeDate()
    {
        var offset = Interlocked.Increment(ref _dayOffsetCounter);
        return DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).AddDays(offset));
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsScheduled_ReturnsDeletedResult()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsScheduled_WorkOrderIsRemovedFromDatabase()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        await _sender.Send(command);

        // Then
        var stillExists = await _context.WorkOrders
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);
        Assert.Null(stillExists);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsCancelled_ReturnsDeletedResult()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.Cancelled);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsInProgress_ReturnsWorkOrderCannotBeDeletedError()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.InProgress);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderCannotBeDeleted(WorkOrderState.InProgress).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsInProgress_NothingIsRemovedFromDatabase()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.InProgress);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        await _sender.Send(command);
  
        // Then
        var stillExists = await _context.WorkOrders.FindAsync(workOrder.Id);
        Assert.NotNull(stillExists);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsCompleted_ReturnsWorkOrderCannotBeDeletedError()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync(WorkOrderState.Completed);
        var command = new DeleteWorkOrderCommand(workOrder.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderCannotBeDeleted(WorkOrderState.Completed).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    private async Task<WorkOrder> CreateWorkOrderAsync(WorkOrderState targetState)
    {
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var customer = CustomerFactory.Create(
            email: $"customer-{Guid.NewGuid()}@example.com",
            vehicles: [VehicleFactory.Create(licensePlate: Guid.NewGuid().ToString("N")[..8].ToUpper()).Value]
        ).Value;

        var employee = EmployeeFactory.Create(role: Role.Labor).Value;

        var repairTask = RepairTaskFactory.Create(
            name: $"Repair Task {Guid.NewGuid()}",
            estimatedDurationInMins: RepairDurationInMinutes._60
        ).Value;

        _context.Customers.Add(customer);
        _context.Employees.Add(employee);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        var vehicle = customer.Vehicles.First();
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

        ApplyStateTransitions(workOrder, targetState);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return workOrder;
    }

    private static void ApplyStateTransitions(WorkOrder workOrder, WorkOrderState targetState)
    {
        switch (targetState)
        {
          case WorkOrderState.Scheduled:
              break;
          case WorkOrderState.InProgress:
              workOrder.UpdateState(WorkOrderState.InProgress);
              break;
          case WorkOrderState.Completed:
              workOrder.UpdateState(WorkOrderState.InProgress);
              workOrder.UpdateState(WorkOrderState.Completed);
              break;
          case WorkOrderState.Cancelled:
              workOrder.UpdateState(WorkOrderState.Cancelled);
              break;
        }
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}