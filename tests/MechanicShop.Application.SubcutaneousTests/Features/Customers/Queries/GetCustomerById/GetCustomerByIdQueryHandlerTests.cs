using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.CustomerGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetCustomerByIdQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsCustomerDto()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetCustomerByIdQuery(existing.Id);

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value.CustomerId);
        Assert.Equal(existing.Name, result.Value.Name);
        Assert.Equal(existing.Email!.Value, result.Value.Email);
        Assert.Equal(existing.PhoneNumber!.Value, result.Value.PhoneNumber);
    }

    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsAssociatedVehicles()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetCustomerByIdQuery(existing.Id);

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);

        var expectedPlates = existing.Vehicles.Select(v => v.LicensePlate).ToList();
        var returnedPlates = result.Value.Vehicles.Select(v => v.LicensePlate).ToList();

        Assert.Equal(expectedPlates.Count, returnedPlates.Count);
        Assert.All(expectedPlates, plate => Assert.Contains(plate, returnedPlates));
    }

    [Fact]
    public async Task Handler_WhenCustomerIsNotInDb_ReturnCustomerNotFoundError()
    {
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var result = await _sender.Send(query);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.CustomerNotFound.Code, result.TopError.Code);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
