using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class RelocateWorkOrderRequest
{
  [Required(ErrorMessage = "New Start At Is Required")]
  public DateTimeOffset NewStartAt { get; set; }

  [Required(ErrorMessage = "New Spot At Is Required")]
  public Spot NewSpot { get; set; }
}