using System.Globalization;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Common.ValueObjects.Money;

public sealed record Money
{

  public decimal Amount { get; }
  public Currency Currency { get; }

  private Money()
  {
  }

  private Money(decimal amount, Currency currency)
  {
    Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    Currency = currency;
  }

  public static Result<Money> Create(decimal amount, Currency currency)
  {
    if (amount < 0)
      return MoneyErrors.AmountCannotBeNegative;
    
    if (!IsSupportedCurrency(currency))
      return MoneyErrors.CurrencyInvalid;

    return new Money(amount, currency);
  }

  public static Result<Money> Create(decimal amount, string currency)
  {
    if (string.IsNullOrWhiteSpace(currency))
      return MoneyErrors.CurrencyRequired;

    if (!Enum.TryParse<Currency>(currency , ignoreCase: true , out var parsedCurrency))
      return MoneyErrors.CurrencyInvalid;

    return Create(amount, parsedCurrency);
  }

  public static bool IsSupportedCurrency(Currency currency)
    => Enum.IsDefined(currency);

  public static Result<Money> Zero(Currency currency)
    => Create(0, currency);

  public Result<Money> Add(Money other)
  {
    if (Currency != other.Currency)
      return MoneyErrors.CurrencyMismatch;

    return new Money(Amount + other.Amount, Currency);
  }

  public Result<Money> Subtract(Money other)
  {
    if (Currency != other.Currency)
      return MoneyErrors.CurrencyMismatch;

    if (Amount < other.Amount)
      return MoneyErrors.AmountCannotBeNegative;

    return new Money(Amount - other.Amount, Currency);
  }

  public Money Multiply(int multiplier)
    => new(Amount * multiplier, Currency);

  public override string ToString()
    => string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");
}
