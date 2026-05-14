using System.ComponentModel.DataAnnotations;
using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public sealed class WorkOrderFilterRequest : IValidatableObject
{
  [Required(ErrorMessage = "Page Number Is Required")]
  [Range(1 , 100 , ErrorMessage = "Page Number Must Be Between 1 And 100")]
  public int PageNumber { get; set; }

  [Required(ErrorMessage = "Page Size Is Required")]
  [Range(1 , 200 , ErrorMessage = "Page Size Must Be Between 1 And 200")]
  public int PageSize { get; set; }
  public string? SearchTerm { get; set; }
  public string SearchColumn { get; set; } = "CreatedAt";
  public string SortDirection { get; set; } = "DESC";
  public WorkOrderState? State { get; set; }
  public Guid? VehicleId { get; set; }
  public Guid? LaborId { get; set; }
  public DateTime? StartDateFrom { get; set; }
  public DateTime? StartDateTo { get; set; }
  public DateTime? EndDateFrom { get; set; }
  public DateTime? EndDateTo { get; set; }
  public Spot? Spot { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (
      StartDateFrom.HasValue &&
      StartDateTo.HasValue &&
      StartDateFrom > StartDateTo
    )
    {
      yield return new ValidationResult(
        "StartDateFrom Must Be Earlier Than StartDateTo",
        [
          nameof(StartDateFrom),
          nameof(StartDateTo)
        ]
      );
    }

    if (
      EndDateFrom.HasValue &&
      EndDateTo.HasValue &&
      EndDateFrom > EndDateTo
    )
    {
      yield return new ValidationResult(
        "EndDateFrom Must Be Earlier Than EndDateTo",
        [
          nameof(EndDateFrom),
          nameof(EndDateTo)
        ]
      );
    }
  }
}