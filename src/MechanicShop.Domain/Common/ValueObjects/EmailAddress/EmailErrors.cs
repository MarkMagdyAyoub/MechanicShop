namespace MechanicShop.Domain.Common.ValueObjects.EmailAddress;

using MechanicShop.Domain.Common.Results;

public static class EmailErrors
{
  public static Error Required => Error.Validation(
    code: "Email.Required",
    description: "Email Is Required");

  public static Error TooLong => Error.Validation(
    code: "Email.TooLong",
    description: "Email Must Be 100 Characters Or Less");

  public static Error Invalid => Error.Validation(
    code: "Email.Invalid",
    description: "Email Is Invalid");
}
