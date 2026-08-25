using System.Net.Mail;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Common.ValueObjects.EmailAddress;

public sealed record EmailAddress
{
  public const int MaxLength = 100;

  public string Value { get; }

#pragma warning disable CS8618
  private EmailAddress()
  {
  }
#pragma warning restore CS8618

  private EmailAddress(string value)
  {
    Value = value;
  }

  public static Result<EmailAddress> Create(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      return EmailErrors.Required;

    email = email.Trim().ToLowerInvariant();

    if (email.Length > MaxLength)
      return EmailErrors.TooLong;

    if (!IsValid(email))
      return EmailErrors.Invalid;

    return new EmailAddress(email);
  }

  public static bool IsValid(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      return false;

    email = email.Trim();

    if (email.Length > MaxLength)
      return false;

    try
    {
      var mailAddress = new MailAddress(email);

      return mailAddress.Address.Equals(email, StringComparison.OrdinalIgnoreCase)
        && email.Count(c => c == '@') == 1;
    }
    catch
    {
      return false;
    }
  }

  public override string ToString()
    => Value;
}
