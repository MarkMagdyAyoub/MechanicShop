using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Application.Features.Customers.Mappers;
// Mapping From Domain Entity To DTO

public static class CustomerMapper
{
  public static CustomerDto ToDto(this Customer customer)
  {
    ArgumentNullException.ThrowIfNull(customer);

    return new CustomerDto
    {
      CustomerId = customer.Id,
      Email = customer.Email!,
      Name = customer.Name!,
      PhoneNumber = customer.PhoneNumber!,
      Vehicles = customer.Vehicles.Select(x => x.ToDto()).ToList() ?? []
    };
  }

  public static VehicleDto ToDto(this Vehicle vehicle)
  {
    ArgumentNullException.ThrowIfNull(vehicle);

    return new VehicleDto
    {
      Id = vehicle.Id,
      LicensePlate = vehicle.LicensePlate,
      Model = vehicle.Model,
      Year = vehicle.Year
    };
  }
  public static List<CustomerDto> ToDo(this IEnumerable<Customer> customers) => 
    customers.Select(c => c.ToDto()).ToList();

  public static List<VehicleDto> ToDo(this IEnumerable<Vehicle> vehicles) =>
    vehicles.Select(v => v.ToDto()).ToList();
}