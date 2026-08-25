using FluentValidation;
using MechanicShop.Domain.Common.ValueObjects.EmailAddress;
using MechanicShop.Domain.Common.ValueObjects.PhoneNumber;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
  public CreateCustomerCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name Is Required")
      .MaximumLength(50)
      .WithMessage("Name Should Be 50 Character Maximum");
    
    RuleFor(x => x.PhoneNumber)
      .Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumber.IsValid(phone))
      .WithMessage(PhoneNumberErrors.Invalid.Description);

    RuleFor(x => x.Email)
      .Must(email => string.IsNullOrWhiteSpace(email) || EmailAddress.IsValid(email))
      .WithMessage("Email Is Not Valid");

    RuleFor(x => x.Vehicles)
      .NotNull()
      .WithMessage("Vehicles List Cannot Be Null")
      .Must(p => p.Count >= 1)
      .WithMessage("At Least One Vehicle Is Allowed.");

    RuleForEach(x => x.Vehicles)
      .SetValidator(new CreateVehicleCommandValidator());
  }
}
