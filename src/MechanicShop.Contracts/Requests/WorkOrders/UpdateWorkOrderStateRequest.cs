using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;
public sealed class UpdateWorkOrderStateRequest
{
  [Required(ErrorMessage = "New Work Order State Is Required.")]
  public WorkOrderState NewWorkOrderState { get; set; }
}