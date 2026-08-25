using System.Text.RegularExpressions;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Common.ValueObjects.PhoneNumber;

public sealed partial record PhoneNumber
{
  public string Value { get; }

#pragma warning disable CS8618
  private PhoneNumber()
  {
  }
#pragma warning restore CS8618

  private PhoneNumber(string value)
  {
    Value = value;
  }

  public static Result<PhoneNumber> Create(string phoneNumber)
  {
    if (string.IsNullOrWhiteSpace(phoneNumber))
      return PhoneNumberErrors.Required;

    phoneNumber = RemoveSpaces(phoneNumber.Trim());

    if (!IsValid(phoneNumber))
      return PhoneNumberErrors.Invalid;

    return new PhoneNumber(Normalize(phoneNumber));
  }

  public static bool IsValid(string phoneNumber)
  {
    if (string.IsNullOrWhiteSpace(phoneNumber))
      return false;

    phoneNumber = RemoveSpaces(phoneNumber.Trim());

    return EgyptianMobileRegex().IsMatch(phoneNumber);
  }

  public override string ToString()
    => Value;

  private static string Normalize(string phoneNumber)
  {
    if (phoneNumber.StartsWith("+20", StringComparison.Ordinal))
      return $"0{phoneNumber[3..]}";

    if (phoneNumber.StartsWith("0020", StringComparison.Ordinal))
      return $"0{phoneNumber[4..]}";

    if (phoneNumber.StartsWith('1'))
      return $"0{phoneNumber}";

    return phoneNumber;
  }

  private static string RemoveSpaces(string value)
    => value.Replace(" ", string.Empty);

  [GeneratedRegex(@"^(?:\+20|0020|0)?1[0125][0-9]{8}$")]
  private static partial Regex EgyptianMobileRegex();
}
