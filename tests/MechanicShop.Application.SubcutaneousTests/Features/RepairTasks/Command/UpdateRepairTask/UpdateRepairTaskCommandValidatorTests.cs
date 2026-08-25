namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.UpdateRepairTask;

using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandValidatorTests
{
    private readonly UpdateRepairTaskCommandValidator _validator;

    public UpdateRepairTaskCommandValidatorTests()
    {
        _validator = new UpdateRepairTaskCommandValidator();
    }
    

    [Fact]
    public void Constructor_WhenValidCommand_ShouldNotReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand();

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenRepairTaskIdIsEmpty_ShouldReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand(repairTaskId: Guid.Empty);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.RepairTaskId));
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ShouldReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand(name: string.Empty);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.Name));
    }

    [Fact]
    public void Constructor_WhenLaborCostIsNegative_ShouldReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand(laborCost: -50.0m);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.LaborCost));
    }

    [Fact]
    public void Constructor_WhenEstimatedDurationIsZero_ShouldReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand(estimatedDurationInMins: 0);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.EstimatedDurationInMins));
    }

    [Fact]
    public void Constructor_WhenPartListIsEmpty_ShouldReturnError()
    {
        // Given
        var command = RepairTaskFactory.UpdateCommand(parts: []);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskCommand.Parts));
    }
}
