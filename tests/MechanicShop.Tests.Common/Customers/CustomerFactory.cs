using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.VehicleGenerator;
namespace MechanicShop.Tests.Common.CustomerGenerator;

public static class CustomerFactory{
  public static Result<Customer> Create(
    Guid? id = null,
    string? name = null,
    string? phoneNumber = null,
    string? email = null,
    List<Vehicle>? vehicles = null
  )
  {
    return Customer.Create(
      id ?? Guid.NewGuid(),
      name ?? "Mark",
      phoneNumber ?? "01250981475",
      email ?? "mark@gmail.com",
      vehicles ?? [VehicleFactory.Create().Value]
    );
  }

  public static CreateCustomerCommand CreateCommand(
    string? name = null,
    string? phoneNumber = null,
    string? email = null,
    List<CreateVehicleCommand>? vehicles = null
  )
  {
    return new CreateCustomerCommand(
        name ?? "Mark",
        phoneNumber ?? "01250981475",
        email ?? "mark@gmail.com",
        vehicles ?? [VehicleFactory.CreateCommand()]
    );
  }

  public static CreateCustomerCommand CreateRandomCommand(
    string? name        = null,
    string? email       = null,
    string? phoneNumber = null,
    List<CreateVehicleCommand>? vehicles = null
  )
  {
    return new CreateCustomerCommand(
        Name:        name        ?? $"Customer {Guid.NewGuid():N}",
        Email:       email       ?? $"{Guid.NewGuid():N}@example.com",
        PhoneNumber: phoneNumber ?? "01000000000",
        Vehicles:    vehicles    ?? [VehicleFactory.CreateRandomCommand()]
    );
  }

  public static UpdateCustomerCommand UpdateCommand(
    Guid customerId,
    string? name = null,
    string? email = null,
    string? phoneNumber = null,
    List<UpdateVehicleCommand>? vehicles = null
  )
    {
        return new UpdateCustomerCommand(
            customerId,
            name ?? "Updated Customer",
            phoneNumber ?? "01250981475",
            email ?? "mark@gmail.com",
            vehicles ?? [VehicleFactory.CreateUpdateCommand()]);
    }

    public static GetCustomerByIdQuery GetCustomerByIdQuery(
        Guid customerId
    )
    {
        return new GetCustomerByIdQuery(
            customerId
        );
    }
}
