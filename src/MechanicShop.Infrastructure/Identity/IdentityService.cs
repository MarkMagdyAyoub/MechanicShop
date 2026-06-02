using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Common.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Infrastructure.Identity;

public sealed class IdentityService(
  UserManager<ApplicationUser> userManager,
  HybridCache cache
) : IIdentityService
{
  private readonly UserManager<ApplicationUser> _userManager = userManager;
  private readonly HybridCache _cache = cache;

  public async Task<Result<UserDto>> AuthenticateUserAsync(string email, string password, CancellationToken ct)
  {
    var user = await _userManager.FindByEmailAsync(email); 

    if(user is null)
      return InfrastructureErrors.UserEmailNotFound(email);

    if (!user.EmailConfirmed)
      return InfrastructureErrors.UserEmailNotConfirmed(email);

    if(!await _userManager.CheckPasswordAsync(user , password)) 
      return InfrastructureErrors.InvalidPassword;

    var roles  = await GetCachedUserRolesAsync(user, ct);
    var claims = await GetCachedUserClaimsAsync(user, ct);

    return new UserDto(
      user.Id, 
      user.Email!,
      roles.AsReadOnly(), 
      claims.AsReadOnly()
    );
  }

  public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId , CancellationToken ct)
  {
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if(user is null)
    {
      return ApplicationErrors.UserNotFound;
    }

    var roles  = await GetCachedUserRolesAsync(user, ct);
    var claims = await GetCachedUserClaimsAsync(user, ct);

    return new UserDto(
      user.Id,
      user.Email!,
      roles.AsReadOnly(),
      claims.AsReadOnly()
    );
  }

  public async Task<string?> GetUserNameAsync(Guid userId , CancellationToken ct) =>
    await _cache.GetOrCreateAsync(
      key: $"userId:{userId}",
      factory: async _ => {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.UserName;
      },
      cancellationToken: ct
    );

  // TODO: don't forget to call it when you are modifying user's data
  private async Task InvalidateUserCache(ApplicationUser user , CancellationToken ct) =>
    await Task.WhenAll(
      _cache.RemoveAsync($"roles:{user.Id}" , ct).AsTask(),
      _cache.RemoveAsync($"claims:{user.Id}" , ct).AsTask(),
      _cache.RemoveAsync($"userId:{user.Id}" , ct).AsTask()
    );

  private async Task<IList<string>> GetCachedUserRolesAsync(ApplicationUser user , CancellationToken ct) =>
    await _cache.GetOrCreateAsync
    (
      key: $"roles:{user.Id}",
      factory: async _ => await _userManager.GetRolesAsync(user),
      cancellationToken: ct
    );

  private async Task<IList<Claim>> GetCachedUserClaimsAsync(ApplicationUser user , CancellationToken ct) =>
    await _cache.GetOrCreateAsync
    (
      key: $"claims:{user.Id}",
      factory: async _ => await _userManager.GetClaimsAsync(user),
      cancellationToken: ct
    );
}