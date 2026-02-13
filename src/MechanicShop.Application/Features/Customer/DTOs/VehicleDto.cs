namespace MechanicShop.Application.Features.Customers.DTOs;
public sealed class VehicleDto
{
  public Guid Id { get; set; } 
  public string Model { get; set; } = string.Empty;
  public string LicensePlate { get; set; } = string.Empty;
  public int Year { get; set; } 
}