using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;

public class CreateVehicleRequest
{
  [Required(ErrorMessage = "Make Is Required")]
  public string Make { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "Model Is Required")]
  public string Model { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "LicensePlate Is Required")]
  public string LicensePlate { get; set; } = string.Empty;

  [Required(ErrorMessage = "Year Is Required")]
  public int Year { get; set; }
}