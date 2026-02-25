using System.Security.Claims;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenProvider
{
  Task<Result<TokenDto>> GenerateJwtTokenAsync(UserDto user , CancellationToken cancellationToken);
  ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}