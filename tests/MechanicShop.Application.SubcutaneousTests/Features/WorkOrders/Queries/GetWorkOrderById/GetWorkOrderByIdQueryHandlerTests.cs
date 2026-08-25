using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
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

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
    public async Task Handle_WhenWorkOrderExists_ReturnsWorkOrderDto()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync();

        var query = new GetWorkOrderByIdQuery(workOrder.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
        Assert.Equal(workOrder.Spot, result.Value.Spot);
        Assert.Equal(workOrder.State, result.Value.State);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ReturnsAssociatedRepairTasks()
    {
        // Given
        var workOrder = await CreateWorkOrderAsync();

        var query = new GetWorkOrderByIdQuery(workOrder.Id);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);

        var expectedRepairTaskIds = workOrder.RepairTasks.Select(rt => rt.Id).ToList();
        var returnedRepairTaskIds = result.Value.RepairTasks.Select(rt => rt.Id).ToList();

        Assert.Equal(expectedRepairTaskIds.Count, returnedRepairTaskIds.Count);
        Assert.All(expectedRepairTaskIds, id => Assert.Contains(id, returnedRepairTaskIds));
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
        // Given
        var query = new GetWorkOrderByIdQuery(Guid.NewGuid());

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    private async Task<WorkOrder> CreateWorkOrderAsync()
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

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return workOrder;
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}