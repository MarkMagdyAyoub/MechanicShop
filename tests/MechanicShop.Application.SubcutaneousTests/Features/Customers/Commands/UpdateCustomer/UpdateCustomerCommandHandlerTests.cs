using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateCustomerCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsUpdatedResult()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var existingVehicleId = existing.Vehicles.First().Id;

        var command = CustomerFactory.UpdateCommand(
            existing.Id,
            vehicles: [VehicleFactory.CreateUpdateCommand(vehicleId: existingVehicleId)]);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_CustomerDetailsArePersisted()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = CustomerFactory.UpdateCommand(
            existing.Id,
            name: "Updated Name",
            email: "updated-email@example.com",
            phoneNumber: "01250981475",
            vehicles: [VehicleFactory.CreateUpdateCommand(vehicleId: existing.Vehicles.First().Id)]);

        await _sender.Send(command);

        var updated = await _context.Customers
            .AsNoTracking()
            .SingleAsync(c => c.Id == existing.Id);

        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Email!.Trim().ToLower(), updated.Email!.Value);
        Assert.Equal(command.PhoneNumber, updated.PhoneNumber!.Value);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_VehiclesAreUpsertedInDatabase()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = CustomerFactory.UpdateCommand(
            existing.Id,
            vehicles: [VehicleFactory.CreateUpdateCommand(vehicleId: existing.Vehicles.First().Id)]);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);

        var licensePlates = command.Vehicles.Select(v => v.LicensePlate).ToList();
        var savedPlates = await _context.Vehicles
            .Where(v => licensePlates.Contains(v.LicensePlate))
            .Select(v => v.LicensePlate)
            .ToListAsync();

        Assert.Equal(licensePlates.Count, savedPlates.Count);
        Assert.All(licensePlates, plate => Assert.Contains(plate, savedPlates));
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsCustomerNotFoundError()
    {
        var command = CustomerFactory.UpdateCommand(Guid.NewGuid());

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_NothingIsPersisted()
    {
        var beforeCount = _context.Customers.Count();

        var command = CustomerFactory.UpdateCommand(Guid.NewGuid());

        await _sender.Send(command);

        Assert.Equal(beforeCount, _context.Customers.Count());
    }

    [Fact]
    public async Task Handle_WhenCustomerDataIsInvalid_ReturnsDomainErrors()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = CustomerFactory.UpdateCommand(existing.Id, name: string.Empty);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors , e => e.Code == nameof(UpdateCustomerCommand.Name));
    }

    [Fact]
    public async Task Handle_WhenVehicleDataIsInvalid_ReturnsDomainErrors()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = CustomerFactory.UpdateCommand(
            existing.Id,
            vehicles: [VehicleFactory.CreateUpdateCommand(licensePlate: string.Empty)]);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
    }

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => factory.ResetDatabaseAsync();
}
