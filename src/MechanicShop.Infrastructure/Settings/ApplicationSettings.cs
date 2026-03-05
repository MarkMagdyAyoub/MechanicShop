namespace MechanicShop.Infrastructure.Settings;

public sealed class ApplicationSettings
{
  public TimeOnly OpeningTime { get; init; }
  public TimeOnly ClosingTime { get; init; }
  public int DistributedCacheExpiration { get; init; }
  public int LocalCacheExpiration { get; init; }
  public int MinimumAppointmentDurationInMinutes { get; init; }
} 