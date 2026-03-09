using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Data.Interceptors;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Identity.Policies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Hybrid;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using MechanicShop.Infrastructure.Services;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.RealTime;

namespace MechanicShop.Infrastructure;

public static class DependencyInjectionExtension
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services , IConfiguration configuration)
  {
    services.AddSingleton(TimeProvider.System);

    services.AddSignalR();

    services.AddHttpContextAccessor();

    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    
    var settings = configuration
        .GetRequiredSection("ApplicationSettings")
        .Get<ApplicationSettings>()!;

    var connectionString = configuration.GetConnectionString("DefaultConnection");

    var jwtSettings = configuration.GetSection("JwtSettings");
    
    ArgumentNullException.ThrowIfNull(connectionString);
    
    ArgumentNullException.ThrowIfNull(jwtSettings);

    ArgumentNullException.ThrowIfNull(settings);

    services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
    
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

    services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));

    services.AddScoped<ISaveChangesInterceptor , UpdatedEntitiesInterceptor>();

    services.AddDbContext<AppDbContext>((serviceCollection , options) =>
    {
      options.AddInterceptors(serviceCollection.GetServices<ISaveChangesInterceptor>());
      options.UseNpgsql(connectionString);
    });

    services.AddScoped<IAppDbContext>(serviceCollection => serviceCollection.GetRequiredService<AppDbContext>());

    services
    .AddAuthentication(
      options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      }
    )
    .AddJwtBearer(
      options =>
      {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        var key = jwtSettings["Key"] 
                  ?? throw new InvalidOperationException("JWT Key is missing");

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateAudience = true,
          ValidateIssuer = true,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtSettings["Issuer"],
          ValidAudience = jwtSettings["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(
            key: Encoding.UTF8.GetBytes(key)
          )
        };
      } 
    );

    services.AddScoped<IAuthorizationHandler, OwnWorkOrderAccessRequirementHandler>();

    services.AddAuthorization(
      options =>
      {
        options.AddPolicy("ManagerOnly" , policy => policy.RequireRole("Manager"));
        options.AddPolicy("OwnWorkOrderAccess" , policy => policy.Requirements.Add(new OwnWorkOrderAccessRequirement()));
      }
    );

    services.AddIdentityCore<ApplicationUser>(
      options =>
      {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredUniqueChars = 1;
        options.SignIn.RequireConfirmedEmail = true;
      }
    )
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();

    services.AddHybridCache(
      options =>
      {
        options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
          Expiration = TimeSpan.FromMinutes(settings.DistributedCacheExpiration),
          LocalCacheExpiration = TimeSpan.FromMinutes(settings.LocalCacheExpiration)
        };
      }
    );

    services.AddScoped<ITokenProvider , TokenProvider>();

    services.AddScoped<IIdentityService , IdentityService>();

    services.AddScoped<IWorkOrderPolicy , WorkOrderPoliciesService>();

    services.AddScoped<IPdfGenerator , PdfGenerator>();

    services.AddScoped<INotificationService , NotificationService>();

    services.AddScoped<IWorkOrderNotifier , WorkOrderNotifierSignalR>();

    services.AddHostedService<OverdueWorkOrderBackgroundService>();

    return services;
  }
}