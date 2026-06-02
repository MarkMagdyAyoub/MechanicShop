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
      // Factory required by GetOrCreateAsync.
      // Value is a dummy placeholder because DisableUnderlyingData prevents its use.
      // Cache miss is handled manually after this call.
      factory: _ => new ValueTask<TResponse>((TResponse)(object)null!),
      options: new HybridCacheEntryOptions
      {
        // On cache miss, use the factory only.
        // Disables any internal fallback or underlying data resolution mechanisms.
        Flags = HybridCacheEntryFlags.DisableUnderlyingData
      },
      cancellationToken: cancellationToken
    );

    if(result is not null)
      _logger.LogInformation("Cache Hit For `{TRequest}`" , typeof(TRequest));
    else 
    {
      _logger.LogInformation("CACHE MISS for {Request}", typeof(TRequest).Name);
      
      // execute the handler , and if it success then cache the result
      // if not success or throws exception then pass it to exception handler
      result = await next(cancellationToken);

      if(result is IResult { IsSuccess: true })
      {
        await _cache.SetAsync(
          key: cacheRequest.CacheKey,
          value: result,
          options: 
            new HybridCacheEntryOptions
            {
              Expiration = cacheRequest.Expiration
            },
          tags: cacheRequest.Tags,
          cancellationToken: cancellationToken
        );
      }
    }

    return result;
  }
}