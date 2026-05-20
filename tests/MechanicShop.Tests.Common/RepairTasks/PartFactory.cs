using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Tests.Common.PartGenerator;

public static class PartFactory
{
  public static Result<Part> Create(
    Guid? id = null, 
    string? name = null, 
    decimal? cost = null, 
    int? quantity = null
  )
  {
    return Part.Create(
      id ?? Guid.NewGuid(),
      name ?? "Engine Oil",
      cost ?? 25.00m,
      quantity ?? 1
    );
  }
}