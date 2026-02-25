using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;

public sealed class GetUserInfoByIdQueryValidator : AbstractValidator<GetUserInfoByIdQuery>
{
  public GetUserInfoByIdQueryValidator()
  {
    RuleFor(x => x.UserId)
      .NotNull().NotEmpty()
      .WithErrorCode("UserId_Null_Or_Empty")
      .WithMessage("UserId Cannot Be Null Or Empty");
  }
}