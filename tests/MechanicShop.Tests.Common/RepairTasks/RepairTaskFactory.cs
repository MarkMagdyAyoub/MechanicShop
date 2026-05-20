using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.PartGenerator;

namespace MechanicShop.Tests.Common.RepairTaskGenerator;

public static class RepairTaskFactory
{
  public static Result<RepairTask> Create(
    Guid? id = null, 
    string? name = null, 
    decimal? laborCost = null, 
    RepairDurationInMinutes? estimatedDurationInMins = null, 
    List<Part>? parts = null
  )
  {
    return RepairTask.Create(
      id ?? Guid.NewGuid(),
      name ?? "Oil Change",
      laborCost ?? 50.00m,
      estimatedDurationInMins ?? RepairDurationInMinutes._60,
      parts ?? [PartFactory.Create().Value]
    );
  }
}