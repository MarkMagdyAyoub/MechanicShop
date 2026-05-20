using MechanicShop.Domain.Employees;
using MechanicShop.Tests.Common.EmployeeGenerator;
namespace MechanicShop.Domain.UnitTests.Employee;

public class EmployeeTests
{
  [Fact]
  public void Create_ValidData_ReturnsEmployeeInstance()
  {
    var result = EmployeeFactory.Create();
  
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_IdIsEmpty_ReturnsIdRequiredError()
  {
    var result = EmployeeFactory.Create(id: Guid.Empty);
  
    Assert.False(result.IsSuccess);
    Assert.Equal(EmployeeErrors.IdRequired.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData(" ")]
  [InlineData("")]
  public void Create_FirstNameIsEmptyOrWhiteSpace_ReturnsFirstNameRequiredError(string firstName)
  {
    var result = EmployeeFactory.Create(firstName: firstName);
  
    Assert.False(result.IsSuccess);
    Assert.Equal(EmployeeErrors.FirstNameRequired.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData(" ")]
  [InlineData("")]
  public void Create_LastNameIsEmptyOrWhiteSpace_ReturnsLastNameRequiredError(string lastName)
  {
    var result = EmployeeFactory.Create(lastName: lastName);
  
    Assert.False(result.IsSuccess);
    Assert.Equal(EmployeeErrors.LastNameRequired.Code , result.TopError.Code);
  }
}