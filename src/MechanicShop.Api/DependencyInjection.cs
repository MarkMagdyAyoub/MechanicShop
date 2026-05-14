using MechanicShop.Api.Exceptions;
using MechanicShop.Api.OpenApi.Transformers;
using MechanicShop.Api.Services;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.RealTime;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
namespace MechanicShop.Api;

// TODO: Add output cache policies with tags per resource to support selective invalidation.
// Issue: deleted resources are removed from DB and hybrid cache,
// but stale responses remain in output cache.
// Apply this when implementing write endpoints (POST, PUT, DELETE)
// to evict cache entries when data changes.

// Example policy:
// options.AddPolicy("work-orders", policy =>
// {
//     policy.Expire(TimeSpan.FromSeconds(10));
//     policy.Tag("work-orders");
// });

// Usage (on data change):
// Inject IOutputCacheStore or IOutputCache
// await cache.EvictByTagAsync("work-orders", default);

public static class DependencyInjection
{
  public static IServiceCollection AddPresentation(
    this IServiceCollection services , 
    IConfiguration configuration ,  
    ConfigureHostBuilder host , 
    IHostEnvironment env
  )
  {
    services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
    
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

    services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));

    services
      .AddControllersWithJsonConfiguration()
      .AddCustomRFC9457()
      .AddGlobalExceptionHandler()
      .AddCustomApiVersioning()
      .AddOpenApiSpecification()
      .AddSerilogConfig(configuration , host)
      .AddOpenTelemetryConfig(env)
      .AddIdentity()
      .AddRateLimiting()
      .AddOutputCaching()
      .AddCorsConfiguration(configuration)
      .AddSignalR();

    services.AddControllers();
      
    return services;
  }

  public static async Task<WebApplication> AddDevelopmentDependencies(this WebApplication app)
  {
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
      options.SwaggerEndpoint("/openapi/v1.json" , "v1");
    });
    await app.InitializeDatabaseAsync();
    return app;
  }

  private static IServiceCollection AddControllersWithJsonConfiguration(this IServiceCollection services)
  {
    services.AddControllers()
    .AddJsonOptions(
      options =>
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    );
    return services;
  }

  private static IServiceCollection AddIdentity(this IServiceCollection services)
  {
    services.AddHttpContextAccessor();
    services.AddScoped<IUser , CurrentUser>();
    return services;
  }

  private static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
  {
    services.AddApiVersioning(options =>
    {
      options.DefaultApiVersion = new ApiVersion(1,0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
      options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    })
    .AddVersionedApiExplorer(options =>
    { 
      options.GroupNameFormat = "'v'VVV";
    });
    return services;
  }

  private static IServiceCollection AddOutputCaching(this IServiceCollection services)
  {
    services.AddOutputCache(options =>
    {
      options.AddPolicy("work-orders" , policy =>
      {
        policy.SetVaryByRouteValue([""]).Expire(TimeSpan.FromSeconds(10));
        policy.Tag("ss");
      });
      options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(10)));
      options.MaximumBodySize = 64 * 1024;  // 64 KB
      options.SizeLimit = 50 * 1024 * 1024; // 50 MB
      options.UseCaseSensitivePaths = false;
    });

    return services;
  }

  private static IServiceCollection AddOpenApiSpecification(this IServiceCollection services)
  {
    string[] versions = ["v1"];
    
    foreach(var version in versions)
    {
      services.AddOpenApi(version , options =>
      {
        options.AddDocumentTransformer<VersioningInfoTransformer>();
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        options.AddOperationTransformer<BearerSecuritySchemePerOperationTransformer>();
      });
    }
    services.AddSwaggerGen();
    return services;
  }

  private static IServiceCollection AddCustomRFC9457(this IServiceCollection services)
  {
    services.AddProblemDetails(
      options =>
      {
        options.CustomizeProblemDetails = context =>
        {
          context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
          context.ProblemDetails.Extensions.Add("requestId" , context.HttpContext.TraceIdentifier);
        };
      }
    );
    return services;
  }

  private static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
  {
    services.AddExceptionHandler<GlobalExceptionHandler>();
    return services;
  }

  private static IServiceCollection AddSerilogConfig(this IServiceCollection services , IConfiguration configuration , ConfigureHostBuilder host)
  {
    host.UseSerilog((context , loggerConfiguration) =>
    {
      loggerConfiguration.ReadFrom.Configuration(configuration);
    });
    return services;
  }

  private static IServiceCollection AddOpenTelemetryConfig(this IServiceCollection services , IHostEnvironment env)
  {
      // Setting up observability:
      // - Metrics will go to Prometheus
      // - Traces will be sent to Seq using OpenTelemetry
      // - Dashboards/monitoring handled by Prometheus + Grafana
      var isDev = env.IsDevelopment();
      services.AddOpenTelemetry()
          .ConfigureResource(res => res.AddService("mechanicShop"))

          // Seq OTLP docs: https://seq.readme.io/docs/opentelemetry-net-sdk-1
          .WithTracing(tracing =>
          {
              // Track incoming HTTP requests in ASP.NET Core
              // Includes middleware pipeline, response times, and exceptions
              tracing.AddAspNetCoreInstrumentation()

                  // Track outgoing HTTP requests as well
                  // This lets me see full request flows across services
                  // Note: the app is currently monolithic, so this isn't strictly necessary yet,
                  // but I set it up now to support potential future microservices
                  .AddHttpClientInstrumentation();
            
              // PostgreSQL (Npgsql) instrumentation
              // - Only enabled in development
              // - I use it to measure query latency and trace database calls
              // - I added this especially because the app uses a cloud-hosted database,
              //   so I want to see how network latency affects queries and monitor query performance over time
              if (isDev) 
                tracing.AddNpgsql();

              // Export traces
              // We'll send them to Seq at http://localhost:5341/ingest/otlp/v1/traces
              // Using HTTP + Protobuf
              // NOTE: the actual endpoint is configured via Docker Compose env vars
              tracing.AddOtlpExporter();
          })
          .WithMetrics(metrics =>
          {
            metrics
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation();

            if (isDev)
              metrics.AddNpgsqlInstrumentation();
            
            metrics
              .AddOtlpExporter()
              .AddPrometheusExporter();
          });

      return services;
  }

  private static IServiceCollection AddCorsConfiguration(this IServiceCollection services , IConfiguration configuration)
  {
    var appSettings = configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>()!;

    services.AddCors(
      options =>
      {
        options.AddPolicy(
          appSettings.CorsPolicyName,
          policy =>
          {
            policy.WithOrigins(appSettings.AllowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
          }
        );
      }
    );
    return services;
  }

  private static IServiceCollection AddRateLimiting(this IServiceCollection services)
  {
    services.AddRateLimiter(options =>
    {
      options.AddSlidingWindowLimiter("SlidingWindow" , configOptions =>
      {
        configOptions.Window = TimeSpan.FromMinutes(1);
        configOptions.SegmentsPerWindow = 6; 
        configOptions.PermitLimit = 100;
        configOptions.QueueLimit = 10;
        configOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        configOptions.AutoReplenishment = true;
      });

      options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
    return services;
  }

  public static IApplicationBuilder UseApplicationMiddlewares(this IApplicationBuilder app)
  {
    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    app.UseRateLimiter();

    app.UseAuthentication();
    
    app.UseAuthorization();

    return app;
  }

  public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapControllers();
    
    app.MapHub<WorkOrderHub>(WorkOrderHub.HUB_URL);
    
    return app;
  }
}