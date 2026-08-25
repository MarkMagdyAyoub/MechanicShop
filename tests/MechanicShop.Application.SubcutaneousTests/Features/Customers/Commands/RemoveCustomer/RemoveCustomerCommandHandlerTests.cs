using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.EmployeeGenerator;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.RemoveCustomer;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RemoveCustomerCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
  private readonly IAppDbContext _context = factory.CreateDbContext();
  private readonly ISender _sender = factory.CreateSender();


  [Fact]
  public async Task Handle_WhenCustomerExists_ReturnsDeleted()
  {
    var created = await _sender.Send(CustomerFactory.CreateCommand());

    var result = await _sender.Send(new RemoveCustomerCommand(created.Value.CustomerId));

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_WhenCustomerExists_CustomerIsRemovedFromDatabase()
  {
    var created = await _sender.Send(CustomerFactory.CreateCommand());

    await _sender.Send(new RemoveCustomerCommand(created.Value.CustomerId));

    var stillExists = _context.Customers.Any(c => c.Id == created.Value.CustomerId);
    Assert.False(stillExists);
  }

  [Fact]
  public async Task Handle_WhenCustomerExists_VehiclesAreRemovedFromDatabase()
  {
    var created = await _sender.Send(CustomerFactory.CreateCommand());

    await _sender.Send(new RemoveCustomerCommand(created.Value.CustomerId));

    var orphanedVehicles = _context.Vehicles.Any(v => v.CustomerId == created.Value.CustomerId);
    Assert.False(orphanedVehicles);
  }

  [Fact]
  public async Task Handle_WhenCustomerRemoved_OtherCustomersAreNotAffected()
  {
    var toRemove = await _sender.Send(CustomerFactory.CreateRandomCommand(email: "alice@gmail.com"));
    var toKeep   = await _sender.Send(CustomerFactory.CreateRandomCommand(email: "bob@gmail.com"));

    await _sender.Send(new RemoveCustomerCommand(toRemove.Value.CustomerId));

    Assert.True(_context.Customers.Any(c => c.Id == toKeep.Value.CustomerId));
  }

  [Fact]
  public async Task Handle_WhenCustomerDoesNotExist_ReturnsCustomerNotFoundError()
  {
    var command = new RemoveCustomerCommand(Guid.NewGuid());

    var result = await _sender.Send(command);

    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenCustomerDoesNotExist_NothingIsRemovedFromDatabase()
  {
    await _sender.Send(CustomerFactory.CreateCommand());
    var beforeCount = _context.Customers.Count();

    await _sender.Send(new RemoveCustomerCommand(Guid.NewGuid()));

    Assert.Equal(beforeCount, _context.Customers.Count());
  }

  [Fact]
  public async Task Handle_WhenCustomerHasWorkOrders_ReturnsCannotDeleteError()
  {
    var created = await _sender.Send(CustomerFactory.CreateCommand());

    var vehicleId = _context.Vehicles
        .First(v => v.CustomerId == created.Value.CustomerId)
        .Id;

    var labor = EmployeeFactory.Create().Value;

    _context.Employees.Add(labor);
    await _context.SaveChangesAsync(CancellationToken.None);

    var workOrder = WorkOrderFactory.Create(vehicleId: vehicleId , laborId: labor.Id).Value;

    _context.WorkOrders.Add(workOrder);
    await _context.SaveChangesAsync(CancellationToken.None);

    var result = await _sender.Send(new RemoveCustomerCommand(created.Value.CustomerId));

    Assert.False(result.IsSuccess);
    Assert.Equal(CustomerErrors.CannotDeleteCustomerWithWorkOrders.Code, result.TopError.Code);
  }

  [Fact]
  public async Task Handle_WhenCustomerHasWorkOrders_CustomerIsNotRemovedFromDatabase()
  {
    var created = await _sender.Send(CustomerFactory.CreateCommand());

    var vehicleId = _context.Vehicles
        .First(v => v.CustomerId == created.Value.CustomerId)
        .Id;

    var labor = EmployeeFactory.Create().Value;

    _context.Employees.Add(labor);
    await _context.SaveChangesAsync(CancellationToken.None);

    var workOrder = WorkOrderFactory.Create(vehicleId: vehicleId , laborId: labor.Id).Value;
    _context.WorkOrders.Add(workOrder);
    await _context.SaveChangesAsync(CancellationToken.None);

    await _sender.Send(new RemoveCustomerCommand(created.Value.CustomerId));

    Assert.True(_context.Customers.Any(c => c.Id == created.Value.CustomerId));
  }


  public Task InitializeAsync() => factory.ResetDatabaseAsync();

  public Task DisposeAsync() => factory.ResetDatabaseAsync();
}
