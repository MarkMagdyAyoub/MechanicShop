using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTasks;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetRepairTasksQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();
    [Fact]
    public async Task Handle_WhenRepairTasksExist_ReturnsAllRepairTasks()
    {
        var seededRepairTasks = new[]
        {
            RepairTaskFactory.Create(name: $"Oil Change {Guid.NewGuid()}").Value,
            RepairTaskFactory.Create(name: $"Brake Service {Guid.NewGuid()}").Value
        };

        _context.RepairTasks.AddRange(seededRepairTasks);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetRepairTasksQuery();

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);

        var returnedIds = result.Value.Select(rt => rt.Id).ToList();

        Assert.All(seededRepairTasks, seeded => Assert.Contains(seeded.Id, returnedIds));
    }

    [Fact]
    public async Task Handle_WhenRepairTasksExist_ReturnsAssociatedParts()
    {
        var existing = RepairTaskFactory.Create(id: Guid.NewGuid() , name: $"Oil Change1 {Guid.NewGuid()}").Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetRepairTasksQuery();

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);

        var returnedRepairTask = result.Value.Single(rt => rt.Id == existing.Id);

        var expectedPartNames = existing.Parts.Select(p => p.Name).ToList();
        var returnedPartNames = returnedRepairTask.Parts.Select(p => p.Name).ToList();

        Assert.Equal(expectedPartNames.Count, returnedPartNames.Count);
        Assert.All(expectedPartNames, name => Assert.Contains(name, returnedPartNames));
    }

    [Fact]
    public async Task Handle_WhenCalled_DoesNotReturnUnknownRepairTask()
    {
        var query = new GetRepairTasksQuery();

        var result = await _sender.Send(query);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, rt => rt.Id == Guid.NewGuid());
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
