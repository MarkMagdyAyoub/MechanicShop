using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.PartGenerator;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.UpdateRepairTask;


[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskPartCommandValidatorTests
{
    private readonly UpdateRepairTaskPartCommandValidator _validator;

    public UpdateRepairTaskPartCommandValidatorTests()
    {
        _validator = new UpdateRepairTaskPartCommandValidator();
    }

    [Fact]
    public void Constructor_WhenValidCommand_ShouldNotReturnError()
    {
        var command = PartFactory.UpdateCommand();

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ShouldReturnError()
    {
        // Arrange
        var command = new UpdateRepairTaskPartCommand(
            PartId: Guid.NewGuid(),
            Name : string.Empty,
            Cost : 10.0m,
            Quantity : 1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Name));
    }

    [Fact]
    public void Constructor_WhenPriceIsNegative_ShouldReturnError()
    {
        // Arrange
        var command = new UpdateRepairTaskPartCommand(
            PartId: Guid.NewGuid(),
            Name : "Brake Pad",
            Cost : -5.0m,
            Quantity : 1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Cost));
    }

    [Fact]
    public void Constructor_WhenQuantityIsZero_ShouldReturnError()
    {
        // Arrange
        var command = new UpdateRepairTaskPartCommand(
            PartId: Guid.NewGuid(),
            Name : "Brake Pad",
            Cost : 10.0m,
            Quantity : 0
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRepairTaskPartCommand.Quantity));
    }
}
