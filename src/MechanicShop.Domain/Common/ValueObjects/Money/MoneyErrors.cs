using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Common.ValueObjects.Money;
public static class MoneyErrors
{
  public static Error AmountCannotBeNegative => Error.Validation(
    code: "Money.Amount.Negative",
    description: "Money Amount Cannot Be Negative");

  public static Error CurrencyRequired => Error.Validation(
    code: "Money.Currency.Required",
    description: "Money Currency Is Required");

  public static Error CurrencyInvalid => Error.Validation(
    code: "Money.Currency.Invalid",
    description: "Money Currency Must Be Either USD Or EGP");

  public static Error CurrencyMismatch => Error.Validation(
    code: "Money.Currency.Mismatch",
    description: "Cannot Calculate Money Values With Different Currencies");
}
