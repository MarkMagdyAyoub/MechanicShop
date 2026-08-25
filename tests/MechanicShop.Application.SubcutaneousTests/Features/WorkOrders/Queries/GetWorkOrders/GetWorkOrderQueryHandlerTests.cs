using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
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

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetWorkOrdersQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
    public async Task Handle_WhenWorkOrdersExist_ReturnsPaginatedResults()
    {
        // Given
        var (workOrder, vehicle, _) = await CreateWorkOrderWithFixturesAsync();

        var query = new GetWorkOrdersQuery(
            PageNumber: 1,
            PageSize: 50,
            SearchTerm: null,
            VehicleId: vehicle.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrder.Id);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(50, result.Value.PageSize);
    }

    [Fact]
    public async Task Handle_WhenFilteredByState_OnlyReturnsMatchingState()
    {
        // Given
        var scheduled = await CreateWorkOrderAsync(WorkOrderState.Scheduled);
        var cancelled = await CreateWorkOrderAsync(WorkOrderState.Cancelled);

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: null, State: WorkOrderState.Cancelled);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == cancelled.Id);
        Assert.DoesNotContain(result.Value.Items!, wo => wo.WorkOrderId == scheduled.Id);
    }

    [Fact]
    public async Task Handle_WhenFilteredByVehicleId_OnlyReturnsMatchingVehicle()
    {
        // Given
        var (workOrder1, vehicle1, _) = await CreateWorkOrderWithFixturesAsync();
        var (workOrder2, _, _) = await CreateWorkOrderWithFixturesAsync();

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: null, VehicleId: vehicle1.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrder1.Id);
        Assert.DoesNotContain(result.Value.Items!, wo => wo.WorkOrderId == workOrder2.Id);
    }

    [Fact]
    public async Task Handle_WhenFilteredByLaborId_OnlyReturnsMatchingLabor()
    {
        // Given
        var (workOrder1, _, employee1) = await CreateWorkOrderWithFixturesAsync();
        var (workOrder2, _, _) = await CreateWorkOrderWithFixturesAsync();

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: null, LaborId: employee1.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrder1.Id);
        Assert.DoesNotContain(result.Value.Items!, wo => wo.WorkOrderId == workOrder2.Id);
    }

    [Fact]
    public async Task Handle_WhenFilteredBySpot_OnlyReturnsMatchingSpot()
    {
        // Given
        var workOrderA = await CreateWorkOrderAsync(spot: Spot.A);
        var workOrderB = await CreateWorkOrderAsync(spot: Spot.B);

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: null, Spot: Spot.B);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrderB.Id);
        Assert.DoesNotContain(result.Value.Items!, wo => wo.WorkOrderId == workOrderA.Id);
    }

    [Fact]
    public async Task Handle_WhenSearchTermMatchesVehicleLicensePlate_ReturnsMatch()
    {
        // Given
        var uniquePlate = $"SRCH{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var workOrder = await CreateWorkOrderAsync(licensePlate: uniquePlate);

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: uniquePlate);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrder.Id);
    }

    [Fact]
    public async Task Handle_WhenSearchTermMatchesRepairTaskName_ReturnsMatch()
    {
        // Given
        var uniqueTaskName = $"Search Task {Guid.NewGuid()}";
        var workOrder = await CreateWorkOrderAsync(repairTaskName: uniqueTaskName);

        var query = new GetWorkOrdersQuery(PageNumber: 1, PageSize: 50, SearchTerm: uniqueTaskName);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.Items!, wo => wo.WorkOrderId == workOrder.Id);
    }

    [Fact]
    public async Task Handle_WhenNoWorkOrdersMatchFilter_ReturnsEmptyItemsWithZeroCount()
    {
        // Given
        var query = new GetWorkOrdersQuery(
            PageNumber: 1,
            PageSize: 50,
            SearchTerm: null,
            VehicleId: Guid.NewGuid());

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items!);
        Assert.Equal(0, result.Value.TotalCount);
    }

    // This test documents a real bug rather than working around it: ApplySorting
    // lowercases the incoming SearchColumn before switching on it, but every case
    // label is mixed-case ("StartAt", "State", etc.), so the switch can never match
    // anything except the default branch. Requesting SortColumn "StartAt" silently
    // falls back to CreatedAt ordering instead. This test is EXPECTED TO FAIL until
    // ApplySorting's case labels are lowercased to match searchColumn.ToLower().
    [Fact]
    public async Task Handle_WhenSortedByStartAt_ResultsAreOrderedByStartAtUtc()
    {
        // Given
        var date = NextFreeDate();
        var earlier = await CreateWorkOrderAsync(
            spot: Spot.A,
            startAt: new DateTimeOffset(date.Year, date.Month, date.Day, 9, 0, 0, TimeSpan.Zero));
        var later = await CreateWorkOrderAsync(
            spot: Spot.B,
            startAt: new DateTimeOffset(date.Year, date.Month, date.Day, 11, 0, 0, TimeSpan.Zero));

        var query = new GetWorkOrdersQuery(
            PageNumber: 1,
            PageSize: 50,
            SearchTerm: null,
            SearchColumn: "StartAt",
            SortDirection: "ASC");

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        var earlierIndex = result.Value.Items!.ToList().FindIndex(wo => wo.WorkOrderId == earlier.Id);
        var laterIndex = result.Value.Items!.ToList().FindIndex(wo => wo.WorkOrderId == later.Id);

        Assert.True(earlierIndex < laterIndex, "Expected results sorted by StartAtUtc ascending — see bug note above ApplySorting's case-matching.");
    }

    private async Task<WorkOrder> CreateWorkOrderAsync(
        WorkOrderState state = WorkOrderState.Scheduled,
        Spot spot = Spot.A,
        DateTimeOffset? startAt = null,
        string? licensePlate = null,
        string? repairTaskName = null)
    {
        var (workOrder, _, _) = await CreateWorkOrderWithFixturesAsync(state, spot, startAt, licensePlate, repairTaskName);
        return workOrder;
    }

    private async Task<(WorkOrder WorkOrder, Domain.Customers.Vehicles.Vehicle Vehicle, Domain.Employees.Employee Employee)>
        CreateWorkOrderWithFixturesAsync(
            WorkOrderState state = WorkOrderState.Scheduled,
            Spot spot = Spot.A,
            DateTimeOffset? startAt = null,
            string? licensePlate = null,
            string? repairTaskName = null)
    {
        var date = NextFreeDate();
        var actualStartAt = startAt ?? new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var customer = CustomerFactory.Create(
            email: $"customer-{Guid.NewGuid()}@example.com",
            vehicles: [VehicleFactory.Create(licensePlate: licensePlate ?? Guid.NewGuid().ToString("N")[..8].ToUpper()).Value]
        ).Value;

        var employee = EmployeeFactory.Create(role: Role.Labor).Value;

        var repairTask = RepairTaskFactory.Create(
            name: repairTaskName ?? $"Repair Task {Guid.NewGuid()}",
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

        if (state == WorkOrderState.Cancelled)
        {
            workOrder.UpdateState(WorkOrderState.Cancelled);
        }

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (workOrder, vehicle, employee);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}