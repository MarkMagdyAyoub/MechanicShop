using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateVehicleCommandValidatorTests
{
  private readonly CreateVehicleCommandValidator _validator;

  public CreateVehicleCommandValidatorTests()
  {
    _validator = new CreateVehicleCommandValidator();
  }

  [Fact]
  public void Constructor_WhenMakeIsEmpty_ReturnVehicleError()
  {
    var command = new CreateVehicleCommand(
      Make: "",
      Model: "Model S",
      LicensePlate: "ABC123",
      Year: 2020
    );
  
    // When
    var result = _validator.Validate(command);

    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVehicleCommand.Make));
  }

  [Fact]
  public void Constructor_WhenModelIsEmpty_ReturnVehicleError()
  {
    var command = new CreateVehicleCommand(
      Make: "Tesla",
      Model: "",
      LicensePlate: "ABC123",
      Year: 2020
    );
  
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors , e => e.PropertyName == nameof(CreateVehicleCommand.Model));
  }

  [Fact]
  public void Constructor_WhenLicensePlateIsEmpty_ReturnVehicleError()
  {
    var command = new CreateVehicleCommand(
      Make: "Tesla",
      Model: "Model S",
      LicensePlate: "",
      Year: 2020
    );
  
    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors , e => e.PropertyName == nameof(CreateVehicleCommand.LicensePlate));
  }
}