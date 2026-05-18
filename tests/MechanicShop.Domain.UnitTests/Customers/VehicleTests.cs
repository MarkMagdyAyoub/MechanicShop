using MechanicShop.Tests.Common.VehicleGenerator;

namespace MechanicShop.Domain.UnitTests.Vehicle;

public class VehicleTests
{
  [Fact]
  public void Create_ValidData_ReturnVehicleInstance()
  {
    // Given
    var result = VehicleFactory.Create(
      id: Guid.NewGuid(),
      make: "Toyota",
      model: "Corolla",
      year: 2022,
      licensePlate: "Cairo 1234 ABC"
    );
    var vehicle = result.Value;

    Assert.True(result.IsSuccess);
    Assert.Equal("Toyota" , vehicle.Make);
    Assert.Equal("Corolla" , vehicle.Model);
    Assert.Equal(2022 , vehicle.Year);
    Assert.Equal("Cairo 1234 ABC" , vehicle.LicensePlate);
  }

  [Fact]
  public void Update_ValidData_ReturnVehicleInstance()
  {
    // Given
    var vehicle = VehicleFactory.Create(
      id: Guid.NewGuid(),
      make: "Toyota",
      model: "Corolla",
      year: 2022,
      licensePlate: "Cairo 1234 ABC"
    ).Value;

    var updatedVehicleResult = vehicle.Update(
      make: "Honda",
      model: "Civic",
      year: 2024,
      licensePlate: "CAI-5678"
    );

    Assert.True(updatedVehicleResult.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_MakeIsNullOrEmpty_ReturnMakeRequiredError(string make)
  {
    var result = VehicleFactory.Create(make: make);
    
    Assert.True(result.IsError);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_ModelIsNullOrEmpty_ReturnModelRequiredError(string model)
  {
    var result = VehicleFactory.Create(model: model);
    
    Assert.True(result.IsError);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_LicensePlateNullOrEmpty_ReturnLicensePlateRequiredError(string licensePlate)
  {
    var result = VehicleFactory.Create(licensePlate: licensePlate);
    
    Assert.True(result.IsError);
  }

  [Theory]
  [InlineData(1885)]
  [InlineData(2200)]
  public void Create_YearIsInvalid_ReturnYearInvalidError(int year)
  {
    var result = VehicleFactory.Create(year: year);
    
    Assert.True(result.IsError);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_MakeIsEmpty_ReturnMakeRequiredError(string make)
  {
    var vehicle = VehicleFactory.Create().Value;
  
    var result = vehicle.Update(make , vehicle.Model , vehicle.Year , vehicle.LicensePlate);

    Assert.False(result.IsSuccess);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_ModelIsEmpty_ReturnModelRequiredError(string model)
  {
    var vehicle = VehicleFactory.Create().Value;
  
    var result = vehicle.Update(vehicle.Make , model , vehicle.Year , vehicle.LicensePlate);

    Assert.False(result.IsSuccess);
  }

  [Theory]
  [InlineData(1885)]
  [InlineData(2200)]
  public void Update_YearInvalid_ReturnYearInvalidError(int year)
  {
    var vehicle = VehicleFactory.Create().Value;
  
    var result = vehicle.Update(vehicle.Make , vehicle.Model , year, vehicle.LicensePlate);

    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void VehicleInfo_ShouldReturnFormattedString()
  {
      var vehicle = VehicleFactory.Create(make: "Ford", model: "Mustang", year: 2021).Value;

      Assert.Equal("Ford | Mustang | 2021", vehicle.VehicleInfo);
  }
}

