using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
  private readonly ILogger<DummyRequest> _logger =
    Substitute.For<ILogger<DummyRequest>>();

  private readonly IUser _user = Substitute.For<IUser>();

  private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

  private readonly LoggingBehavior<DummyRequest> _sut;

  public LoggingBehaviorTests()
  {
    _sut = new LoggingBehavior<DummyRequest>(
      _logger,
      _user,
      _identityService);
  }

  [Fact]
  public async Task Process_UserExists_LogsRequestInformation()
  {
    var request = new DummyRequest();

    var userId = Guid.NewGuid();

    _user.Id.Returns(userId);

    await _sut.Process(request , CancellationToken.None);

    await _identityService.Received(1).GetUserNameAsync(userId , new CancellationToken());

    _logger.Received(1).Log(
      LogLevel.Information,
      Arg.Any<EventId>(),
      Arg.Is<object>(o =>
        o.ToString()!.Contains("Request:")),
      Arg.Any<Exception>(),
      Arg.Any<Func<object, Exception?, string>>()
    );
  }

  [Fact]
  public async Task Process_UserDoesNotExist_DoesNotCallIdentityService()
  {
    var request = new DummyRequest();

    _user.Id.Returns((Guid?)null);

    await _sut.Process(request, CancellationToken.None);

    await _identityService.DidNotReceive().GetUserNameAsync(Arg.Any<Guid>() , new CancellationToken());
  }
}

public class DummyRequest;