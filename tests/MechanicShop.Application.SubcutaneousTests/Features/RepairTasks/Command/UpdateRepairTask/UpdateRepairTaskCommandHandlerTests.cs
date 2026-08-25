using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.PartGenerator;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.UpdateRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsUpdatedResult()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var existingPartId = existing.Parts.First().Id;

        var command = RepairTaskFactory.UpdateCommand(
            existing.Id,
            parts: [PartFactory.UpdateCommand(partId: existingPartId)]);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_RepairTaskDetailsArePersisted()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var existingPartId = existing.Parts.First().Id;

        var command = RepairTaskFactory.UpdateCommand(
            existing.Id,
            name: "Updated Repair Task",
            laborCost: 200.00m,
            parts: [PartFactory.UpdateCommand(partId: existingPartId)]);

        await _sender.Send(command);

        var updated = await _context.RepairTasks
            .AsNoTracking()
            .SingleAsync(rt => rt.Id == existing.Id);

        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.LaborCost, updated.LaborCost);
        Assert.Equal(command.EstimatedDurationInMins, updated.EstimatedDurationInMins);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_PartsAreUpsertedInDatabase()
    {
        var existingPart = PartFactory.Create().Value;
        var existing = RepairTaskFactory.Create(parts: [existingPart]).Value;
        _context.Parts.Add(existingPart);
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var existingPartId = existing.Parts.First().Id;

        var command = RepairTaskFactory.UpdateCommand(
            existing.Id,
            parts: [PartFactory.UpdateCommand(partId: existingPartId, name: "Updated Part Name")]);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);

        var updatedPart = await _context.Parts
            .AsNoTracking()
            .SingleAsync(p => p.Id == existingPartId);

        Assert.Equal("Updated Part Name", updatedPart.Name);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_ReturnsRepairTaskNotFoundError()
    {
        var command = RepairTaskFactory.UpdateCommand(Guid.NewGuid());

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenRepairTaskNotFound_NothingIsPersisted()
    {
        var beforeCount = await _context.RepairTasks.CountAsync();

        var command = RepairTaskFactory.UpdateCommand(Guid.NewGuid());

        await _sender.Send(command);

        Assert.Equal(beforeCount, await _context.RepairTasks.CountAsync());
    }

    [Fact]
    public async Task Handle_WhenRepairTaskDataIsInvalid_ReturnsDomainErrors()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = RepairTaskFactory.UpdateCommand(existing.Id, name: string.Empty);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenPartDataIsInvalid_ReturnsDomainErrors()
    {
        var existing = RepairTaskFactory.Create().Value;
        _context.RepairTasks.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = RepairTaskFactory.UpdateCommand(
            existing.Id,
            parts: [PartFactory.UpdateCommand(name: string.Empty)]);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}
