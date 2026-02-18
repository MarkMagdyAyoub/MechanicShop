namespace MechanicShop.Application.Features.Scheduling.DTOs;

public sealed class ScheduleDto
{
  public DateOnly OnDate { get; set; }
  public bool IsPastDate { get; set; }
  public List<SpotDto> Spots { get; set; } = [];
}