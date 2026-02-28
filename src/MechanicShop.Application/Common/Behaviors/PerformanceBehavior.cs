using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;


public class PerformanceBehavior<TRequest, TResponse>(
  ILogger<TRequest> logger,
  IUser user,
  IIdentityService identityService
) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly ILogger<TRequest> _logger = logger;
  private readonly Stopwatch _timer = new Stopwatch();
  private readonly IUser _user = user;
  private readonly IIdentityService _identityService = identityService;
  private const int MAXIMUM_EXPECTED_DURATION_MS  = 500;

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    _timer.Start();

    var response = await next(cancellationToken);

    _timer.Stop();

    var elapsedMilliseconds = _timer.ElapsedMilliseconds;
    
    if(elapsedMilliseconds > MAXIMUM_EXPECTED_DURATION_MS)
    {
      var userId = _user.Id ?? Guid.Empty;
      string? username = string.Empty;
      var requestName = typeof(TRequest).Name;
      if (!userId.Equals(Guid.Empty))
      {
        username = await _identityService.GetUserNameAsync(userId);
      }
      _logger.LogWarning(
        "Long Running Request: {RequestName} takes `{Duration} Milliseconds` {UserId} {UserName} {@Request}" , 
        requestName , 
        elapsedMilliseconds , 
        userId , 
        username,
        request
      );
    }

    return response;
  }
}