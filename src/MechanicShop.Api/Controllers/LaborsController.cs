// <copyright file="LaborsController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using MechanicShop.Application.Features.Labor.Queries;
using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

[Route("api/labors")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaborsController(ISender sender) : ApiController
{
  [HttpGet]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetAllLabors")]
  [EndpointSummary("Get All Labors Existing In The System.")]
  [EndpointDescription("Return All Labors In The System , Only Managers Role Are Authorized.")]
  [Tags("labors")]
  [OutputCache(Duration = 60)]
  public async Task<IActionResult> GetLabors(CancellationToken ct)
  {
    var result = await sender.Send(new GetLaborsQuery(), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }
}
