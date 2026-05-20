using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.PartGenerator;

namespace MechanicShop.Domain.UnitTests.Part;

public class PartTests
{
  [Fact]
  public void Create_ValidDate_ReturnPartInstance()
  {
    var result = PartFactory.Create();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Update_ValidData_UpdatePartInstance()
  {
    var part = PartFactory.Create(Guid.NewGuid() , "Engine Oil" , 25.00m , 1).Value;

    var updatedResult = part.Update("Brake Pads" , 45m , 4);

    Assert.True(updatedResult.IsSuccess);
    Assert.Equal("Brake Pads" , part.Name);
    Assert.Equal(45 , part.Cost);
    Assert.Equal(4 , part.Quantity);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_NameIsNullOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var result = PartFactory.Create(name: name);

    Assert.False(result.IsSuccess);
    Assert.Equal(PartErrors.NameRequired.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(100002)]
  public void Create_CostIsInvalid_ReturnCostInvalidError(decimal cost)
  {
    var result = PartFactory.Create(cost: cost);

    Assert.False(result.IsSuccess);
    Assert.Equal(PartErrors.CostInvalid.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(21)]
  public void Create_QuantityIsInvalid_ReturnQuantityInvalidError(int quantity)
  {
    var result = PartFactory.Create(quantity: quantity);

    Assert.False(result.IsSuccess);
    Assert.Equal(PartErrors.QuantityInvalid.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Update_NameIsNullOrWhiteSpace_ReturnNameRequiredError(string name)
  {
    var part = PartFactory.Create(Guid.NewGuid() , "Engine Oil" , 25.00m , 1).Value;

    var updatedResult = part.Update(name , part.Cost , part.Quantity);

    Assert.False(updatedResult.IsSuccess);
    Assert.Equal(PartErrors.NameRequired.Code , updatedResult.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(10002)]
  public void Update_CostIsInvalid_ReturnCostInvalidError(decimal cost)
  {
    var part = PartFactory.Create(Guid.NewGuid() , "Engine Oil" , 25.00m , 1).Value;

    var updatedResult = part.Update(part.Name , cost , part.Quantity);

    Assert.False(updatedResult.IsSuccess);
    Assert.Equal(PartErrors.CostInvalid.Code , updatedResult.TopError.Code);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(21)]
  public void Update_QuantityIsInvalid_ReturnQuantityInvalidError(int quantity)
  {
    var part = PartFactory.Create(Guid.NewGuid() , "Engine Oil" , 25.00m , 1).Value;

    var updatedResult = part.Update(part.Name , part.Cost , quantity);

    Assert.False(updatedResult.IsSuccess);
    Assert.Equal(PartErrors.QuantityInvalid.Code , updatedResult.TopError.Code);
  }
}
