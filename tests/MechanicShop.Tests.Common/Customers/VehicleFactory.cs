namespace MechanicShop.Tests.Common.VehicleGenerator;

using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
public static class VehicleFactory
{
  public static Result<Vehicle> Create(
      Guid? id = null,
      string? make = null,
      string? model = null,
      int? year = null,
      string? licensePlate = null
  )
  {
    return Vehicle.Create(
      id ?? Guid.NewGuid(),
      make ?? "Toyota",
      model ?? "Corolla",
      year ?? 2022,
      licensePlate ?? "Cairo 1234 ABC"
    );
  }
}