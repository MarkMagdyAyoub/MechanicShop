using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandValidator 
  : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
  public UpdateWorkOrderRepairTasksCommandValidator()
  {
    RuleFor(x => x.WorkOrderId)
      .NotEmpty()
      .WithErrorCode("WorkOrderId_Required")
      .WithMessage("Work Order Id is required.");

    RuleFor(x => x.RepairTaskIds)
      .Cascade(CascadeMode.Stop)
      .NotNull()
        .WithErrorCode("RepairTasks_Null")
        .WithMessage("Repair Tasks Collection Cannot Be Null.")
      .NotEmpty()
        .WithErrorCode("RepairTasks_Required")
        .WithMessage("At Least One Repair Task Must Be Provided.")
      .Must(list => list.Distinct().Count() == list.Count)
        .WithErrorCode("RepairTasks_Duplicates")
        .WithMessage("Duplicate repair task ids are not allowed.");
  }
}