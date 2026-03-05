using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.DTOs;

public sealed record UserDto(
  Guid userId,
  string Email,
  IReadOnlyList<string> Roles,
  IReadOnlyList<Claim> Claims
);