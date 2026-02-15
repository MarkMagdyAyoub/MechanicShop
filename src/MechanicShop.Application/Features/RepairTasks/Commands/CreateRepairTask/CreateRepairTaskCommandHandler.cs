using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandHandler(
  IAppDbContext context,
  ILogger<CreateRepairTaskCommandHandler> logger,
  HybridCache cache
)
: IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken cancellationToken)
  {
    
    // TODO: Don't Forget To Make Name CITEXT For Case-Insensitive
    var nameExists = await _context.RepairTasks.AnyAsync(rt => rt.Name == request.Name); 

    if (nameExists)
    {
      _logger.LogWarning("Duplicate Part Name {PartName}" , request.Name);
      return RepairTaskErrors.DuplicateName;
    }

    List<Part> parts = [];

    foreach(var part in request.Parts)
    {
      var createPartResult = Part.Create(Guid.NewGuid() , part.Name , part.Cost , part.Quantity);
      if (createPartResult.IsError)
      {
        return createPartResult.Errors;
      }
      parts.Add(createPartResult.Value);
    }

    var createRepairTaskResult = RepairTask.Create(
      id: Guid.NewGuid(),
      name: request.Name,
      laborCost: request.LaborCost,
      estimatedDurationInMins: request.EstimatedDurationInMins,
      parts: parts
    );

    if (createRepairTaskResult.IsError)
    {
      return createRepairTaskResult.Errors;
    }

    await _context.RepairTasks.AddAsync(createRepairTaskResult.Value , cancellationToken);
    await _context.SaveChangeAsync(cancellationToken);
    await _cache.RemoveByTagAsync("repair-task" , cancellationToken);
    
    _logger.LogInformation("Repair Task Added Successfully");

    return createRepairTaskResult.Value.ToDto();


  }
}