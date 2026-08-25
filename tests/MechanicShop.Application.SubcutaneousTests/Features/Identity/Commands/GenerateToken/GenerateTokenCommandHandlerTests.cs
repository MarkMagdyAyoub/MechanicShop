using MechanicShop.Application.Features.Identity.Commands.GenerateToken;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Commands.GenerateToken;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GenerateTokenCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly ISender _sender = factory.CreateSender();
    private const string SeededManagerEmail = "mark@gmail.com";
    private const string SeededManagerPassword = "Manager@1234!";

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsTokenDto()
    {
        // Given
        var command = new GenerateTokenCommand(SeededManagerEmail, SeededManagerPassword);

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
    }

    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ReturnsError()
    {
        // Given
        var command = new GenerateTokenCommand($"unknown-{Guid.NewGuid()}@example.com", "SomePassword123!");

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ReturnsError()
    {
        // Given
        var command = new GenerateTokenCommand(SeededManagerEmail, "WrongPassword123!");

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}