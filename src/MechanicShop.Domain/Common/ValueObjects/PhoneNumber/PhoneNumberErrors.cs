using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Common.ValueObjects.PhoneNumber;

public static class PhoneNumberErrors
{
  public static Error Required => Error.Validation(
    code: "PhoneNumber.Required",
    description: "Phone Number Is Required");

  public static Error Invalid => Error.Validation(
    code: "PhoneNumber.Invalid",
    description: "Phone Number Must Be A Valid Egyptian Mobile Number");
}
