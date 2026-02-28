using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
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

    var isRepairTaskInAnyWorkOrder = await _context.WorkOrders
                                          .AsNoTracking()
                                          .SelectMany(wo => wo.RepairTasks)
                                          .AnyAsync(rt => rt.Id == request.RepairTaskId , cancellationToken);
    if (isRepairTaskInAnyWorkOrder)
    {
      _logger.LogWarning("Cannot Deleted Repair Task With Id `{repairTaskId}` Because It Is Included In A Work Order" , request.RepairTaskId);

      return RepairTaskErrors.InUse;
    }

    _context.RepairTasks.Remove(exists);
    
    await _context.SaveChangesAsync(cancellationToken);
    
    await _cache.RemoveByTagAsync("repair-task" , cancellationToken);
    
    _logger.LogInformation("Repair Task `{RepairTaskId}` Deleted Successfully." , request.RepairTaskId);

    return Result.Deleted;
  }
}