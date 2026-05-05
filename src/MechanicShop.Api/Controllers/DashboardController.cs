using MechanicShop.Application.Features.Dashboard.DTOs;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MechanicShop.Api.Controllers;

[Route("api/dashboard")]
[ApiVersion("1.0")]
[Authorize]
public sealed class DashboardController(ISender sender) : ApiController
{
  
  [HttpGet("stats")]
  [ProducesResponseType(typeof(WorkOrderStatisticsDto) , StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails) , StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetStats([FromQuery] DateOnly? date , CancellationToken ct)
  {
    var todayDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var result = await sender.Send(new GetWorkOrderStatisticsQuery(todayDate));

    return result.Match(
      Ok,
      ProblemDetailsHandler
    );
  }
}