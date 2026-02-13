using FluentValidation;

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
    
    RuleFor(x => x.Email)
      .NotEmpty()
      .WithMessage("Email Is Required")
      .EmailAddress()
      .WithMessage("Email Is Not Valid")
      .MaximumLength(100)
      .WithMessage("Email Should Be 100 Character Maximum");
    
    RuleFor(x => x.PhoneNumber)
      .NotEmpty()
      .WithMessage("Phone Number Is Required")
      .Matches(@"^(?:\+20|0020|0)?1[0125][0-9]{8}$")
      .WithMessage("Invalid Phone Number");

    RuleFor(x => x.Vehicles)
      .NotNull()
      .WithMessage("Vehicles List Cannot Be Null")
      .Must(p => p.Count >= 1)
      .WithMessage("At Least One Vehicle Is Allowed.");

    RuleForEach(x => x.Vehicles)
      .SetValidator(new CreateVehicleCommandValidator());
  }
}