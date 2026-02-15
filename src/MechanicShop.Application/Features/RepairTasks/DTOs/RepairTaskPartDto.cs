namespace MechanicShop.Application.Features.RepairTasks.DTOs;

public class RepairTaskPartDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public decimal Cost { get; set; }
  public int Quantity { get; set; }
}