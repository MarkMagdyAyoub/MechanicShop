using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
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

  public static CreateRepairTaskPartCommand CreateCommand(
    string? name = null,
    decimal? cost = null,
    int? quantity = null
  )
    => new CreateRepairTaskPartCommand(
      Name: name ?? "Brake Pads",
      Cost: cost ?? 75.00m,
      Quantity: quantity ?? 1
    );

    public static UpdateRepairTaskPartCommand UpdateCommand(
        Guid? partId = null,
        string? name = null,
        decimal? cost = null,
        int? quantity = null
    )
    => new UpdateRepairTaskPartCommand(
      PartId: partId ?? Guid.NewGuid(),
      Name: name ?? "Brake Pads",
      Cost: cost ?? 75.00m,
      Quantity: quantity ?? 1
    );
}
