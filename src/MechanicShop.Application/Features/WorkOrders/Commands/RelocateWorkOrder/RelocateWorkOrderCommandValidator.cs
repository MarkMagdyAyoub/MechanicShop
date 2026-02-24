using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
  public RelocateWorkOrderCommandValidator()
  {
    RuleFor(x => x.WorkOrderId)
      .NotEmpty()
      .WithMessage("WorkOrder Id Is Required");

    RuleFor(x => x.NewStartAt)
      .NotEmpty()
      .WithMessage("StartAt Is Required.")
      .GreaterThan(DateTimeOffset.UtcNow)
      .WithMessage("New Start Time Must Be In The Future.");

    RuleFor(x => x.NewSpot)
      .IsInEnum()
      .WithErrorCode("Spot_Invalid")
      .WithMessage("Spot Must Be A Valid Spot Value. [A, B, C, D]");
  }
}