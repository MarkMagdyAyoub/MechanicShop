using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.DTOs;

public sealed record UserDto(
  string userId,
  string Email,
  IList<string> Roles,
  IList<Claim> Claims
);