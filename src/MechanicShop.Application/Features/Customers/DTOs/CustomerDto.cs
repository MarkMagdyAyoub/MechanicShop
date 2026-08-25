namespace MechanicShop.Application.Features.Customers.DTOs;
public sealed class CustomerDto
{
  public Guid CustomerId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? PhoneNumber { get; set; }
  public string? Email { get; set; }
  public List<VehicleDto> Vehicles { get; set; } = [];
}

