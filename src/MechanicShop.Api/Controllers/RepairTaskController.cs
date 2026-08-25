// <copyright file="RepairTaskController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

[Route("api/repair-tasks")]
[ApiVersion("1.0")]
[Authorize]
public sealed class RepairTaskController(ISender sender) : ApiController
{
  [HttpGet]
  [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetAllRepairTasks")]
  [EndpointSummary("Return All Repair Tasks In The System.")]
  [EndpointDescription("Returns A List Of All Repair Tasks Available In The System.")]
  [Tags("repairTasks")]
  [OutputCache(Duration = 60)]
  public async Task<IActionResult> GetAll(CancellationToken ct)
  {
    var result = await sender.Send(new GetRepairTasksQuery(), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpGet("{repairTaskId:guid}", Name = "GetRepairTaskById")]
  [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetRepairTaskById")]
  [EndpointSummary("Return A Repair Tasks By Id.")]
  [EndpointDescription("Returns A Details Information About Specific Repair Tasks.")]
  [Tags("repairTasks")]
  [OutputCache(Duration = 60)]
  public async Task<IActionResult> GetById(Guid repairTaskId, CancellationToken ct)
  {
    var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpPost]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("CreateRepairTask")]
  [EndpointSummary("Return A New Repair Tasks.")]
  [EndpointDescription("Creates A Repair Task.")]
  [Tags("repairTasks")]
  public async Task<IActionResult> Create([FromBody] CreateRepairTaskRequest request, CancellationToken ct)
  {
    var parts = request.Parts.ConvertAll(part => new CreateRepairTaskPartCommand(part.Name, part.Cost, part.Quantity));
    var result = await sender.Send(
      new CreateRepairTaskCommand(
        request.Name,
        request.LaborCost,
        EstimatedDurationInMins: (Domain.RepairTasks.Enums.RepairDurationInMinutes)request.EstimatedDurationInMins,
        parts),
      ct);

    return result.Match(
      response => this.CreatedAtRoute(
        routeName: "GetRepairTaskById",
        routeValues: new
        {
          version = this.HttpContext.GetRequestedApiVersion()?.ToString(),
          repairTaskId = response.Id,
        },
        value: response),
      this.ProblemDetailsHandler);
  }

  [HttpPut("{repairTaskId:guid}")]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("UpdateRepairTask")]
  [EndpointSummary("Update An Existing Repair Tasks.")]
  [EndpointDescription("Update Information About A Specified Repair Task.")]
  [Tags("repairTasks")]
  public async Task<IActionResult> Update(Guid repairTaskId, [FromBody] UpdateRepairTaskRequest request, CancellationToken ct)
  {
    var parts = request.Parts.ConvertAll(part => new UpdateRepairTaskPartCommand(part.Id, part.Name, part.Cost, part.Quantity));

    var result = await sender.Send(
      new UpdateRepairTaskCommand(
        repairTaskId,
        request.Name,
        request.LaborCost,
        EstimatedDurationInMins: (Domain.RepairTasks.Enums.RepairDurationInMinutes)request.EstimatedDurationInMins,
        parts),
      ct);

    return result.Match(
      response => this.Ok(response),
      this.ProblemDetailsHandler);
  }

  [HttpDelete("{repairTaskId:guid}")]
  [Authorize(Roles = nameof(Role.Manager))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("DeleteRepairTask")]
  [EndpointSummary("Delete An Existing Repair Tasks.")]
  [EndpointDescription("Delete A Specified Repair Task.")]
  [Tags("repairTasks")]
  public async Task<IActionResult> Delete(Guid repairTaskId, CancellationToken ct)
  {
    var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId), ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }
}
