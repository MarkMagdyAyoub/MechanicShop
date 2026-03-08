namespace MechanicShop.Infrastructure.Settings;

public class EmailSettings
{
  public string DisplayName { get; set; } = default!;
  public string From { get; set; } = default!;
  public string Host { get; set; } = default!;
  public int Port { get; set; }
  public string UserName { get; set; } = default!;
  public string Password { get; set; } = default!;
}