using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<UpdateRepairTaskCommandHandler> logger,
  HybridCache cache
)
: IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Updated>> Handle(UpdateRepairTaskCommand request, CancellationToken cancellationToken)
  { 
    var exists = await _context.RepairTasks
                        .Include(rt => rt.Parts)
                        .FirstOrDefaultAsync(rt => rt.Id == request.RepairTaskId , cancellationToken);
    
    if(exists is null)
    {
      _logger.LogWarning("Repair Task `{RepairTaskId}` Not Found For Update." , request.RepairTaskId);
      return ApplicationErrors.RepairTaskNotFound;
    }

    var parts = new List<Part>();

    foreach (var part in request.Parts)
    {
      var partId = part.PartId ?? Guid.NewGuid();
      var partResult = Part.Create(partId , part.Name , part.Cost , part.Quantity);
      if (partResult.IsError)
      {
        return partResult.Errors;
      }
      parts.Add(partResult.Value);
    }

    var updateRepairTaskResult = exists.Update(request.Name , request.LaborCost , request.EstimatedDurationInMins);
    if (updateRepairTaskResult.IsError)
    {
      return updateRepairTaskResult.Errors;
    }

    var upsertPartsResult = exists.UpsertParts(parts);
    if (upsertPartsResult.IsError)
    {
      return upsertPartsResult.Errors;
    }

    await _context.SaveChangesAsync(cancellationToken);
    
    await _cache.RemoveByTagAsync("repair-task" , cancellationToken);

    _logger.LogInformation("Repair Task `{RepairTaskId}` Updated Successfully" , request.RepairTaskId);
    
    return Result.Updated;
  }
}