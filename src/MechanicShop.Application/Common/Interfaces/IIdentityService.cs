using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface IIdentityService
{
  Task<Result<UserDto>> AuthenticateUserAsync(string email , string password , CancellationToken cancellationToken);
  Task<Result<UserDto>> GetUserByIdAsync(Guid UserId);
}