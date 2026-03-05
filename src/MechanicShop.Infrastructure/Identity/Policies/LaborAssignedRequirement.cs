using System.Security.Claims;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Identity.Policies;

public sealed class OwnWorkOrderAccessRequirement : IAuthorizationRequirement;

public sealed class OwnWorkOrderAccessRequirementHandler(IAppDbContext context , IHttpContextAccessor contextAccessor) : AuthorizationHandler<OwnWorkOrderAccessRequirement>
{
  private readonly IAppDbContext _context = context;
  private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnWorkOrderAccessRequirement requirement)
  {
    var userId = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if(!Guid.TryParse(userId , out var userIdGuid))
    {
      context.Fail();
      return;
    }
    
    var workOrderId = _contextAccessor.HttpContext?.Request.RouteValues["WorkOrderId"]?.ToString();
    
    if(!Guid.TryParse(workOrderId , out var workOrderIdGuid))
    {
      context.Fail();
      return;
    }

    var isAssigned = await _context.WorkOrders.AnyAsync(wo => wo.Id == workOrderIdGuid && wo.LaborId == userIdGuid , new CancellationToken());

    if (isAssigned)
    {
      context.Succeed(requirement);
      return;
    }

    if (context.User.IsInRole(nameof(Role.Manager)))
    {
      context.Succeed(requirement);
      return;
    }

    context.Fail();
  }
}