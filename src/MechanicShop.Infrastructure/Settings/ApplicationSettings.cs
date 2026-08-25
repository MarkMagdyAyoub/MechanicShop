namespace MechanicShop.Infrastructure.Settings;

public sealed class ApplicationSettings
{
  public TimeOnly OpeningTime { get; set; }
  public TimeOnly ClosingTime { get; set; }
  public int DistributedCacheExpiration { get; init; }
  public int LocalCacheExpiration { get; init; }
  public int MinimumAppointmentDurationInMinutes { get; init; }
  public int OverdueBookingCleanupFrequencyMinutes { get; init; }
  public int BookingCancellationThresholdMinutes { get; init; }
  public string AllowedOrigin { get; set; } = null!;
  public string CorsPolicyName { get; set; } = null!;
} 