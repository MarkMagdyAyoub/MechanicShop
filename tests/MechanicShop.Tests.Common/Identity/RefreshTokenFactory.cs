using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.RefreshTokenGenerator;

public static class RefreshTokenFactory
{
  public static Result<RefreshToken> Create(
    Guid? id = null , 
    string? token = null, 
    Guid? userId = null, 
    DateTimeOffset? expiresOnUtc = null
  )
  {
    return RefreshToken.Create(
      id ?? Guid.NewGuid(),
      token ?? "token",
      userId ?? Guid.NewGuid(),
      expiresOnUtc ?? DateTimeOffset.UtcNow.AddDays(1)
    );
  }
}