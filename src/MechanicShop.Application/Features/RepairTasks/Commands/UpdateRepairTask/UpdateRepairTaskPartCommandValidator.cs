using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;


public sealed class UpdateRepairTaskPartCommandValidator : AbstractValidator<UpdateRepairTaskPartCommand>
{
  public UpdateRepairTaskPartCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Part Name Is Required.")
      .MaximumLength(100)
      .WithMessage("Part Name Must Not Exceed 100 Characters.");

    RuleFor(x => x.Cost)
      .InclusiveBetween(1, 10_000)
      .WithMessage("Cost Must Be Between 1 And 10,000.");

    RuleFor(x => x.Quantity)
      .InclusiveBetween(1, 10)
      .WithMessage("Quantity Must Be Between 1 And 10.");
  }
}