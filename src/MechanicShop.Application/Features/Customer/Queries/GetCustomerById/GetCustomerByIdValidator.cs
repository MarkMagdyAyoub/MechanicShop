using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
  public GetCustomerByIdQueryValidator()
  {
    RuleFor(c => c.CustomerId)
      .NotEmpty()
      .WithErrorCode("CustomerId_Is_Required")
      .WithMessage("Customer Id Is Required");
  }
}