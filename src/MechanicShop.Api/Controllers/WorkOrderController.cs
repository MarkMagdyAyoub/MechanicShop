// <copyright file="WorkOrderController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Scheduling.DTOs;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/work-orders")]
[ApiVersion("1.0")]
[Authorize]
public sealed class WorkOrderController(ISender sender) : ApiController
{
  private const string TAG = "workOrders";

  [HttpGet]
  [ProducesResponseType(typeof(PaginatedList<WorkOrderDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetWorkOrderPage")]
  [EndpointSummary("Retrieve A Paginated List Of Work Orders")]
  [EndpointDescription("Returns A Paginated List Of Work Orders Filtered By Search Term, State, Vehicle, Labor, Spot, And Date Ranges.")]
  [Tags(TAG)]
  public async Task<IActionResult> GetWorkOrderPage([FromQuery] WorkOrderFilterRequest filter, CancellationToken ct)
  {
    var result = await sender.Send(
      new GetWorkOrdersQuery(
        filter.PageNumber,
        filter.PageSize,
        filter.SearchTerm,
        filter.SearchColumn,
        filter.SortDirection,
        filter.State is null ? null : (Domain.WorkOrders.Enums.WorkOrderState)filter.State,
        filter.VehicleId,
        filter.LaborId,
        filter.StartDateFrom,
        filter.StartDateTo,
        filter.EndDateFrom,
        filter.EndDateTo,
        filter.Spot is null ? null : (Domain.WorkOrders.Enums.Spot)filter.Spot),
      ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpGet("{workOrderId:guid}", Name = "GetWorkOrderById")]
  [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetWorkOrderById")]
  [EndpointSummary("Retrieve Work Order Details By ID")]
  [EndpointDescription("Returns Detailed Information About A Specific Work Order Using Its Unique Identifier.")]
  [Tags(TAG)]
  public async Task<IActionResult> GetById(Guid workOrderId, CancellationToken ct)
  {
    var result = await sender.Send(new GetWorkOrderByIdQuery(workOrderId), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpPut("{workOrderId:guid}/state")]
  [Authorize(Roles = $"{nameof(Role.Labor)},{nameof(Role.Manager)}", Policy = "OwnWorkOrderAccess")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("UpdateWorkOrderState")]
  [EndpointSummary("Update Work Order State")]
  [EndpointDescription(
    "Updates The Current State Of A Work Order. " +
    "This Operation Can Only Be Performed By A Manager Or The Labor Employee Assigned To The Specified Work Order.")]
  [Tags(TAG)]
  public async Task<IActionResult> UpdateState(
    Guid workOrderId,
    [FromBody] UpdateWorkOrderStateRequest request,
    CancellationToken ct)
  {
    var result = await sender.Send(
      new UpdateWorkOrderStateCommand(
        workOrderId,
        (Domain.WorkOrders.Enums.WorkOrderState)request.NewWorkOrderState),
      ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpPost]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("CreateWorkOrder")]
  [EndpointSummary("Create A New Work Order")]
  [EndpointDescription("Creates A New Work Order For A Vehicle, Assigns A Labor Employee, Schedules The Start Time, And Attaches Repair Tasks , Only Manager Role Who Have That Permission.")]
  [Tags(TAG)]
  public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
  {
    var result = await sender.Send(
      new CreateWorkOrderCommand(
        (Domain.WorkOrders.Enums.Spot)request.Spot,
        request.VehicleId,
        request.StartAt,
        request.RepairTaskIds,
        request.LaborId),
      ct);

    return result.Match(
      response => this.CreatedAtRoute(
        routeName: "GetWorkOrderById",
        routeValues: new
        {
          version = this.HttpContext.GetRequestedApiVersion()?.ToString(),
          workOrderId = response.WorkOrderId,
        },
        value: response),
      this.ProblemDetailsHandler);
  }

  [HttpPut("{workOrderId:guid}/labor")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("ReassignWorkOrderLabor")]
  [EndpointSummary("Reassign Work Order Labor")]
  [EndpointDescription("Reassigns A Work Order To Another Labor Employee. , Only Manager Role Who Have That Permission.")]
  [Tags(TAG)]
  public async Task<IActionResult> ReassignLabor(
    Guid workOrderId,
    [FromBody] ReassignWorkOrderLaborRequest request,
    CancellationToken ct)
  {
    var result = await sender.Send(
      new ReassignLaborCommand(
        workOrderId,
        request.LaborId),
      ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpPut("{workOrderId:guid}/relocate")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("RelocateWorkOrder")]
  [EndpointSummary("Relocate Work Order")]
  [EndpointDescription("Relocates A Work Order To A New Spot And Updates Its Scheduled Start Time.")]
  [Tags(TAG)]
  public async Task<IActionResult> Relocate(
    Guid workOrderId,
    [FromBody] RelocateWorkOrderRequest request,
    CancellationToken ct)
  {
    var result = await sender.Send(
      new RelocateWorkOrderCommand(
        workOrderId,
        request.NewStartAt,
        (Domain.WorkOrders.Enums.Spot)request.NewSpot),
      ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpPut("{workOrderId:guid}/repair-task")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("UpdateWorkOrderRepairTasks")]
  [EndpointSummary("Update Work Order Repair Tasks")]
  [EndpointDescription(
    "Updates The Repair Tasks Assigned To A Specific Work Order. " +
    "This Operation Can Only Be Performed By A Manager.")]
  [Tags(TAG)]
  public async Task<IActionResult> UpdateRepairTasks(
    Guid workOrderId,
    [FromBody] UpdateWorkOrderRepairTasksRequest request,
    CancellationToken ct)
  {
    var result = await sender.Send(
      new UpdateWorkOrderRepairTasksCommand(
        workOrderId,
        request.NewRepairTaskIds),
      ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpDelete("{workOrderId:guid}")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("DeleteWorkOrder")]
  [EndpointSummary("Delete Work Order")]
  [EndpointDescription(
    "Permanently Deletes A Work Order By Its Unique Identifier. " +
    "This Operation Can Only Be Performed By A Manager.")]
  [Tags(TAG)]
  public async Task<IActionResult> Delete(Guid workOrderId, CancellationToken ct)
  {
    var result = await sender.Send(new DeleteWorkOrderCommand(workOrderId), ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpGet("schedule/{date}")]
  [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetDailySchedule")]
  [EndpointSummary("Return Schedule Information For Specified Date.")]
  [EndpointDescription("Returns A Schedule View For The Specified Date. If No Date Is Provided, Today's Schedule Is Returned. You Can Optionally Filter By Labor ID.")]
  [Tags(TAG)]
  public async Task<IActionResult> GetDailySchedule(
    DateOnly? date,
    [FromQuery] Guid? laborId,
    [FromHeader(Name = "X-TimeZone")] string? zoneInfo,
    CancellationToken ct)
  {
    if (zoneInfo is null)
    {
      return this.Problem(
        detail: "`X-TimeZone` Header Not Provided In The Request Header.",
        statusCode: StatusCodes.Status400BadRequest,
        title: "Time Zone Required");
    }

    TimeZoneInfo timeZoneInfo;
    try
    {
      timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneInfo);
    }
    catch
    {
      return this.Problem(
        detail: "Invalid Or Unknown TimeZone",
        statusCode: StatusCodes.Status400BadRequest,
        title: "Invalid Time Zone");
    }

    var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

    var result = await sender.Send(new GetDailyScheduleQuery(timeZoneInfo, scheduleDate, laborId), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }
}
