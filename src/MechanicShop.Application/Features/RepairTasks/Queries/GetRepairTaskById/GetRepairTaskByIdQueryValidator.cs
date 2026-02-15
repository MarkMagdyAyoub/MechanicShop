namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;

using FluentValidation;

public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(request => request.RepairTaskId)
            .NotEmpty()
            .WithMessage("RepairTaskId Is Required.");
    }
}