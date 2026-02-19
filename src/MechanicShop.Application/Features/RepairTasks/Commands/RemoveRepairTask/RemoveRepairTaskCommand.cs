using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;

public sealed class RemoveRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<RemoveRepairTaskCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<RemoveRepairTaskCommand, Result<Deleted>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<RemoveRepairTaskCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Deleted>> Handle(RemoveRepairTaskCommand request, CancellationToken cancellationToken)
  {
    var exists = await _context.RepairTasks.FirstOrDefaultAsync(rt => rt.Id == request.RepairTaskId , cancellationToken);
    if (exists is null)
    {
      _logger.LogWarning("RepairTask {RepairTaskId} Not Found For Deletion" , request.RepairTaskId);
      return ApplicationErrors.RepairTaskNotFound;
    }

    // TODO: check if the repair task is not in any work order
    //var exists = _context.WorkOrders.AsNoTracking().SelectMany(wo => wo.RepairTasks).AnyAsync(rt => rt.RepairTaskId , ct)
    // if(exists) LogWarning $$ RepairTaskErrors.InUse

    _context.RepairTasks.Remove(exists);
    
    await _context.SaveChangesAsync(cancellationToken);
    
    await _cache.RemoveByTagAsync("repair-task" , cancellationToken);
    
    _logger.LogInformation("Repair Task `{RepairTaskId}` Deleted Successfully." , request.RepairTaskId);

    return Result.Deleted;
  }
}