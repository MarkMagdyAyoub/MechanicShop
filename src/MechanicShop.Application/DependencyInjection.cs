using System.Reflection;
using FluentValidation;
using MechanicShop.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
namespace MechanicShop.Application;
public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
      config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
      config.AddOpenBehavior(typeof(LoggingBehavior<>));
      config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
      config.AddOpenBehavior(typeof(ValidationBehavior<,>));
      config.AddOpenBehavior(typeof(CachingBehavior<,>));
    });


    return services;
  }
}