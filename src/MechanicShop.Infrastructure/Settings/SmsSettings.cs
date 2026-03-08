namespace MechanicShop.Infrastructure.Settings;

public sealed class SmsSettings
{
  public string AccountSid { get; set; } = default!;
  public string AuthToken { get; set; } = default!;
  public string FromNumber { get; set; } = default!;
}