using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Tests.Common.CustomerGenerator;
using MechanicShop.Tests.Common.VehicleGenerator;

namespace MechanicShop.Domain.UnitTests.Customer;

public class CustomerTests
{
  [Fact]
  public void Create_ValidData_ReturnCustomerInstance()
  {
    var result = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com",
      vehicles: [VehicleFactory.Create().Value]
    );

    var customer = result.Value;
  
    Assert.True(result.IsSuccess);
    Assert.Equal("Mark" , customer.Name);
    Assert.Equal("01112978485" , customer.PhoneNumber);
    Assert.Equal("mark@gmail.com" , customer.Email);
    Assert.Single(customer.Vehicles);
  }

  [Fact]
  public void Update_ValidData_ReturnCustomerInstance()
  {
    var customer = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com",
      vehicles: [VehicleFactory.Create().Value]
    ).Value;

    var updatedCustomerResult = customer.Update(
      name: "Bob",
      phoneNumber: "01524578943",
      email: "bob@gmail.com"
    );

  
    Assert.True(updatedCustomerResult.IsSuccess);
    Assert.Equal(Result.Updated , updatedCustomerResult.Value);
    Assert.Equal("Bob" , customer.Name);
    Assert.Equal("01524578943" , customer.PhoneNumber);
    Assert.Equal("bob@gmail.com" , customer.Email);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_NameIsEmptyOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var result = CustomerFactory.Create(name: name);

    Assert.False(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("015151552200200")]
  [InlineData("01210")]
  public void Create_PhoneNumberIsEmptyOrWhiteSpaceOrInvalid_ReturnInvalidPhoneNumberError(string phoneNumber)
  {
    var result = CustomerFactory.Create(phoneNumber: phoneNumber);

    Assert.False(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_EmailIsEmptyOrWhiteSpace_ReturnEmailRequiredError(string email)
  {
    var result = CustomerFactory.Create(email: email);

    Assert.False(result.IsSuccess);
  }

  [Theory]
  [InlineData("mark-magdy$gmail.com")]
  public void Create_EmailIsInvalid_ReturnEmailRequiredError(string email)
  {
    var result = CustomerFactory.Create(email: email);

    Assert.False(result.IsSuccess);
    Assert.Equal(result.TopError.Code , CustomerErrors.EmailInvalid.Code);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_NameIsEmptyOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var customer = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com"
    ).Value;

    var updatedCustomerResult = customer.Update(
      name: name,
      phoneNumber: customer.PhoneNumber!,
      email: customer.Email!
    );

    Assert.False(updatedCustomerResult.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_EmailIsEmptyOrWhiteSpace_ReturnEmailRequiredError(string email)
  {
    var customer = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com"
    ).Value;

    var updatedCustomerResult = customer.Update(
      name: customer.Name!,
      phoneNumber: customer.PhoneNumber!,
      email: email
    );

    Assert.False(updatedCustomerResult.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("015151552200200")]
  [InlineData("01210")]
  public void Update_PhoneNumberIsEmptyOrWhiteSpaceOrInvalid_ReturnInvalidPhoneNumberError(string phone)
  {
    var customer = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com"
    ).Value;

    var updatedCustomerResult = customer.Update(
      name: customer.Name!,
      phoneNumber: phone,
      email: customer.Email!
    );

    Assert.False(updatedCustomerResult.IsSuccess);
  }

  [Theory]
  [InlineData("mark-magdy$gmail.com")]
  public void Update_EmailIsInvalid_ReturnEmailInvalidError(string email)
  {
    var customer = CustomerFactory.Create(
      id: Guid.NewGuid(),
      name: "Mark",
      phoneNumber: "01112978485",
      email: "mark@gmail.com"
    ).Value;

    var updatedCustomerResult = customer.Update(
      name: customer.Name!,
      phoneNumber: customer.PhoneNumber!,
      email: email
    );

    Assert.False(updatedCustomerResult.IsSuccess);
  }

  [Fact]
  public void UpsertParts_AddNewVehiclesAndUpdateExisting_ReturnUpdatedAndNewVehicles()
  {
    var originalVehicle = VehicleFactory.Create(make: "Ford").Value;
    var customer = CustomerFactory.Create(vehicles: [originalVehicle]).Value;

    var updatedVehicle = VehicleFactory.Create(id: originalVehicle.Id, make: "Tesla").Value;
    var newVehicle = VehicleFactory.Create(make: "Toyota").Value;

    var result = customer.UpsertParts([updatedVehicle, newVehicle]);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, customer.Vehicles.Count());
    Assert.Equal(Result.Updated, result.Value);
    Assert.Contains(customer.Vehicles, v => v.Id == updatedVehicle.Id && v.Make == "Tesla");
    Assert.Contains(customer.Vehicles, v => v.Id == newVehicle.Id && v.Make == "Toyota");
  }

  [Fact]
  public void UpsertParts_AddingNewVehicleList_ReturnUpdatedAndNewVehicles()
  {
    var originalVehicle = VehicleFactory.Create(make: "Ford").Value;
    var customer = CustomerFactory.Create(vehicles: [originalVehicle]).Value;

    var newVehicle1 = VehicleFactory.Create(make: "Tesla").Value;
    var newVehicle2 = VehicleFactory.Create(make: "Toyota").Value;

    var result = customer.UpsertParts([newVehicle1, newVehicle2]);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, customer.Vehicles.Count());
    Assert.Equal(Result.Updated, result.Value);
    Assert.Contains(customer.Vehicles, v => v.Id == newVehicle1.Id && v.Make == "Tesla");
    Assert.Contains(customer.Vehicles, v => v.Id == newVehicle2.Id && v.Make == "Toyota");
  }
}