using System.ComponentModel.DataAnnotations;
namespace MechanicShop.Contracts.Requests.Customers;
public class CreateCustomerRequest
{
  [Required(ErrorMessage = "Name Is Required")]
  public string Name { get; set; } = string.Empty;

  [Required(ErrorMessage = "Phone Number Is Required")]
  [RegularExpression(@"^(?:\+20|0020|0)?1[0125][0-9]{8}$" , ErrorMessage = "Invalid Phone Number")]
  public string PhoneNumber { get; set; } = string.Empty;

  [Required(ErrorMessage = "Email Is Required")]
  [EmailAddress(ErrorMessage = "Email Is Invalid")]
  public string Email { get; set; } = string.Empty;

  [MinLength(1 , ErrorMessage = "At Least One Vehicle Is Required")]
  public List<CreateVehicleRequest> Vehicles { get; set; } = [];
}