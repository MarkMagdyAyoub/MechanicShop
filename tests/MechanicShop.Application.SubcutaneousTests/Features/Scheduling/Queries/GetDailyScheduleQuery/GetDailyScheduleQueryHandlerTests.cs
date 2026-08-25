using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.Extensions.Time.Testing;

namespace MechanicShop.Application.SubcutaneousTests.Features.Scheduling.Queries.GetDailySchedule;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetDailyScheduleQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();
    private readonly FakeTimeProvider _timeProvider = factory.GetFakeTimeProvider();

    // Monotonically increasing so no two tests (or runs within the same process)
    // ever ask the clock to move backward — FakeTimeProvider forbids that.
    private static long _dayOffsetCounter = 0;

    private static DateOnly NextFreeDate()
    {
        var offset = Interlocked.Increment(ref _dayOffsetCounter);
        // +1 year base keeps us clear of the seeder's "tomorrow..next month" window;
        // +offset days keeps every test on its own untouched date, ever-increasing.
        return DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).AddDays(offset));
    }

    [Fact]
    public async Task Handle_WhenNoWorkOrdersExistOnDate_ReturnsAllFreeSegments()
    {
        // Given
        var date = NextFreeDate();
        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, date, null);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        foreach (var spot in result.Value.Spots)
        {
            Assert.All(spot.Segments, segment =>
            {
                Assert.False(segment.IsOccupied);
                Assert.Null(segment.WorkOrderId);
            });
        }
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExistsInRange_ReturnsOccupiedSegmentWithDetails()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);
        var endAt = startAt.AddHours(1);

        var (workOrder, employee, repairTask) = await CreateWorkOrderAsync(startAt, endAt, Spot.A);

        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, date, null);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        var spotA = result.Value.Spots.Single(s => s.Spot == Spot.A);
        var occupiedSegment = spotA.Segments.Single(s => s.WorkOrderId == workOrder.Id);

        Assert.True(occupiedSegment.IsOccupied);
        Assert.False(occupiedSegment.IsAvailable);
        Assert.Equal(WorkOrderState.Scheduled, occupiedSegment.WorkOrderState);
        Assert.True(occupiedSegment.WorkOrderLocked);
        Assert.Equal(employee.Id, occupiedSegment.Labor!.Id);
        Assert.Contains(occupiedSegment.RepairTasks!, rt => rt.Id == repairTask.Id);
    }

    [Fact]
    public async Task Handle_WhenLaborIdProvided_OnlyReturnsThatLaborsWorkOrder()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);
        var endAt = startAt.AddHours(1);

        var (workOrderA, employeeA, _) = await CreateWorkOrderAsync(startAt, endAt, Spot.A);
        var (workOrderB, _, _) = await CreateWorkOrderAsync(startAt, endAt, Spot.B);

        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, date, employeeA.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        var spotA = result.Value.Spots.Single(s => s.Spot == Spot.A);
        Assert.Contains(spotA.Segments, s => s.WorkOrderId == workOrderA.Id && s.IsOccupied);

        var spotB = result.Value.Spots.Single(s => s.Spot == Spot.B);
        Assert.All(spotB.Segments, s =>
        {
            Assert.False(s.IsOccupied);
            Assert.NotEqual(workOrderB.Id, s.WorkOrderId);
        });
    }

    [Fact]
    public async Task Handle_WhenDateIsInPast_ReturnsIsPastDateTrue()
    {
        // Given
        var date = NextFreeDate();
        var fakeNow = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(fakeNow);

        var pastDate = DateOnly.FromDateTime(date.ToDateTime(TimeOnly.MinValue).AddDays(-1));

        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, pastDate, null);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsPastDate);
    }

    [Fact]
    public async Task Handle_WhenDateIsToday_ReturnsIsPastDateFalse()
    {
        // Given
        var date = NextFreeDate();
        var fakeNow = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(fakeNow);

        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, date, null);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsPastDate);
    }

    [Fact]
    public async Task Handle_WhenSegmentTimeIsBeforeNow_IsAvailableIsFalse()
    {
        // Given
        var date = NextFreeDate();
        var fakeNow = new DateTimeOffset(date.Year, date.Month, date.Day, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.SetUtcNow(fakeNow);

        var query = new GetDailyScheduleQuery(TimeZoneInfo.Utc, date, null);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        var spotA = result.Value.Spots.Single(s => s.Spot == Spot.A);

        var beforeNow = spotA.Segments.Where(s => s.EndAt <= fakeNow).ToList();
        var afterNow = spotA.Segments.Where(s => s.StartAt >= fakeNow).ToList();

        Assert.NotEmpty(beforeNow);
        Assert.NotEmpty(afterNow);
        Assert.All(beforeNow, s => Assert.False(s.IsAvailable));
        Assert.All(afterNow, s => Assert.True(s.IsAvailable));
    }

    private async Task<(WorkOrder WorkOrder, Employee Employee, RepairTask RepairTask)> CreateWorkOrderAsync(
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Spot spot)
    {
        var customer = CustomerFactory.Create(
            email: $"customer-{Guid.NewGuid()}@example.com",
            vehicles: [VehicleFactory.Create(licensePlate: Guid.NewGuid().ToString("N")[..6].ToUpper()).Value]
        ).Value;

        var employee = EmployeeFactory.Create(role: Role.Labor).Value;

        var repairTask = RepairTaskFactory.Create(name: $"Repair Task {Guid.NewGuid()}").Value;

        _context.Customers.Add(customer);
        _context.Employees.Add(employee);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        var vehicle = customer.Vehicles.First();

        var workOrder = WorkOrder.Create(
            Guid.NewGuid(),
            vehicle.Id,
            startAt,
            endAt,
            employee.Id,
            spot,
            [repairTask]
        ).Value;

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (workOrder, employee, repairTask);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => factory.ResetDatabaseAsync();
}