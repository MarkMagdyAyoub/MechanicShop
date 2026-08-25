using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.ValueObjects.EmailAddress;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateCustomerCommandHandlerTests(SubcutaneousTestAppFactory factory , ITestOutputHelper output) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();


    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsCustomerDto()
    {
      var command = CustomerFactory.CreateCommand();

      var result = await _sender.Send(command);

      Assert.True(result.IsSuccess);
      Assert.Equal(command.Name, result.Value.Name);
      Assert.Equal(command.Email!.Trim().ToLower(), result.Value.Email);
      Assert.Equal(command.PhoneNumber, result.Value.PhoneNumber);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_CustomerIsPersistedToDatabase()
    {
        var command = CustomerFactory.CreateCommand();

        await _sender.Send(command);
        var email = command.Email!.Trim().ToLower();

        var saved = await _context.Customers
            .SingleOrDefaultAsync(c => c.Email == EmailAddress.Create(email).Value);

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_VehiclesArePersistedToDatabase()
    {
      var command = CustomerFactory.CreateCommand();

      var result = await _sender.Send(command);

      Assert.True(result.IsSuccess);

      var licensePlates = command.Vehicles.Select(v => v.LicensePlate).ToList();
      var savedPlates = _context.Vehicles
          .Where(v => licensePlates.Contains(v.LicensePlate))
          .Select(v => v.LicensePlate)
          .ToList();

      Assert.Equal(licensePlates.Count, savedPlates.Count);

      Assert.All(licensePlates, plate => Assert.Contains(plate, savedPlates));
    }


    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsCustomerExistsError()
    {
      var existing = CustomerFactory.Create().Value;
      _context.Customers.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      // different vehicles so the plate check is not the one that fires
      var command = CustomerFactory.CreateCommand(email: existing.Email!.Value);

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
      Assert.Equal(CustomerErrors.CustomerExists.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenEmailDiffersOnlyByCase_ReturnsCustomerExistsError()
    {
      var existing = CustomerFactory.Create().Value; 
      _context.Customers.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var command = CustomerFactory.CreateCommand(email: existing.Email!.Value.ToUpper());

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
      Assert.Equal(CustomerErrors.CustomerExists.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_NothingIsPersistedToDatabase()
    {
      var existing = CustomerFactory.Create().Value;
      _context.Customers.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var beforeCount = _context.Customers.Count();
      var command = CustomerFactory.CreateCommand(email: existing.Email!.Value);
      await _sender.Send(command);

      Assert.Equal(beforeCount, _context.Customers.Count());
    }

    [Fact]
    public async Task Handle_WhenLicensePlateAlreadyExists_ReturnsUniqueLicensePlateRequiredError()
    {
      var existing = CustomerFactory.Create().Value;
      _context.Customers.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var duplicatePlate = existing.Vehicles.First().LicensePlate;
      var command = CustomerFactory.CreateCommand(
          email: "another-email@example.com",
          vehicles: [VehicleFactory.CreateCommand(licensePlate: duplicatePlate)]);

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
      Assert.Equal(VehicleErrors.UniqueLicensePlateRequired.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenLicensePlateAlreadyExists_NothingIsPersistedToDatabase()
    {
      var existing = CustomerFactory.Create().Value;
      _context.Customers.Add(existing);
      await _context.SaveChangesAsync(CancellationToken.None);

      var duplicatePlate = existing.Vehicles.First().LicensePlate;
      var beforeCount = _context.Customers.Count();

      var command = CustomerFactory.CreateCommand(
          email: "unique-email@example.com",
          vehicles: [VehicleFactory.CreateCommand(licensePlate: duplicatePlate)]);

      await _sender.Send(command);

      Assert.Equal(beforeCount, _context.Customers.Count());
    }

    [Fact]
    public async Task Handle_WhenVehicleDataIsInvalid_ReturnsDomainErrors()
    {
      var command = CustomerFactory.CreateCommand(
          vehicles: [VehicleFactory.CreateCommand(licensePlate: string.Empty)]);

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCustomerDataIsInvalid_ReturnsDomainErrors()
    {
      var command = CustomerFactory.CreateCommand(name: string.Empty);

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
    }

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => factory.ResetDatabaseAsync();
}
