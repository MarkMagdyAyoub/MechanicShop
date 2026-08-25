using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.PartGenerator;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.CreateRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateRepairTaskPartCommandValidatorTests
{
  private readonly CreateRepairTaskPartCommandValidator _validator;

  public CreateRepairTaskPartCommandValidatorTests()
  {
    _validator = new CreateRepairTaskPartCommandValidator();
  }

  [Fact]
    public void Constructor_WhenValidCommand_ShouldNotReturnError()
    {
        // Arrange
        var command = PartFactory.CreateCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

  [Fact]
  public void Constructor_WhenNameIsEmpty_ShouldReturnError()
  {
    // Arrange
    var command = new CreateRepairTaskPartCommand(
      Name : string.Empty,
      Cost : 10.0m,
      Quantity : 1
    );

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Name));
  }

  [Fact]
  public void Constructor_WhenPriceIsNegative_ShouldReturnError()
  {
    // Arrange
    var command = new CreateRepairTaskPartCommand(
      Name : "Brake Pad",
      Cost : -5.0m,
      Quantity : 1
    );

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Cost));
  }

  [Fact]
  public void Constructor_WhenQuantityIsZero_ShouldReturnError()
  {
    // Arrange
    var command = new CreateRepairTaskPartCommand(
      Name : "Brake Pad",
      Cost : 10.0m,
      Quantity : 0
    );

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskPartCommand.Quantity));
  }
}
