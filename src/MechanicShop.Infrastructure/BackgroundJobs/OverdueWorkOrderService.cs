using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Settings;
using MechanicShop.Domain.WorkOrders.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Infrastructure.BackgroundJobs;

public sealed class OverdueWorkOrderBackgroundService(
  ILogger<OverdueWorkOrderBackgroundService> logger,
  IServiceScopeFactory serviceScopeFactory,
  IOptions<ApplicationSettings> appSettings,
  TimeProvider timeProvider
) : BackgroundService
{
  private readonly ILogger<OverdueWorkOrderBackgroundService> _logger = logger;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly ApplicationSettings _appSettings = appSettings.Value;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var interval = TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes);
    using var timer = new PeriodicTimer(interval);

    while(await timer.WaitForNextTickAsync(stoppingToken))
    {
      var now = _timeProvider.GetUtcNow();
      
      _logger.LogInformation("Check Overdue Work Orders At `{Now}`," , now);

      try{
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        
        var threshold = _appSettings.BookingCancellationThresholdMinutes;

        var overdueWorkOrders = await context.WorkOrders
                                              .Where(
                                                wo => wo.State == WorkOrderState.Scheduled && 
                                                wo.StartAtUtc.AddMinutes(threshold) <= now
                                              )
                                              .ToListAsync(stoppingToken);

        if(overdueWorkOrders.Count != 0)
        {
          _logger.LogInformation("Found {Count} Overdue Work Orders." , overdueWorkOrders.Count);

          foreach(var wo in overdueWorkOrders)
          {
            var result = wo.Cancel();
            if (result.IsError)
            {
              _logger.LogWarning("Failed To Cancel Work Order With Id `{WorkOrderId}`" , wo.Id);
            }
          }

          await context.SaveChangesAsync(stoppingToken);

        }
        else
        {
          _logger.LogInformation("No Overdue Work Orders Found");
        }
      }catch(Exception ex)
      {
        _logger.LogError(ex , "Error Cleaning Up Overdue Work Orders At {Now}." , now);
      }
    }
  }
}