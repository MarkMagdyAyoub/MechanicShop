using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.DTOs;

public class RepairTaskDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public decimal LaborCost { get; set; } 
  public decimal TotalCost { get; set; } 
  public RepairDurationInMinutes EstimatedDurationInMinS { get; set; }
  public List<RepairTaskPartDto> Parts = [];
}