using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator 
    : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Repair Task Name Is Required.")
            .MaximumLength(100)
            .WithMessage("Repair Task Name Must Not Exceed 100 Characters.");

        RuleFor(x => x.LaborCost)
            .InclusiveBetween(1, 10_000)
            .WithMessage("Labor Cost Must Be Between 1 And 10,000.");

        RuleFor(x => x.EstimatedDurationInMins)
            .NotNull()
            .WithMessage("Estimated Duration Is Required.")
            .IsInEnum()
            .WithMessage("Estimated Duration Value Is Invalid.");

        RuleFor(x => x.Parts)
            .NotNull()
            .WithMessage("Parts Collection Cannot Be Null.")
            .Must(parts => parts.Count > 0)
            .WithMessage("At Least One Part Must Be Provided.");

        RuleForEach(x => x.Parts)
            .SetValidator(new CreateRepairTaskPartCommandValidator());
    }
}