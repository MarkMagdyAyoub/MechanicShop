using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
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

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
    public async Task Handle_WhenValidCommand_ReturnsUpdatedResult()
    {
        // Given
        var (workOrder, startAt) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, Spot.A);
        var newStartAt = startAt.AddHours(2);

        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_TimingAndSpotAreUpdatedInDatabase()
    {
        // Given
        var (workOrder, startAt) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, Spot.A);
        var newStartAt = startAt.AddHours(2);
        var expectedEndAt = newStartAt.Add(workOrder.EndAtUtc - workOrder.StartAtUtc);

        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // When
        await _sender.Send(command);

        // Then
        var saved = await _context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Equal(Spot.B, saved.Spot);
        Assert.Equal(newStartAt, saved.StartAtUtc);
        Assert.Equal(expectedEndAt, saved.EndAtUtc);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), Spot.A);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenNewSpotIsOccupied_ReturnsSpotAvailabilityError()
    {
        // Given
        var (workOrderToMove, startAt) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, Spot.A);
        var (_, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, Spot.B, startAt); // occupies Spot.B at the same time

        var command = new RelocateWorkOrderCommand(workOrderToMove.Id, startAt, Spot.B);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.SpotAvailability(startAt, startAt.AddMinutes(60)).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenLaborIsOccupiedAtNewTime_ReturnsLaborOccupiedError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);
        var laterStartAt = startAt.AddHours(3);

        var (workOrderToMove, _, labor) = await CreateWorkOrderWithLaborAsync(WorkOrderState.Scheduled, Spot.A, startAt);

        // same labor, different work order, occupying the target time slot
        await CreateWorkOrderForLaborAsync(labor, Spot.C, laterStartAt);

        var command = new RelocateWorkOrderCommand(workOrderToMove.Id, laterStartAt, Spot.D);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsNotEditable_ReturnsTimingReadonlyError()
    {
        // Given
        var (workOrder, startAt) = await CreateWorkOrderAsync(WorkOrderState.InProgress, Spot.A);
        var newStartAt = startAt.AddHours(2);

        var command = new RelocateWorkOrderCommand(workOrder.Id, newStartAt, Spot.B);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.TimingReadonly(workOrder.Id.ToString(), workOrder.State).Code, result.TopError.Code);
    }

    private async Task<(WorkOrder WorkOrder, DateTimeOffset StartAt)> CreateWorkOrderAsync(
        WorkOrderState targetState,
        Spot spot,
        DateTimeOffset? startAt = null)
    {
        var (workOrder, actualStartAt, _) = await CreateWorkOrderWithLaborAsync(targetState, spot, startAt);
        return (workOrder, actualStartAt);
    }

    private async Task<(WorkOrder WorkOrder, DateTimeOffset StartAt, Employee Labor)> CreateWorkOrderWithLaborAsync(
        WorkOrderState targetState,
        Spot spot,
        DateTimeOffset? startAt = null)
    {
        var date = NextFreeDate();
        var actualStartAt = startAt ?? new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

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
        var endAt = actualStartAt.AddMinutes((int)repairTask.EstimatedDurationInMins);

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicle.Id,
            actualStartAt,
            endAt,
            employee.Id,
            spot,
            [repairTask]
        ).Value;

        ApplyStateTransitions(workOrder, targetState);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (workOrder, actualStartAt, employee);
    }

    private async Task<WorkOrder> CreateWorkOrderForLaborAsync(Employee labor, Spot spot, DateTimeOffset startAt)
    {
        var customer = CustomerFactory.Create(
            email: $"customer-{Guid.NewGuid()}@example.com",
            vehicles: [VehicleFactory.Create(licensePlate: Guid.NewGuid().ToString("N")[..8].ToUpper()).Value]
        ).Value;

        var repairTask = RepairTaskFactory.Create(
            name: $"Repair Task {Guid.NewGuid()}",
            estimatedDurationInMins: RepairDurationInMinutes._60
        ).Value;

        _context.Customers.Add(customer);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        var vehicle = customer.Vehicles.First();
        var endAt = startAt.AddMinutes((int)repairTask.EstimatedDurationInMins);

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicle.Id,
            startAt,
            endAt,
            labor.Id,
            spot,
            [repairTask]
        ).Value;

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