using MechanicShop.Domain.WorkOrders.Enums;
namespace MechanicShop.Application.Features.Scheduling.DTOs;

public sealed class SpotDto
{
  public Spot Spot { get; set; }
  public List<SegmentInfoDto> Segments { get; set; } = null!;
}