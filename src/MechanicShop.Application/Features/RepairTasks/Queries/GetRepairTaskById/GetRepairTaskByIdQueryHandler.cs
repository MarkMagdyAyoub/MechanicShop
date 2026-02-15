using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

public sealed class GetRepairTaskByIdQueryHandler(
  IAppDbContext context,
  Logger<GetRepairTaskByIdQueryHandler> logger
) : IRequestHandler<GetRepairTaskByIdQuery, Result<RepairTaskDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly Logger<GetRepairTaskByIdQueryHandler> _logger = logger;

  public async Task<Result<RepairTaskDto>> Handle(GetRepairTaskByIdQuery request, CancellationToken cancellationToken)
  { 
    var exists = await _context.RepairTasks.AsNoTracking().Include(rt => rt.Parts).FirstOrDefaultAsync(rt => rt.Id == request.RepairTaskId , cancellationToken);
    if(exists is null)
    {
      _logger.LogWarning("RepairTask With Id `{RepairTaskId}` Not Found For Retrieval" , request.RepairTaskId);
      return ApplicationErrors.RepairTaskNotFound;
    }

    return exists.ToDto();
  }
}