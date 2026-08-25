using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;
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

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.ReassignLabor;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class ReassignLaborCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
        var (workOrder, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var newLabor = EmployeeFactory.Create(role: Role.Labor).Value;
        _context.Employees.Add(newLabor);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReassignLaborCommand(workOrder.Id, newLabor.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_LaborIsUpdatedInDatabase()
    {
        // Given
        var (workOrder, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var newLabor = EmployeeFactory.Create(role: Role.Labor).Value;
        _context.Employees.Add(newLabor);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReassignLaborCommand(workOrder.Id, newLabor.Id);

        // When
        await _sender.Send(command);

        // Then
        var saved = await _context.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(saved);
        Assert.Equal(newLabor.Id, saved.LaborId);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var newLabor = EmployeeFactory.Create(role: Role.Labor).Value;
        _context.Employees.Add(newLabor);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReassignLaborCommand(Guid.NewGuid(), newLabor.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenLaborIdIsNotALaborRole_ReturnsLaborNotFoundError()
    {
        // Given
        var (workOrder, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var manager = EmployeeFactory.Create(role: Role.Manager).Value;
        _context.Employees.Add(manager);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReassignLaborCommand(workOrder.Id, manager.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenNewLaborIsAlreadyOccupied_ReturnsLaborOccupiedError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (workOrderToReassign, _) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, startAt, Spot.A);

        // a second, unrelated work order occupying the same time window with a different labor
        var (otherWorkOrder, otherLabor) = await CreateWorkOrderAsync(WorkOrderState.Scheduled, startAt, Spot.B);

        var command = new ReassignLaborCommand(workOrderToReassign.Id, otherLabor.Id);

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
        var (workOrder, _) = await CreateWorkOrderAsync(WorkOrderState.InProgress);
        var newLabor = EmployeeFactory.Create(role: Role.Labor).Value;
        _context.Employees.Add(newLabor);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ReassignLaborCommand(workOrder.Id, newLabor.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.TimingReadonly(workOrder.Id.ToString(), workOrder.State).Code, result.TopError.Code);
    }

    private async Task<(WorkOrder WorkOrder, Employee Employee)> CreateWorkOrderAsync(
        WorkOrderState targetState,
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

        return (workOrder, employee);
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