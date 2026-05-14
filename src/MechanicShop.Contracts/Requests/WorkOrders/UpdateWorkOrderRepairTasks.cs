using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.WorkOrders;
public sealed class UpdateWorkOrderRepairTasksRequest
{
  [MinLength(1 , ErrorMessage = "At Least One Repair Task Should Be Provided")]
  public List<Guid> NewRepairTaskIds { get; set; } = [];
}