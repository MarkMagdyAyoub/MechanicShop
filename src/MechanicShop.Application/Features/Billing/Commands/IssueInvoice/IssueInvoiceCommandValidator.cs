using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
  public IssueInvoiceCommandValidator()
  {
    RuleFor(request => request.WorkOrderId)
      .NotNull()
      .NotEmpty()
      .WithErrorCode("workOrderId_Is_Required")
      .WithMessage("WorkOrder Id Is Required.");
  }
}