using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.CustomerGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomers;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetCustomersQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenCustomersExist_ReturnsAllCustomers()
    {
        var first = CustomerFactory.Create(
            email: "first@example.com",
            phoneNumber: "01250479826"
        ).Value;

        var second = CustomerFactory.Create(
            email: "second@example.com",
            phoneNumber: "01250479827"
        ).Value;

        _context.Customers.AddRange(first, second);
        await _context.SaveChangesAsync(CancellationToken.None);


        var query = new GetCustomersQuery();

        var result = await _sender.Send(query);
        var list = result.Value;

        Assert.True(result.IsSuccess);

        var returnedIds = result.Value.Select(c => c.CustomerId).ToList();
        Assert.Contains(first.Id, returnedIds);
        Assert.Contains(second.Id, returnedIds);
    }

    [Fact]
    public async Task Handle_WhenCustomersExist_ReturnsAssociatedVehicles()
    {
        var existing = CustomerFactory.Create().Value;
        _context.Customers.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);


        var query = new GetCustomersQuery();

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);

        var returnedCustomer = result.Value.Single(c => c.CustomerId == existing.Id);

        var expectedPlates = existing.Vehicles.Select(v => v.LicensePlate).ToList();
        var returnedPlates = returnedCustomer.Vehicles.Select(v => v.LicensePlate).ToList();

        Assert.Equal(expectedPlates.Count, returnedPlates.Count);
        Assert.All(expectedPlates, plate => Assert.Contains(plate, returnedPlates));
    }

    [Fact]
    public async Task Handle_WhenNoCustomersExist_ReturnsEmptyList()
    {
        var query = new GetCustomersQuery();

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
