// <copyright file="IdentityController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using System.Security.Claims;
using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.Features.Identity.Commands.RefreshToken;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiController
{
  [HttpPost("token/generate")]
  [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(TokenDto), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Generate A JWT Token And Refresh Token For A Valid User.")]
  [EndpointDescription("Authenticates A User Using Provided Credentials And Return A JWT Token And Refresh Token Pair.")]
  [EndpointName("Generate Token")]
  [Consumes("application/json")]
  [Tags("identity")]
  public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenCommand command, CancellationToken ct)
  {
    var result = await sender.Send(command, ct);

    return result.Match(
      response => this.Ok(response),
      this.ProblemDetailsHandler);
  }

  [HttpPost("token/refresh-token")]
  [Authorize]
  [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(TokenDto), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Generate A Refresh Access Token Using A Valid Refresh Token")]
  [EndpointDescription("Exchange An Expired Token With A New Access Token.")]
  [EndpointName("RefreshToken")]
  [Consumes("application/json")]
  [Tags("identity")]
  public async Task<ActionResult> GenerateRefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
  {
    var result = await sender.Send(command, ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpGet("user/claims")]
  [Authorize]
  [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointSummary("Get The Current User's Info")]
  [EndpointDescription("Return Information About The Current Authenticated User.")]
  [EndpointName("GetUserInformation")]
  [Tags("identity")]
  public async Task<ActionResult> GetUserInfo(CancellationToken ct)
  {
    var userId = Guid.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var result = await sender.Send(new GetUserInfoByIdQuery(userId), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }
}
