using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class ReassignWorkOrderLaborRequest
{
  [Required(ErrorMessage = "Labor Id Is Required")]
  public Guid LaborId { get; set; }
}