using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
  HybridCache cache, 
  ILogger<CachingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
  private readonly HybridCache _cache = cache;
  private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    if(request is not ICachedQuery cacheRequest)
      return await next(cancellationToken);

    _logger.LogInformation($"Checking Cache For {typeof(TRequest).Name}");

    var result = await _cache.GetOrCreateAsync(
      key: cacheRequest.CacheKey,
      factory: async ct =>
      {
        var innerResult = await next(ct);
        if(innerResult is IResult r && r.IsSuccess)
          return innerResult;
        
        return default!;
      },
      options: new HybridCacheEntryOptions
      {
        Expiration = cacheRequest.Expiration
      },
      tags: cacheRequest.Tags,
      cancellationToken: cancellationToken
    );

    return result;
  }
}