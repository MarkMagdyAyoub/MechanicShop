using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.InvoiceGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStatistics;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatisticsQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    // Monotonically increasing so each test gets its own untouched date,
    // never colliding with the seeder's generated range or with each other.
    private static long _dayOffsetCounter = 0;

    private static DateOnly NextFreeDate()
    {
        var offset = Interlocked.Increment(ref _dayOffsetCounter);
        return DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).AddDays(offset));
    }

    [Fact]
    public async Task Handle_WhenNoWorkOrdersOnDate_ReturnsEmptyStatisticsWithDateSet()
    {
        // Given
        var date = NextFreeDate();
        var query = new GetWorkOrderStatisticsQuery(date);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(date, result.Value.Date);
        Assert.Equal(0, result.Value.TotalOrders);
        Assert.Equal(0, result.Value.Scheduled);
        Assert.Equal(0, result.Value.InProgress);
        Assert.Equal(0, result.Value.Completed);
        Assert.Equal(0, result.Value.Cancelled);
        Assert.Equal(0m, result.Value.TotalRevenue);
    }

    [Fact]
    public async Task Handle_WhenWorkOrdersExistWithDifferentStates_ReturnsCorrectCounts()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        await CreateWorkOrderAsync(startAt, Spot.A, WorkOrderState.Scheduled);
        await CreateWorkOrderAsync(startAt, Spot.B, WorkOrderState.InProgress);
        await CreateWorkOrderAsync(startAt, Spot.C, WorkOrderState.Completed);
        await CreateWorkOrderAsync(startAt, Spot.D, WorkOrderState.Cancelled);

        var query = new GetWorkOrderStatisticsQuery(date);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.TotalOrders);
        Assert.Equal(1, result.Value.Scheduled);
        Assert.Equal(1, result.Value.InProgress);
        Assert.Equal(1, result.Value.Completed);
        Assert.Equal(1, result.Value.Cancelled);
        Assert.Equal(4, result.Value.UniqueVehicles);
        Assert.Equal(4, result.Value.UniqueCustomers);
        Assert.Equal(25m, result.Value.CompletionRate);
    }

    [Fact]
    public async Task Handle_WhenInvoicesExist_ReturnsCorrectRevenueAndCostCalculations()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (workOrder, _, _) = await CreateWorkOrderAsync(startAt, Spot.A, WorkOrderState.Completed);

        var invoice = InvoiceFactory.Create(workOrderId: workOrder.Id).Value;
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(CancellationToken.None);

        var linkedWorkOrder = await _context.WorkOrders
            .AsNoTracking()
            .Include(wo => wo.Invoice)
            .SingleAsync(wo => wo.Id == workOrder.Id);

        Assert.NotNull(linkedWorkOrder.Invoice);
        Assert.Equal(invoice.Id, linkedWorkOrder.Invoice.Id);

        var query = new GetWorkOrderStatisticsQuery(date);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(invoice.Total, result.Value.TotalRevenue);
        Assert.Equal(workOrder.TotalPartsCost, result.Value.TotalPartsCost);
        Assert.Equal(workOrder.TotalLaborCost, result.Value.TotalLaborCost);
        Assert.Equal(invoice.Total - workOrder.TotalPartsCost - workOrder.TotalLaborCost, result.Value.NetProfit);
    }

    private async Task<(WorkOrder WorkOrder, Employee Employee, RepairTask RepairTask)> CreateWorkOrderAsync(
        DateTimeOffset startAt,
        Spot spot,
        WorkOrderState targetState)
    {
        var customer = CustomerFactory.Create(
            email: $"customer-{Guid.NewGuid()}@example.com",
            vehicles: [VehicleFactory.Create(licensePlate: Guid.NewGuid().ToString("N")[..8].ToUpper()).Value]
        ).Value;

        var employee = EmployeeFactory.Create(role: Role.Labor).Value;

        var repairTask = RepairTaskFactory.Create(name: $"Repair Task {Guid.NewGuid()}").Value;

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
            spot,
            [repairTask]
        ).Value;

        ApplyStateTransitions(workOrder, targetState);

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return (workOrder, employee, repairTask);
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