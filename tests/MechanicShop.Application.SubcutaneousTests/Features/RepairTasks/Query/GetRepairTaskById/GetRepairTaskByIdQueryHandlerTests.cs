using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenQueryIsValid_ShouldReturnRepairTaskDto()
    {
        // Given
        var repairTask = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        // When
        var result = await _sender.Send(new GetRepairTaskByIdQuery(repairTask.Id));

        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal(repairTask.Id, result.Value.Id);
        Assert.Equal(repairTask.Name, result.Value.Name);
        Assert.Equal(repairTask.LaborCost, result.Value.LaborCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, result.Value.EstimatedDurationInMins);
    }

    [Fact]
    public async Task Handle_WhenQueryIsValid_ShouldReturnAssociatedParts()
    {
        // Given
        var repairTask = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(CancellationToken.None);

        // When
        var result = await _sender.Send(new GetRepairTaskByIdQuery(repairTask.Id));

        // Then
        Assert.True(result.IsSuccess);

        var expectedPartNames = repairTask.Parts.Select(p => p.Name).ToList();
        var returnedPartNames = result.Value.Parts.Select(p => p.Name).ToList();

        Assert.Equal(expectedPartNames.Count, returnedPartNames.Count);
        Assert.All(expectedPartNames, name => Assert.Contains(name, returnedPartNames));
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ReturnsRepairTaskNotFoundError()
    {
        // Given
        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        // When
        var result = await _sender.Send(query);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
