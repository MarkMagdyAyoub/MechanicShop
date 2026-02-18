using FluentValidation;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;

public sealed class GetDailyScheduleQueryValidator 
  : AbstractValidator<GetDailyScheduleQuery>
{
  public GetDailyScheduleQueryValidator()
  {
    RuleFor(x => x.TimeZone)
      .NotNull()
      .WithMessage("Time zone is required.")
      .Must(BeValidTimeZone)
      .WithMessage("Invalid or unsupported time zone.");

    RuleFor(x => x.ScheduleDate)
      .NotEqual(default(DateOnly))
      .WithMessage("Schedule date is required.");
  }

  private static bool BeValidTimeZone(TimeZoneInfo? timeZone)
  {
    if (timeZone is null)
      return false;

    try
    {
      _ = TimeZoneInfo.FindSystemTimeZoneById(timeZone.Id);
      return true;
    }
    catch
    {
      return false;
    }
  }

}
