using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
{
  public DeleteWorkOrderCommandValidator()
  {
    RuleFor(wo => wo.WorkOrderId)
      .NotEmpty()
      .WithErrorCode("WorkOrderId_Required")
      .WithMessage("WorkOrder Id Required");
  }
}