using FluentValidation;
using MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;
namespace MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;
public sealed class ReassignLaborCommandValidator : AbstractValidator<ReassignLaborCommand>
{
  public ReassignLaborCommandValidator()
  {
    RuleFor(wo => wo.WorkOrderId)
      .NotEmpty()
      .WithErrorCode("WorkOrderId_Required")
      .WithMessage("WorkOrderId Is Required");

    RuleFor(wo => wo.LaborId)
      .NotEmpty()
      .WithErrorCode("LaborId_Required")
      .WithMessage("LaborId is required.");
  }
}