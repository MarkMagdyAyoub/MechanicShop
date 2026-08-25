using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
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
    public async Task Handle_WhenValidCommand_ReturnsWorkOrderDto()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [repairTask.Id],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(vehicle.Id, result.Value.Vehicle!.Id);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_WorkOrderIsPersistedToDatabase()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [repairTask.Id],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);

        var saved = await _context.WorkOrders.FindAsync(result.Value.WorkOrderId);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskIdIsMissing_ReturnsRepairTaskMissingError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (_, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);
        var missingRepairTaskId = Guid.NewGuid();

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [missingRepairTaskId],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskMissing([missingRepairTaskId]).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenLaborIdIsNotALaborRole_ReturnsLaborNotFoundError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, _, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);
        var manager = EmployeeFactory.Create(role: Role.Manager).Value;
        _context.Employees.Add(manager);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [repairTask.Id],
            manager.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenVehicleDoesNotExist_ReturnsVehicleNotFoundError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, _) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            Guid.NewGuid(),
            startAt,
            [repairTask.Id],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.VehicleNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenStartTimeIsBeforeOpeningHours_ReturnsWorkOrderOperatingHourError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 7, 0, 0, TimeSpan.Zero); // before 9:00

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [repairTask.Id],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOperatingHour(startAt, startAt.AddMinutes(60)).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenEndTimeIsAfterClosingHours_ReturnsWorkOrderOperatingHourError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 17, 30, 0, TimeSpan.Zero); // ends 18:30, after 18:00 close

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var command = new CreateWorkOrderCommand(
            Spot.A,
            vehicle.Id,
            startAt,
            [repairTask.Id],
            employee.Id);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOperatingHour(startAt, startAt.AddMinutes(60)).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenSpotIsAlreadyOccupied_ReturnsSpotAvailabilityError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var firstCommand = new CreateWorkOrderCommand(Spot.A, vehicle.Id, startAt, [repairTask.Id], employee.Id);
        var firstResult = await _sender.Send(firstCommand);
        Assert.True(firstResult.IsSuccess);

        // second attempt: different vehicle/labor, same spot, overlapping time
        var (repairTask2, employee2, vehicle2) = await CreateFixturesAsync(RepairDurationInMinutes._60);
        var secondCommand = new CreateWorkOrderCommand(Spot.A, vehicle2.Id, startAt, [repairTask2.Id], employee2.Id);

        // When
        var result = await _sender.Send(secondCommand);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.SpotAvailability(startAt, startAt.AddMinutes(60)).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenVehicleAlreadyUnderMaintenance_ReturnsVehicleAlreadyUnderMaintenanceError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var firstCommand = new CreateWorkOrderCommand(Spot.A, vehicle.Id, startAt, [repairTask.Id], employee.Id);
        var firstResult = await _sender.Send(firstCommand);
        Assert.True(firstResult.IsSuccess);

        // second attempt: same vehicle, different spot/labor, overlapping time
        var (repairTask2, employee2, _) = await CreateFixturesAsync(RepairDurationInMinutes._60);
        var secondCommand = new CreateWorkOrderCommand(Spot.B, vehicle.Id, startAt, [repairTask2.Id], employee2.Id);

        // When
        var result = await _sender.Send(secondCommand);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.VehicleAlreadyUnderMaintenance.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenLaborIsAlreadyOccupied_ReturnsLaborOccupiedError()
    {
        // Given
        var date = NextFreeDate();
        var startAt = new DateTimeOffset(date.Year, date.Month, date.Day, 10, 0, 0, TimeSpan.Zero);

        var (repairTask, employee, vehicle) = await CreateFixturesAsync(RepairDurationInMinutes._60);

        var firstCommand = new CreateWorkOrderCommand(Spot.A, vehicle.Id, startAt, [repairTask.Id], employee.Id);
        var firstResult = await _sender.Send(firstCommand);
        Assert.True(firstResult.IsSuccess);

        // second attempt: same labor, different spot/vehicle, overlapping time
        var (repairTask2, _, vehicle2) = await CreateFixturesAsync(RepairDurationInMinutes._60);
        var secondCommand = new CreateWorkOrderCommand(Spot.B, vehicle2.Id, startAt, [repairTask2.Id], employee.Id);

        // When
        var result = await _sender.Send(secondCommand);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborOccupied.Code, result.TopError.Code);
    }

    private async Task<(RepairTask RepairTask, Employee Employee, Vehicle Vehicle)>
        CreateFixturesAsync(RepairDurationInMinutes duration)
    {
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

        return (repairTask, employee, customer.Vehicles.First());
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}