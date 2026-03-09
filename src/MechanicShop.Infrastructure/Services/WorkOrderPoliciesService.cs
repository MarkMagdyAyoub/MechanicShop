using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Infrastructure.Common.Errors;
using MechanicShop.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public sealed class WorkOrderPoliciesService(
  IOptions<ApplicationSettings> applicationSettings
) : IWorkOrderPolicy
{
  private readonly ApplicationSettings _applicationSettings = applicationSettings.Value;

  public bool IsOutSideWorkingHours(DateTimeOffset startAt, TimeSpan duration)
  {
      var opening = _applicationSettings.OpeningTime;  
      var closing = _applicationSettings.ClosingTime;  

      var startTime = TimeOnly.FromDateTime(startAt.DateTime);
      var endTime = startTime.Add(duration);

      return startTime < opening || endTime > closing;
  }

  public Result<Success> ValidateMinimumRequirement(DateTimeOffset startAt, DateTimeOffset endAt)
  {
    if((endAt - startAt) < TimeSpan.FromMinutes(_applicationSettings.MinimumAppointmentDurationInMinutes))
    {
      return InfrastructureErrors.WorkOrderTooShort(_applicationSettings.MinimumAppointmentDurationInMinutes);
    }

    return Result.Success;
  }
}