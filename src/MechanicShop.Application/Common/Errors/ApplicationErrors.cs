using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
  // the system is available between 6AM to 8PM
  // so you cannot assign WorkOrder outside these intervals
  public static Error WorkOrderOperatingHour(DateTimeOffset startAtUtc , DateTimeOffset endAtUtc) => 
    Error.Conflict(
      code: "ApplicationErrors.WorkOrder.Outside.OperatingHours",
      description: $"The WorkOrder Time ({startAtUtc} : {endAtUtc}) Is Outside Of Source Operating Hours."
    );
  public static Error WorkOrderNotFound => 
    Error.NotFound(
      code: "ApplicationErrors.WorkOrder.NotFound",
      description: "WorkOrder Does Not Found."
    );
  public static Error LaborOccupied => 
    Error.Conflict(
      code: "ApplicationErrors.Labor.Occupied",
      description: "Labor Is Already Occupied During The Requested Time."
    );

  public static Error CustomerNotFound =>
    Error.NotFound(
      code: "ApplicationErrors.Customer.NotFound",
      description: "Customer Does Not Exist."
    );

  public static Error VehicleNotFound =>
    Error.NotFound(
      code: "ApplicationErrors.Vehicle.NotFound",
      description: "Vehicle Does Not Exist."
    );

  public static Error VehicleSchedulingConflict =>
    Error.Conflict(
      code: "ApplicationErrors.Vehicle.OverlappingWorkOrder",
      description: "The Vehicle Already Has An Overlapping WorkOrder."
    );

  public static Error RepairTaskNotFound =>
    Error.NotFound(
      code: "ApplicationErrors.RepairTask.NotFound",
      description: "Repair Task Does Not Exist."
    );

  public static Error WorkOrderMustBeCompletedForInvoicing =>
    Error.Conflict(
      code: "ApplicationErrors.WorkOrder.InvoiceIssuance.InvalidState",
      description: "WorkOrder Must Be In 'Completed' State To Issue An Invoice."
    );

  public static Error InvoiceNotFound => 
    Error.NotFound(
      code: "ApplicationErrors.Invoice.NotFound",
      description: "Invoice Does Not Exist."
    );

  public static Error InvalidRefreshToken =>
    Error.Validation(
      code: "ApplicationErrors.RefreshToken.Expiry.Invalid",
      description: "Expiry Must Be In The Future."
    );
        

  public static readonly Error ExpiredAccessTokenInvalid = 
    Error.Conflict(
        code: "ApplicationErrors.Auth.ExpiredAccessToken.Invalid",
        description: "Expired Access Token Is Not Valid."
    );

  public static readonly Error UserIdClaimInvalid = 
    Error.Conflict(
      code: "ApplicationErrors.Auth.UserIdClaim.Invalid",
      description: "Invalid UserId Claim."
    );

  public static readonly Error RefreshTokenExpired = 
    Error.Conflict(
      code: "ApplicationErrors.Auth.RefreshToken.Expired",
      description: "Refresh Token Is Invalid Or Has Expired."
    );

  public static readonly Error UserNotFound = 
    Error.NotFound(
      code: "ApplicationErrors.Auth.User.NotFound",
      description: "User Not Found."
    );

  public static readonly Error TokenGenerationFailed = 
    Error.Failure(
      code: "ApplicationErrors.Auth.TokenGeneration.Failed",
      description: "Failed To Generate New JWT Token."
    );

  public static Error LaborNotFound =>
    Error.NotFound(
      code: "ApplicationErrors.Employee.LaborNotFound", 
      description: "Labor Does Not Exist."
    );
}
