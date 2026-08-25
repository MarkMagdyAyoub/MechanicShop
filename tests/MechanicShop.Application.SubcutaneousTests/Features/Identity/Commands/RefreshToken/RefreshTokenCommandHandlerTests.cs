using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.Features.Identity.Commands.RefreshToken;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.RefreshToken;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RefreshTokenCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();
    private const string SeededManagerEmail = "mark@gmail.com";
    private const string SeededManagerPassword = "Manager@1234!";

    [Fact]
    public async Task Handle_WhenTokenIsValid_ReturnsNewTokenDto()
    {
        // Given
        var loginCommand = new GenerateTokenCommand(SeededManagerEmail , SeededManagerPassword);
        var loginResult = await _sender.Send(loginCommand);

        Assert.True(loginResult.IsSuccess);

        var command = new RefreshTokenCommand(
            loginResult.Value.RefreshToken!,
            loginResult.Value.AccessToken!
          );

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
    }

    [Fact]
    public async Task Handle_WhenAccessTokenIsInvalid_ReturnsExpiredAccessTokenInvalidError()
    {
        // Given
        var command = new RefreshTokenCommand("some-refresh-token", "not-a-real-jwt");

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.ExpiredAccessTokenInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenDoesNotMatchAnyRecord_ReturnsRefreshTokenExpiredError()
    {
        // Given
        var loginResult = await _sender.Send(new GenerateTokenCommand(SeededManagerEmail, SeededManagerPassword));
        Assert.True(loginResult.IsSuccess);

        var command = new RefreshTokenCommand(
            Guid.NewGuid().ToString(),
            loginResult.Value.AccessToken!
          );

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RefreshTokenExpired.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenRefreshTokenIsExpired_ReturnsRefreshTokenExpiredError()
    {
        // Given
        var loginResult = await _sender.Send(new GenerateTokenCommand(SeededManagerEmail, SeededManagerPassword));
        Assert.True(loginResult.IsSuccess);

        var affected = await _context.RefreshTokens
            .Where(rt => rt.Token == loginResult.Value.RefreshToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rt => rt.ExpiresOnUtc, DateTimeOffset.UtcNow.AddDays(-1)));

        Assert.Equal(1, affected);

        var command = new RefreshTokenCommand(
            loginResult.Value.RefreshToken!,
            loginResult.Value.AccessToken!);

        var refreshScopeSender = factory.CreateSender();
        var result = await refreshScopeSender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RefreshTokenExpired.Code, result.TopError.Code);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}