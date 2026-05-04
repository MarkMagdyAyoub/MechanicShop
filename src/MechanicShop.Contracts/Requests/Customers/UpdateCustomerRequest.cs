using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;
public class UpdateCustomerRequest
{
  [Required(ErrorMessage = "Name Is Required.")]
  public string Name { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "Phone Number Is Required.")]
  public string PhoneNumber { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "Email Is Required.")]
  public string Email { get; set; } = string.Empty;

  [MinLength(1 , ErrorMessage = "At Least One Vehicle Is Required")]
  public List<UpdateVehicleRequest> Vehicles { get; set; } = [];
}