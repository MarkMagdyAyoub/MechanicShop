using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.UnitTests.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class UnhandledExceptionHandlerTests
{
  private readonly ILogger<DummyRequest> _logger;
  private readonly DummyRequest _request;
  private readonly DummyResponse _response;
  private readonly UnhandledExceptionBehavior<DummyRequest , int> _sub;

  public UnhandledExceptionHandlerTests()
  {
    _logger = Substitute.For<ILogger<DummyRequest>>();
    _request = Substitute.For<DummyRequest>();
    _response = Substitute.For<DummyResponse>();
    _sub = new UnhandledExceptionBehavior<DummyRequest , int>(_logger);
  }

  [Fact]
  public async Task Handle_ValidRequest_ReturnTheResultOfNextFunction()
  {
    int expectedResponse = 1;
    var next = Substitute.For<RequestHandlerDelegate<int>>();
    next.Invoke().Returns(expectedResponse);

    var result = await _sub.Handle(
      _request,
      next,
      CancellationToken.None
    );
  
    Assert.Equal(expectedResponse , result);
    _logger.DidNotReceive().Log(
      LogLevel.Error,
      Arg.Any<EventId>(),
      Arg.Any<object>(),
      Arg.Any<Exception>(),
      Arg.Any<Func<object , Exception? , string>>()
    );
  }

  [Fact]
  public async Task Handle_ThrowException_LogErrorAndTrows()
  {
    var exception = new Exception("expected failure");
    var next = Substitute.For<RequestHandlerDelegate<int>>();
    next.Invoke().Returns<Task<int>>(_ => throw exception);

    var ex = await Assert.ThrowsAsync<Exception>(() => _sub.Handle(_request , next , CancellationToken.None));

    Assert.Equal(exception , ex);

    _logger.Received(1).Log(
      LogLevel.Error,
      Arg.Any<EventId>(),
      Arg.Any<object>(),
      Arg.Any<Exception>(),
      Arg.Any<Func<object , Exception? , string>>()
    );
  }
}