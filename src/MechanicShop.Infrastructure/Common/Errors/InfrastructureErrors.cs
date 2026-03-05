using MechanicShop.Application.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Infrastructure.Common.Errors;

public static class InfrastructureErrors
{
  public static Error UserEmailNotFound(string email) => Error.NotFound(
    code: "User_Not_Found",
    description: $"User With Email `{UtilityService.MaskEmail(email)}` Not Found"
  );

  public static Error UserEmailNotConfirmed(string email) => Error.NotFound(
    code: "Email_Not_Confirmed",
    description: $"Email `{UtilityService.MaskEmail(email)}` Not Confirmed"
  );

  public static Error InvalidPassword => Error.NotFound(
    code: "Invalid_Login_Attempt",
    description: $"Email/Password Are Incorrect"
  );

  public static Error WorkOrderTooShort(int minimumAppointmentDurationInMinutes) => Error.NotFound(
    code: "WorkOrder_TooShort",
    description: $"WorkOrder Duration Must Be At Least {minimumAppointmentDurationInMinutes} Minutes."
  );
}