using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Tests.Common.VehicleGenerator;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandValidatorTests
{
  private readonly UpdateCustomerCommandValidator _validator;

  public UpdateCustomerCommandValidatorTests()
  {
    _validator = new UpdateCustomerCommandValidator();
  }

  [Fact]
  public void Constructor_WhenNameIsEmpty_ShouldHaveValidationError()
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "",
      Email: "mark@example.com",
      PhoneNumber: "01152846575",
      Vehicles: [VehicleFactory.CreateUpdateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Name));
  }

  [Theory]
  [InlineData("invalid-email")]
  [InlineData("invalid-email@")]
  [InlineData("invalid-email.com")]
  public void Constructor_WhenEmailIsInvalid_ShouldHaveValidationError(string email)
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "John Doe",
      Email: email,
      PhoneNumber: "0123456789",
      Vehicles: [VehicleFactory.CreateUpdateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Email));
  }

  [Theory]
  [InlineData("johndoe@example.com")]
  [InlineData("jane.smith@example.com")]
  public void Constructor_WhenEmailIsValid_ShouldNotHaveValidationError(string email)
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "John Doe",
      Email: email,
      PhoneNumber: "01210480491",
      Vehicles: [VehicleFactory.CreateUpdateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
    Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Email));
  }

  [Theory]
  [InlineData("invalid-phone-number")]
  [InlineData("123")]
  public void Constructor_WhenPhoneNumberIsInvalid_ShouldHaveValidationError(string phoneNumber)
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: phoneNumber,
      Vehicles: [VehicleFactory.CreateUpdateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.PhoneNumber));
  }

  [Theory]
  [InlineData("01512345678")]
  [InlineData("01210480491")]
  [InlineData("01123456782")]
  public void Constructor_WhenPhoneNumberIsValid_ShouldNotHaveValidationError(string phoneNumber)
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: phoneNumber,
      Vehicles: [VehicleFactory.CreateUpdateCommand()]
    );

    var result = _validator.Validate(command);

    Assert.True(result.IsValid);
    Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.PhoneNumber));
  }

  [Fact]
  public void Constructor_WhenVehiclesListIsEmpty_ShouldHaveValidationError()
  {
    var command = new UpdateCustomerCommand(
      CustomerId: Guid.NewGuid(),
      Name: "John Doe",
      Email: "john.doe@example.com",
      PhoneNumber: "01512345678",
      Vehicles: []
    );

    var result = _validator.Validate(command);

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCustomerCommand.Vehicles));
  }
}
