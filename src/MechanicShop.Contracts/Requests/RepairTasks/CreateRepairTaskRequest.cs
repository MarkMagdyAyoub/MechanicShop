using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public sealed class CreateRepairTaskRequest
{
  [Required(ErrorMessage = "Name Of The Repair Task Is Required")]
  public string Name { get; set; } = string.Empty;

  
  [Required(ErrorMessage = "Labor Cost Is Required")]
  public decimal LaborCost { get; set; }

  
  [Required(ErrorMessage = "Estimated Duration Is Required.")]
  public RepairDurationInMinutes EstimatedDurationInMins { get; set; }

  
  [MinLength(1, ErrorMessage = "At Least One Part Is Required.")]
  public List<CreateRepairTaskPartRequest> Parts { get; set; } = [];
}
