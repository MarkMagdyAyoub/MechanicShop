using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public Guid? Id
  {
    get
    {
      var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if(Guid.TryParse(userId , out var parsedValue))
      {
        return parsedValue;
      }

      return null;
    }
  }
}