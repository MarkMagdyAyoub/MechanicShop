using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
  public UpdateWorkOrderStateCommandValidator()
  {
    RuleFor(wo => wo.WorkOrderId)
      .NotEmpty()
      .WithMessage("WorkOrder Id Is Required");

    RuleFor(wo => wo.State)
      .IsInEnum()
      .WithErrorCode("WorkOrderState_Invalid")
      .WithMessage("WorkOrder State Is Invalid.");
  }
}