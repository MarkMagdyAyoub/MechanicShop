using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.RefreshTokenGenerator;

namespace MechanicShop.Domain.UnitTests.Identity;

public class RefreshTokenTests
{
  [Fact]
  public void Create_ValidData_ReturnIdRequiredError()
  {
    var result = RefreshTokenFactory.Create();
    
    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_IdIsEmpty_ReturnIdRequiredError()
  {
    var result = RefreshTokenFactory.Create(id: Guid.Empty);
    
    Assert.False(result.IsSuccess);
    Assert.Equal(RefreshTokenErrors.IdRequired.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_TokenIsEmptyOrWhiteSpace_ReturnTokenRequiredError(string token)
  {
    var result = RefreshTokenFactory.Create(token: token);
    
    Assert.False(result.IsSuccess);
    Assert.Equal(RefreshTokenErrors.TokenRequired.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_UserIdEmpty_ReturnUserIdRequiredError()
  {
    var result = RefreshTokenFactory.Create(userId: Guid.Empty);
    
    Assert.False(result.IsSuccess);
    Assert.Equal(RefreshTokenErrors.UserIdRequired.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_ExpiresOnUtcPastDate_ReturnPastDateError()
  {
    var result = RefreshTokenFactory.Create(expiresOnUtc: DateTimeOffset.UtcNow.AddDays(-1));
    
    Assert.False(result.IsSuccess);
    Assert.Equal(RefreshTokenErrors.PastDate.Code , result.TopError.Code);
  }
}

