using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
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
using Microsoft.Extensions.Time.Testing;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderState;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderStateCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();
    private readonly FakeTimeProvider _timeProvider = factory.GetFakeTimeProvider();

    private static long _dayOffsetCounter = 0;

    private static DateOnly NextFreeDate()
    {
        var offset = Interlocked.Increment(ref _dayOffsetCounter);
        return DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).AddDays(offset));
    }

    [Fact]
    public async Task Handle_WhenTransitionIsValidAndStartTimeHasPassed_ReturnsUpdatedResult()
    {
        // Given
        var startAt = new DateTimeOffset(NextFreeDate().Year, NextFreeDate().Month, NextFreeDate().Day, 10, 0, 0, TimeSpan.Zero);
        var workOrder = await CreateWorkOrderAsync(startAt);

        _timeProvider.SetUtcNow(startAt.AddMinutes(5));

        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenTransitionIsValid_StateIsUpdatedInDatabase()
    {
        // Given
        var startAt = new DateTimeOffset(NextFreeDate().Year, NextFreeDate().Month, NextFreeDate().Day, 10, 0, 0, TimeSpan.Zero);
        var workOrder = await CreateWorkOrderAsync(startAt);

        _timeProvider.SetUtcNow(startAt.AddMinutes(5));

        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // When
        await _sender.Send(command);

        // Then
        var saved = await _context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Equal(WorkOrderState.InProgress, saved.State);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenStartTimeIsInTheFuture_ReturnsStateTransitionNotAllowedError()
    {
        // Given
        var startAt = new DateTimeOffset(NextFreeDate().Year, NextFreeDate().Month, NextFreeDate().Day, 10, 0, 0, TimeSpan.Zero);
        var workOrder = await CreateWorkOrderAsync(startAt);

        _timeProvider.SetUtcNow(startAt.AddMinutes(-5));

        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenTransitionIsInvalid_ReturnsInvalidStateTransitionError()
    {
        // Given
        var startAt = new DateTimeOffset(NextFreeDate().Year, NextFreeDate().Month, NextFreeDate().Day, 10, 0, 0, TimeSpan.Zero);
        var workOrder = await CreateWorkOrderAsync(startAt);

        _timeProvider.SetUtcNow(startAt.AddMinutes(5));

        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Completed);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenCompletingWorkOrder_RaisesWorkOrderCompletedDomainEventAndPersists()
    {
        // Given
        var startAt = new DateTimeOffset(NextFreeDate().Year, NextFreeDate().Month, NextFreeDate().Day, 10, 0, 0, TimeSpan.Zero);
        var workOrder = await CreateWorkOrderAsync(startAt);

        _timeProvider.SetUtcNow(startAt.AddMinutes(5));
        await _sender.Send(new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.InProgress));

        var command = new UpdateWorkOrderStateCommand(workOrder.Id, WorkOrderState.Completed);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);

        var saved = await _context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Equal(WorkOrderState.Completed, saved.State);
    }

    private async Task<WorkOrder> CreateWorkOrderAsync(DateTimeOffset startAt)
    {
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

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return workOrder;
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}