using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator 
  : AbstractValidator<CreateWorkOrderCommand>
{
  public CreateWorkOrderCommandValidator()
  {
    RuleFor(wo => wo.Spot)
      .NotNull()
      .WithMessage("Spot is required.")
      .IsInEnum()
      .WithMessage("Spot value is invalid.");

    RuleFor(wo => wo.RepairTaskIds)
      .NotNull()
      .WithMessage("Repair tasks list is required.")
      .Must(ids => ids.Count > 0)
      .WithMessage("At Least One Repair Task Is Required")
      .Must(ids => ids!.Distinct().Count() == ids.Count)
      .WithMessage("Duplicate Repair Tasks Are Not Allowed.");

    RuleFor(wo => wo.VehicleId)
      .NotEmpty()
      .WithMessage("VehicleId Is Required.")
      .NotEqual(Guid.Empty)
      .WithMessage("VehicleId Is Invalid.");

    RuleFor(wo => wo.LaborId)
      .NotEmpty()
      .WithMessage("LaborId Is Required.")
      .NotEqual(Guid.Empty)
      .WithMessage("LaborId Is Invalid.");

    RuleFor(wo => wo.StartAt)
      .NotEmpty()
      .WithMessage("StartAt is required.")
      .GreaterThan(DateTimeOffset.UtcNow)
      .WithMessage("StartAt must be in the future.");
  }
}
