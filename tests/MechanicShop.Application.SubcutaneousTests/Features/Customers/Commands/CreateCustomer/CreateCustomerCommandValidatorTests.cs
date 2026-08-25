using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Tests.Common.VehicleGenerator;
using NSubstitute;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidatorTests
{
  private readonly CreateCustomerCommandValidator _validator;

  public CreateCustomerCommandValidatorTests()
  {
    _validator = new CreateCustomerCommandValidator();
  }

  [Fact]
  public void Constructor_WhenNameIsEmpty_ShouldHaveValidationError()
  {
    var command = new CreateCustomerCommand(
      Name: "",
      Email: "mark@example.com",
      PhoneNumber: "01152846575",
      Vehicles: [VehicleFactory.CreateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Name));
  }

  [Theory]
  [InlineData("invalid-email")]
  [InlineData("invalid-email@")]
  [InlineData("invalid-email.com")]
  public void Constructor_WhenEmailIsInvalid_ShouldHaveValidationError(string email)
  {
    var command = new CreateCustomerCommand(
      Name: "John Doe",
      Email: email,
      PhoneNumber: "0123456789",
      Vehicles: [VehicleFactory.CreateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Email));
  }

  [Theory]
  [InlineData("johndoe@example.com")]
  [InlineData("jane.smith@example.com")]
  public void Constructor_WhenEmailIsValid_ShouldNotHaveValidationError(string email)
  {
    var command = new CreateCustomerCommand(
      Name: "John Doe",
      Email: email,
      PhoneNumber: "01210480491",
      Vehicles: [VehicleFactory.CreateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
    Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Email));
  }

  [Theory]
  [InlineData("invalid-phone-number")]
  [InlineData("123")]
  public void Constructor_WhenPhoneNumberIsInvalid_ShouldHaveValidationError(string phoneNumber)
  {
    var command = new CreateCustomerCommand(
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: phoneNumber,
      Vehicles: [VehicleFactory.CreateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.PhoneNumber));
  }

  [Theory]
  [InlineData("01512345678")]
  [InlineData("01210480491")]
  [InlineData("01123456782")]
  public void Constructor_WhenPhoneNumberIsValid_ShouldNotHaveValidationError(string phoneNumber)
  {
    var command = new CreateCustomerCommand(
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: phoneNumber,
      Vehicles: [VehicleFactory.CreateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
    Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.PhoneNumber));
  }

  [Fact]
  public void Constructor_WhenVehiclesListIsEmpty_ShouldHaveValidationError()
  {
    var command = new CreateCustomerCommand(
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: "01512345678",
      Vehicles: []
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCustomerCommand.Vehicles));
  }
}