using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.RemoveRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandValidatorTests
{
    private readonly RemoveRepairTaskCommandValidator _validator;

    public RemoveRepairTaskCommandValidatorTests()
    {
        _validator = new RemoveRepairTaskCommandValidator();
    }

    [Fact]
    public void Constructor_WhenValidCommand_ShouldNotReturnError()
    {
        var command = new RemoveRepairTaskCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ShouldReturnError()
    {
        // Given
        var command = new RemoveRepairTaskCommand(Guid.Empty);

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors , e => e.PropertyName == nameof(RemoveRepairTaskCommand.RepairTaskId));
    }
}


