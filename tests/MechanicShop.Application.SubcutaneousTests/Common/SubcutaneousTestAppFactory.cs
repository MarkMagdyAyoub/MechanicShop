using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Interfaces;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Settings;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using StackExchange.Redis;
using Testcontainers.PostgreSql;

namespace MechanicShop.Application.SubcutaneousTests.Common;
// Tests require a separate DbContext instance to avoid interfering with the application's real database.
// Each test must operate on an isolated and clean database state.
//
// Using the production DbContext directly is unsafe because it may modify shared or production data.
//
// While in-memory databases can provide isolation, they do not fully replicate PostgreSQL behavior
// and may lead to incorrect test results due to missing features or differences in query translation.
//
// Therefore, we use an ephemeral PostgreSQL instance (e.g., via Testcontainers) to ensure:
// - Full compatibility with the production database engine
// - Clean state per test run or test fixture
//
// The test database is created dynamically, and each test run uses an isolated database instance
// to guarantee deterministic and independent test execution.

public class SubcutaneousTestAppFactory : WebApplicationFactory<Program> , IAsyncLifetime
{
  private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:18-alpine").Build();
  public ISender CreateSender()
  {
    var scope = Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<ISender>();
  }

  public IAppDbContext CreateDbContext()
  {
    var scope = Services.CreateScope();
    return scope.ServiceProvider.GetRequiredService<IAppDbContext>();
  }

  public FakeTimeProvider GetFakeTimeProvider() => (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();

  /// <summary>
  /// Resets the database to a clean state by truncating all relevant tables and clearing cache entries.
  /// </summary>
  /// <returns></returns>
  public async Task ResetDatabaseAsync()
  {
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    await db.Database.ExecuteSqlRawAsync(
      """
        TRUNCATE TABLE
          "InvoiceLineItems",
          "Invoices",
          "WorkOrderRepairTasks",
          "WorkOrders",
          "Vehicles",
          "Customers",
          "RepairTasks",
          "Parts",
          "Employees",
          "RefreshTokens"
        RESTART IDENTITY CASCADE;
      """
    );

    var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();
    await cache.RemoveByTagAsync("customer");
    await cache.RemoveByTagAsync("repair-task");
    await cache.RemoveByTagAsync("work-order");
    await cache.RemoveByTagAsync("invoice");
    await cache.RemoveByTagAsync("dashboard");
    await cache.RemoveByTagAsync("labors");
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
      builder.ConfigureTestServices(services =>
      {
          // Remove the existing DB registration that points to your production connection string
          services.RemoveAll<OverdueWorkOrderBackgroundService>();
          services.RemoveAll<DbContextOptions<AppDbContext>>();
          services.RemoveAll<ApplicationSettings>();
          services.RemoveAll<IConnectionMultiplexer>();
          services.RemoveAll<HybridCache>();
          services.RemoveAll<IDistributedCache>();
          services.RemoveAll<TimeProvider>();

          // without this, tests make live third-party calls during domain event firing
          services.RemoveAll<INotificationService>();

          foreach (var descriptor in services
            .Where(descriptor => descriptor.ImplementationType?.IsGenericType == true
              && descriptor.ImplementationType.GetGenericTypeDefinition() == typeof(CachingBehavior<,>))
            .ToList())
          {
            services.Remove(descriptor);
          }

          // Re-register DbContext using the test container's connection string
          services.AddSingleton<INotificationService, NoOpNotificationService>();
          services.AddSingleton<TimeProvider>(_ => new FakeTimeProvider(DateTimeOffset.UtcNow));
          services.AddDistributedMemoryCache();
          services.AddHybridCache();
          services.AddDbContext<AppDbContext>((serviceProvider , options) =>
          {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(_postgresContainer.GetConnectionString());
          });

          services.PostConfigure<ApplicationSettings>(options =>
          {
            options.OpeningTime = new TimeOnly(9 , 0);
            options.ClosingTime = new TimeOnly(18 , 0);
          });
      });
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    var host = builder.Build();

    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        conn.Open();

        using var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS citext;", conn);
        cmd.ExecuteNonQuery();

        conn.ReloadTypes();
        NpgsqlConnection.ClearPool(conn);
        conn.Close();
    }

    host.Start();
    return host;
  }

  public async Task InitializeAsync() =>
    await _postgresContainer.StartAsync();

  async Task IAsyncLifetime.DisposeAsync()
    => await _postgresContainer.DisposeAsync();
}
