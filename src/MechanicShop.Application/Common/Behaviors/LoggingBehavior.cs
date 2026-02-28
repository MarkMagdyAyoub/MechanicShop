using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class LoggingBehavior<TRequest>(
  ILogger<TRequest> logger,
  IUser user,
  IIdentityService identityService
) : IRequestPreProcessor<TRequest> where TRequest : notnull
{
  private readonly ILogger<TRequest> _logger = logger;
  private readonly IUser _user = user;
  private readonly IIdentityService _identityService = identityService;

  public async Task Process(TRequest request, CancellationToken cancellationToken)
  {
    Guid userId = _user.Id ?? Guid.Empty;
    string? username = string.Empty;
    var requestName = typeof(TRequest).Name;

    if (!userId.Equals(Guid.Empty))
    {
      username = await _identityService.GetUserNameAsync(userId);
    }

    _logger.LogInformation(
      "Request: {RequestName} {UserId} {Username} {@Request}",
      requestName,
      userId,
      username,
      request)
    ;
  }
}