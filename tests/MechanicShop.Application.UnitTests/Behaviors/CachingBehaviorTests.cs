using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.UnitTests.Common;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class CachingBehaviorTests
{
  private readonly HybridCache _cache;
  private readonly ILogger<CachingBehavior<DummyRequest, Result<string>>> _logger;
  private readonly CachingBehavior<DummyRequest , Result<string>> _sub;
  private readonly RequestHandlerDelegate<Result<string>> _next;

  public CachingBehaviorTests()
  {
    _cache = Substitute.For<HybridCache>();
    _logger = Substitute.For<ILogger<CachingBehavior<DummyRequest, Result<string>>>>();
    _next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
    _sub = new CachingBehavior<DummyRequest, Result<string>>(_cache , _logger);
  }

  [Fact]
  public async Task Handle_NotCacheableQuery_ShouldCallNextWithoutUsingCache()
  {
      var request = new DummyRequest();

      var result = await _sub.Handle(request, _next, CancellationToken.None);
      
      await _next.Received(1).Invoke();
      
      await _cache.DidNotReceive()
          .GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<DummyResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>()
          );
  }

  [Fact]
  public async Task Handle_CacheableQueryAndKeyNotExists_CacheAndReturnTheResult()
  {
    var request = new CacheableRequest();
    var response = (Result<string>)request.CacheValue;

    _cache.GetOrCreateAsync(
      key: Arg.Any<string>(),
      factory: Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
      options: Arg.Any<HybridCacheEntryOptions>(),
      cancellationToken: Arg.Any<CancellationToken>()
    )
    .Returns((Result<string>)null!);

    var logger = Substitute.For<ILogger<CachingBehavior<CacheableRequest, Result<string>>>>();
    var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
    var behavior = new CachingBehavior<CacheableRequest , Result<string>>(_cache , logger);

    next.Invoke().Returns(response);

    var result = await behavior.Handle(request , next , CancellationToken.None);

    Assert.Equal(result , response);
    
    await next.Received(1).Invoke();
    
    logger.Received(2).Log(
      LogLevel.Information,
      Arg.Any<EventId>(),
      Arg.Any<object>(),
      Arg.Any<Exception>(),
      Arg.Any<Func<object, Exception?, string>>()
    );

    await _cache.Received(1)
      .GetOrCreateAsync(
        Arg.Any<string>(),
        Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
        Arg.Any<HybridCacheEntryOptions>(),
        Arg.Any<IEnumerable<string>>(),
        Arg.Any<CancellationToken>()
      );
    
    await _cache.Received(1)
      .SetAsync(
        key: request.CacheKey,
        value: response,
        options: Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == request.Expiration),
        tags: Arg.Any<string[]>(),
        cancellationToken: Arg.Any<CancellationToken>()
      );
  }

  [Fact]
  public async Task Handle_CacheableQueryAndKeyExists_ReturnCachedResult()
  {
    var request = new CacheableRequest();
    var response = (Result<string>)request.CacheValue;

    _cache.GetOrCreateAsync(
      key: Arg.Any<string>(),
      factory: Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
      options: Arg.Any<HybridCacheEntryOptions>(),
      cancellationToken: Arg.Any<CancellationToken>()
    )
    .Returns(response);

    var logger = Substitute.For<ILogger<CachingBehavior<CacheableRequest, Result<string>>>>();
    var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
    var behavior = new CachingBehavior<CacheableRequest , Result<string>>(_cache , logger);

    next.Invoke().Returns(response);

    var result = await behavior.Handle(request , next , CancellationToken.None);

    Assert.Equal(result , response);
    
    await next.DidNotReceive().Invoke();
    
    logger.Received(2).Log(
      LogLevel.Information,
      Arg.Any<EventId>(),
      Arg.Any<object>(),
      Arg.Any<Exception>(),
      Arg.Any<Func<object, Exception?, string>>()
    );

    await _cache.Received(1)
      .GetOrCreateAsync(
        Arg.Any<string>(),
        Arg.Any<Func<CancellationToken, ValueTask<Result<string>>>>(),
        Arg.Any<HybridCacheEntryOptions>(),
        Arg.Any<IEnumerable<string>>(),
        Arg.Any<CancellationToken>()
      );
    
    await _cache.DidNotReceive()
      .SetAsync(
        key: request.CacheKey,
        value: response,
        options: Arg.Is<HybridCacheEntryOptions>(o => o.Expiration == request.Expiration),
        tags: Arg.Any<string[]>(),
        cancellationToken: Arg.Any<CancellationToken>()
      );
  }

  [Fact]
  public async Task Handle_ResultIsError_NotCacheTheResult()
  {
    var request = new CacheableRequest();
    var response = (Result<string>)Error.Validation();

    var logger = Substitute.For<ILogger<CachingBehavior<CacheableRequest, Result<string>>>>();
    var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
    var behavior = new CachingBehavior<CacheableRequest , Result<string>>(_cache , logger);

    next.Invoke().Returns(response);

    var result = await behavior.Handle(request , next , CancellationToken.None);

    Assert.False(result.IsSuccess);
    
    await next.Received().Invoke();
  }
}