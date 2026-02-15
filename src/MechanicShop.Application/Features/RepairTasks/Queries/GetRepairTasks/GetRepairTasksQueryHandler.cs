using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public sealed class GetRepairTasksQueryHandler (
  IAppDbContext context,
  ILogger<GetRepairTasksQueryHandler> logger
) : IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetRepairTasksQueryHandler> _logger = logger;

  public async Task<Result<List<RepairTaskDto>>> Handle(GetRepairTasksQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Return All RepairTasks");
    return await _context.RepairTasks
      .AsNoTracking()
      .Include(rt => rt.Parts)
      .Select(rt => rt.ToDto())
      .ToListAsync(cancellationToken);
  }
}