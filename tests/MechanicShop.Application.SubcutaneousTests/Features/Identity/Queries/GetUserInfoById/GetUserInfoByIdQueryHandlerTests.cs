using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfoById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetUserInfoByIdQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly ISender _sender = factory.CreateSender();

    private static readonly Guid SeededManagerId = Guid.Parse("8d7f2f44-8c91-4f7f-a5f2-1d9c3b8a4e11");
    private const string SeededManagerEmail = "mark@gmail.com";

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsUserDto()
    {
        // Given
        var query = new GetUserInfoByIdQuery(SeededManagerId);

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(SeededManagerId, result.Value.userId);
        Assert.Equal(SeededManagerEmail, result.Value.Email);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsError()
    {
        // Given
        var query = new GetUserInfoByIdQuery(Guid.NewGuid());

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.False(result.IsSuccess);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => Task.CompletedTask;
}