using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateVehicleCommandValidator : AbstractValidator<UpdateVehicleCommand>
{
  public UpdateVehicleCommandValidator()
  {
    RuleFor(v => v.Make)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(v => v.Model)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(v => v.LicensePlate)
      .NotEmpty()
      .MaximumLength(10);
  }
}