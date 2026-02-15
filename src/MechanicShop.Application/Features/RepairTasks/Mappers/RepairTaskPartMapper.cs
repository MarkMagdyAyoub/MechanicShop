using MechanicShop.Application.Features.RepairTasks.DTOs;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskPartMapper
{
  public static RepairTaskPartDto ToDto(this Part part)
  {
    ArgumentNullException.ThrowIfNull(part);
    return new RepairTaskPartDto
    {
      Id = part.Id,
      Name = part.Name,
      Cost = part.Cost,
      Quantity = part.Quantity
    };
  }
}