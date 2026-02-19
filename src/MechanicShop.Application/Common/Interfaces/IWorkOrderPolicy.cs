using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderPolicy
{
  public bool IsOutSideWorkingHours(DateTimeOffset startAt , TimeSpan duration);
  public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt , DateTimeOffset endAt);
}