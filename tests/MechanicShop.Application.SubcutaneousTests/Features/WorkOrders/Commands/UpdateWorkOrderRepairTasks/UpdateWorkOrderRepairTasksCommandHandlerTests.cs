using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._60);
        var newRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._30);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [newRepairTask.Id]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_RepairTasksAreUpdatedInDatabase()
    {
        // Given
        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._60);
        var newRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._30);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [newRepairTask.Id]);

        // When
        await _sender.Send(command);

        // Then
        var saved = await _context.WorkOrders
            .AsNoTracking()
            .Include(wo => wo.RepairTasks)
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Single(saved.RepairTasks);
        Assert.Equal(newRepairTask.Id, saved.RepairTasks.Single().Id);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_EndTimeIsRecalculated()
    {
        // Given
        var (workOrder, startAt, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._60);
        var newRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._30);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [newRepairTask.Id]);

        // When
        await _sender.Send(command);

        // Then
        var saved = await _context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Equal(startAt.AddMinutes(30), saved.EndAtUtc);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskIdIsMissing_ReturnsRepairTaskMissingError()
    {
        // Given
        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._60);
        var missingRepairTaskId = Guid.NewGuid();

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [missingRepairTaskId]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskMissing([missingRepairTaskId]).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsNotEditable_ReturnsReadonlyError()
    {
        // Given
        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.InProgress, RepairDurationInMinutes._60);
        var newRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._30);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [newRepairTask.Id]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenNewDurationPushesOutsideWorkingHours_ReturnsWorkOrderOperatingHourError()
    {
        // Given
        var date = NextFreeDate();
        var lateStartAt = new DateTimeOffset(date.Year, date.Month, date.Day, 17, 30, 0, TimeSpan.Zero); // 17:30
        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._30, lateStartAt);

        // extending to 90 minutes pushes end to 19:00, past the 18:00 close
        var longRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._90);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [longRepairTask.Id]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOperatingHour(lateStartAt, lateStartAt.AddMinutes(90)).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenNewDurationCausesOverlapWithAnotherWorkOrder_ReturnsWorkOrderOverlappingError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (workOrder, _, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._30, startAt, Spot.A);

        // second work order in the same spot, starting right after the first one's original (30-min) end time
        var conflictingStartAt = startAt.AddMinutes(30);
        await CreateWorkOrderAsync(WorkOrderState.Scheduled, RepairDurationInMinutes._30, conflictingStartAt, Spot.A);

        // extending the first work order to 90 minutes now overlaps the second one
        var longRepairTask = await CreateStandaloneRepairTaskAsync(RepairDurationInMinutes._90);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrder.Id, [longRepairTask.Id]);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOverlapping.Code, result.TopError.Code);
    }

    private async Task<RepairTask> CreateStandaloneRepairTaskAsync(RepairDurationInMinutes duration)
    {
        var repairTask = RepairTaskFactory.Create(
            name: $"Repair Task {Guid.NewGuid()}",
            estimatedDurationInMins: duration
        ).Value;

        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        return repairTask;
    }

    private async Task<(WorkOrder WorkOrder, DateTimeOffset StartAt, RepairTask RepairTask)> CreateWorkOrderAsync(
        WorkOrderState targetState,
        RepairDurationInMinutes duration,
        DateTimeOffset? startAt = null,
        Spot spot = Spot.A)
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
            estimatedDurationInMins: duration
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

        return (workOrder, actualStartAt, repairTask);
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