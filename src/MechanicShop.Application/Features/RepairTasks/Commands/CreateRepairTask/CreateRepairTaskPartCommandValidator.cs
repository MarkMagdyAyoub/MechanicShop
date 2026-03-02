using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskPartCommandValidator 
    : AbstractValidator<CreateRepairTaskPartCommand>
{
    public CreateRepairTaskPartCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Part Name Is Required.")
            .MaximumLength(100)
            .WithMessage("Part Name Must Not Exceed 100 Characters.");

        RuleFor(x => x.Cost)
            .InclusiveBetween(1, 100_000)
            .WithMessage("Cost Must Be Between 1 And 100,000.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 100)
            .WithMessage("Quantity Must Be Between 1 And 100.");
    }
}
