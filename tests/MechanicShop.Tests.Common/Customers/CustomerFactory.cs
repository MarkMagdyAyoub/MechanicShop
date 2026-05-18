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
      vehicles ?? [VehicleFactory.Create().Value , VehicleFactory.Create().Value , VehicleFactory.Create().Value]
    );
  }
}