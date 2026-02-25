using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken;

public sealed class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
{
  public GenerateTokenCommandValidator()
  {
    RuleFor(request => request.Email)
      .NotNull()
      .NotEmpty()
      .WithErrorCode("Email_Null_Or_Empty")
      .WithMessage("Email Cannot Be Null Or Empty");

    RuleFor(request => request.Password)
      .NotNull().NotEmpty()
      .WithErrorCode("Password_Null_Or_Empty")
      .WithMessage("Password Cannot Be Null Or Empty.");
  }
}