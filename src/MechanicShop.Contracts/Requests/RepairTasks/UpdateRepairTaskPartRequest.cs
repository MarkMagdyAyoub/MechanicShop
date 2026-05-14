using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public class UpdateRepairTaskPartRequest
{
  [Required(ErrorMessage = "Id Of The Part Is Required.")]
  public Guid Id { get; set; }
  
  [Required(ErrorMessage = "Name Of The Part Is Required.")]
  public string Name { get; set; } = string.Empty;
  
  [Required(ErrorMessage = "Cost Of The Part Is Required.")]
  [Range(1, 10000, ErrorMessage = "Cost Must Be Between 1 And 10,000.")]
  public decimal Cost { get; set; }
  
  [Required(ErrorMessage = "Quantity Of The Part Is Required.")]
  [Range(1, 100, ErrorMessage = "Quantity Must Be Between 1 And 10,000.")]
  public int Quantity { get; set; }
}