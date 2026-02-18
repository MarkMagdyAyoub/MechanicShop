using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
  public UpdateCustomerCommandValidator()
  {
    RuleFor(c => c.CustomerId).NotEmpty();
    
    RuleFor(c => c.Name)
      .NotEmpty()
      .WithMessage("Name Is Required")
      .MaximumLength(50)
      .WithMessage("Name Should Be 50 Character Maximum");
    
    RuleFor(c => c.Email)
      .NotEmpty()
      .WithMessage("Email Is Required")
      .EmailAddress()
      .WithMessage("Email Is Not Valid")
      .MaximumLength(100)
      .WithMessage("Email Should Be 100 Character Maximum");
    
    RuleFor(c => c.PhoneNumber)
      .NotEmpty()
      .WithMessage("Phone Number Is Required")
      .Matches(@"^(?:\+20|0020|0)?1[0125][0-9]{8}$")
      .WithMessage("Invalid Phone Number");

    RuleFor(c => c.Vehicles)
      .NotNull()
      .WithMessage("Vehicles List Cannot Be Null")
      .Must(list => list.Count >= 1)
      .WithMessage("At Least One Vehicle Is Allowed.");

    RuleForEach(c => c.Vehicles)
      .SetValidator(new UpdateVehicleCommandValidator());
  }
}