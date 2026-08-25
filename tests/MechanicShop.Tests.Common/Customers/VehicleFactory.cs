namespace MechanicShop.Tests.Common.VehicleGenerator;

using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
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
      licensePlate ?? "1234 ABC"
    );
  }

  public static CreateVehicleCommand CreateCommand(
    string? make = null,
    string? model = null,
    string? licensePlate = null,
    int? year = null
  )
  {
    return new CreateVehicleCommand(
      make ?? "Toyota",
      model?? "Corolla",
      licensePlate ?? "1234 ABC",
      year ?? 2022
    );
  }

  public static CreateVehicleCommand CreateRandomCommand(
    string? make         = null,
    string? model        = null,
    int?    year         = null,
    string? licensePlate = null
  )
  {
    return new CreateVehicleCommand(
      Make:         make         ?? "Toyota",
      Model:        model        ?? "Camry",
      Year:         year         ?? 2020,
      LicensePlate: licensePlate ?? Guid.NewGuid().ToString("N")[..8].ToUpper()
    );
  }

  public static UpdateVehicleCommand CreateUpdateCommand(
    Guid?   vehicleId    = null,
    string? make         = null,
    string? model        = null,
    int?    year         = null,
    string? licensePlate = null
  )
  {
    return new UpdateVehicleCommand(
        vehicleId,
        make ?? "Toyota",
        model ?? "Camry",
        licensePlate ?? "ABC123",
        year ?? 2020
    );
  }
}
