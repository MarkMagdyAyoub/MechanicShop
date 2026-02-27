using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;

public sealed class GetWorkOrderStatisticsQueryValidator : AbstractValidator<GetWorkOrderStatisticsQuery>
{
  public GetWorkOrderStatisticsQueryValidator()
  {
    RuleFor(request => request.Date)
      .NotEmpty()
      .WithErrorCode("Date_Is_Required")
      .WithMessage("Date is required.");
  }
}