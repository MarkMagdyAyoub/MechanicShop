using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class CreateWorkOrderRequest
{
  [Required(ErrorMessage = "Spot Is Required")]
  [Range(0 , 3 , ErrorMessage = "Spot Must Be One Of The Following Values: (0 -> A , 1 -> B , 2 -> C , 3 -> D)")]
  public Spot Spot { get; set; }
  
  [Required(ErrorMessage = "Vehicle Id Is Required")]
  public Guid VehicleId { get; set; }
  
  [Required(ErrorMessage = "StartAt Is Required")]
  public DateTimeOffset StartAt { get; set; }

  [MinLength(1, ErrorMessage = "At Least One Repair Task Must Be Selected.")]
  public List<Guid> RepairTaskIds { get; set; } = [];
  
  [Required(ErrorMessage = "Labor Id Is Required")]
  public Guid LaborId { get; set; }
}