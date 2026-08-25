// <copyright file="CurrentUser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Services;

using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

  /// <inheritdoc/>
  public Guid? Id
  {
    get
    {
      var userId = this.httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (Guid.TryParse(userId, out var parsedValue))
      {
        return parsedValue;
      }

      return null;
    }
  }
}
