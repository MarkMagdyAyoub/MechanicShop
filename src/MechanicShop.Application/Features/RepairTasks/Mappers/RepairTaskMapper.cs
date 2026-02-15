using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMapper
{
  public static RepairTaskDto ToDto(this RepairTask repairTask)
  {
    ArgumentNullException.ThrowIfNull(repairTask);

    return new RepairTaskDto
    {
      Id = repairTask.Id,
      Name = repairTask.Name,
      LaborCost = repairTask.LaborCost,
      TotalCost = repairTask.TotalCost,
      EstimatedDurationInMinS = repairTask.EstimatedDurationInMins,
      Parts = repairTask.Parts.Select(p => p.ToDto()).ToList()
    };
  }
}