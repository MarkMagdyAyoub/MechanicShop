using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.UnitTests.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class PerformanceBehaviorTests
{
    private const int SlowRequest = 501;
    private const int FastRequest = 499;
    private const int ExactThreshold = 500;

    private readonly ILogger<DummyRequest> _logger;
    private readonly IUser _user;
    private readonly IExecutionTimer _timer;
    private readonly IIdentityService _identityService;
    private readonly PerformanceBehavior<DummyRequest, DummyResponse> _sut;

    public PerformanceBehaviorTests()
    {
        _logger = Substitute.For<ILogger<DummyRequest>>();
        _user = Substitute.For<IUser>();
        _timer = Substitute.For<IExecutionTimer>();
        _identityService = Substitute.For<IIdentityService>();

        _sut = new PerformanceBehavior<DummyRequest, DummyResponse>(
              _logger, 
              _user, 
              _timer, 
              _identityService
            );
    }


    [Fact]
    public async Task Handle_Always_StartsAndStopsTimer()
    {
        SetupFastRequest();

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        Received.InOrder(() =>
        {
            _timer.Start();
            _timer.Stop();
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(FastRequest)]
    [InlineData(ExactThreshold)]
    public async Task Handle_ElapsedAtOrBelowThreshold_DoesNotLog(long elapsed)
    {
        _timer.ElapsedMilliseconds.Returns(elapsed);

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertNoWarningLogged();
        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<Guid>() , Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ElapsedAboveThreshold_LogsAtWarningLevel()
    {
        SetupSlowRequest(userId: Guid.NewGuid(), username: "any");

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertWarningLoggedExactlyOnce();
    }

    [Fact]
    public async Task Handle_ElapsedAboveThreshold_LogContainsRequestName()
    {
        SetupSlowRequest(userId: Guid.NewGuid(), username: "any");

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertWarningContains(nameof(DummyRequest));
    }

    [Fact]
    public async Task Handle_ElapsedAboveThreshold_LogContainsElapsedMilliseconds()
    {
        SetupSlowRequest(userId: Guid.NewGuid(), username: "any", elapsed: SlowRequest);

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertWarningContains(SlowRequest.ToString());
    }

    [Fact]
    public async Task Handle_ElapsedAboveThreshold_LogContainsUserId()
    {
        var userId = Guid.NewGuid();
        SetupSlowRequest(userId: userId, username: "any");

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertWarningContains(userId.ToString());
    }

    [Fact]
    public async Task Handle_ElapsedAboveThreshold_LogContainsUsername()
    {
        var username = "MarkMagdy";
        SetupSlowRequest(userId: Guid.NewGuid(), username: username);

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        AssertWarningContains(username);
    }

    [Fact]
    public async Task Handle_ElapsedAboveThresholdAndUserIdPresent_FetchesUsername()
    {
        var userId = Guid.NewGuid();
        SetupSlowRequest(userId: userId, username: "MarkMagdy");

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        await _identityService.Received(1).GetUserNameAsync(userId , Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ElapsedAboveThresholdAndUserIdIsNull_DoesNotFetchUsername()
    {
        _user.Id.Returns((Guid?)null);
        _timer.ElapsedMilliseconds.Returns(SlowRequest);

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<Guid>() , Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ElapsedAboveThresholdAndUserIdIsEmptyGuid_DoesNotFetchUsername()
    {
        _user.Id.Returns(Guid.Empty);
        _timer.ElapsedMilliseconds.Returns(SlowRequest);

        await _sut.Handle(new DummyRequest(), FastNext(), CancellationToken.None);

        await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<Guid>() , Arg.Any<CancellationToken>());
        AssertWarningLoggedExactlyOnce();
    }

    [Fact]
    public async Task Handle_Always_ReturnsNextHandlerResponse()
    {
        SetupFastRequest();
        var expected = new DummyResponse();

        var result = await _sut.Handle(
            new DummyRequest(), _ => Task.FromResult(expected), CancellationToken.None);

        Assert.Equal(expected, result);
    }

    private void SetupFastRequest() =>
        _timer.ElapsedMilliseconds.Returns(FastRequest);

    private void SetupSlowRequest(Guid userId, string username, long elapsed = SlowRequest)
    {
        _user.Id.Returns(userId);
        _identityService.GetUserNameAsync(userId , new CancellationToken()).Returns(username);
        _timer.ElapsedMilliseconds.Returns(elapsed);
    }

    private static RequestHandlerDelegate<DummyResponse> FastNext()
        => _ => Task.FromResult(new DummyResponse());

    private void AssertNoWarningLogged() =>
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

    private void AssertWarningLoggedExactlyOnce() =>
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

    private void AssertWarningContains(string fragment) =>
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(fragment)),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
}