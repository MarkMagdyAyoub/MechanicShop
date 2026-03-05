using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Common.Errors;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity;

public sealed class IdentityService(
  UserManager<ApplicationUser> userManager
) : IIdentityService
{
  private readonly UserManager<ApplicationUser> _userManager = userManager;

  public async Task<Result<UserDto>> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken)
  {
    var user = await _userManager.FindByEmailAsync(email);

    if(user is null)
      return InfrastructureErrors.UserEmailNotFound(email);

    if (!user.EmailConfirmed)
      return InfrastructureErrors.UserEmailNotConfirmed(email);

    if(!await _userManager.CheckPasswordAsync(user , password))
      return InfrastructureErrors.InvalidPassword;

    var roles = await _userManager.GetRolesAsync(user);
    var claims = await _userManager.GetClaimsAsync(user);

    return new UserDto(
      user.Id, 
      user.Email! ,
      roles.AsReadOnly() , 
      claims.AsReadOnly()
    );
  }

  public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if(user is null)
    {
      return ApplicationErrors.UserNotFound;
    }

    var roles = await _userManager.GetRolesAsync(user);
    var claims = await _userManager.GetClaimsAsync(user);

    return new UserDto(
      user.Id,
      user.Email!,
      roles.AsReadOnly(),
      claims.AsReadOnly()
    );
  }

  public async Task<string?> GetUserNameAsync(Guid userId)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if(user is null)
      return null;

    return user.UserName;
  }
}