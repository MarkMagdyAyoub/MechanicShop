using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken;

public sealed class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
{
  public GenerateTokenCommandValidator()
  {
    RuleFor(request => request.Email)
        .NotNull().WithMessage("Email is required")
        .NotEmpty().WithMessage("Email cannot be empty")
        .EmailAddress().WithMessage("Invalid email format");

    RuleFor(request => request.Password)
      .NotNull().NotEmpty()
      .WithErrorCode("Password_Null_Or_Empty")
      .WithMessage("Password Cannot Be Null Or Empty.");
  }
}