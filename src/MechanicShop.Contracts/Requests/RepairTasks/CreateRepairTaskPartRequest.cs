using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public class CreateRepairTaskPartRequest
{
  [Required(ErrorMessage = "Name Of The Part Is Required")]
  public string Name { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "Cost Of The Part Is Required")]
  [Range(1 , 1000 , ErrorMessage = "The Cost Of The Part Must Be Between 1 And 10,000.")]
  public decimal Cost { get; set; }
  
  [Required(ErrorMessage = "Quantity Of The Part Is Required")]
  [Range(1 , 1000 , ErrorMessage = "The Quantity Of The Part Must Be Between 1 And 10,000.")]
  public int Quantity { get; set; }
}