using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.PartGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.CreateRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsRepairTaskDto()
    {
        var command = RepairTaskFactory.CreateCommand();

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.Name, result.Value.Name);
        Assert.Equal(command.LaborCost, result.Value.LaborCost);
        Assert.Equal(command.EstimatedDurationInMins, result.Value.EstimatedDurationInMins);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_RepairTaskIsPersistedToDatabase()
    {
        var command = RepairTaskFactory.CreateCommand();

        await _sender.Send(command);

        var saved = await _context.RepairTasks
            .AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Name == command.Name);

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_PartsArePersistedToDatabase()
    {
        var command = RepairTaskFactory.CreateCommand(
            parts: [PartFactory.CreateCommand(), PartFactory.CreateCommand(name: "Brake Pad")]);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);

        var partNames = command.Parts.Select(p => p.Name).ToList();
        var savedPartNames = await _context.Parts
            .Where(p => partNames.Contains(p.Name))
            .Select(p => p.Name)
            .ToListAsync();

        Assert.Equal(partNames.Count, savedPartNames.Count);
        Assert.All(partNames, name => Assert.Contains(name, savedPartNames));
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsDuplicateNameError()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = RepairTaskFactory.CreateCommand(name: existing.Name);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.DuplicateName.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_NothingIsPersistedToDatabase()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var beforeCount = await _context.RepairTasks.CountAsync();

        var command = RepairTaskFactory.CreateCommand(name: existing.Name);
        await _sender.Send(command);

        Assert.Equal(beforeCount, await _context.RepairTasks.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenPartDataIsInvalid_ReturnsDomainErrors()
    {
        var command = RepairTaskFactory.CreateCommand(
            parts: [PartFactory.CreateCommand(name: string.Empty)]);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskDataIsInvalid_ReturnsDomainErrors()
    {
        var command = RepairTaskFactory.CreateCommand(name: string.Empty);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
