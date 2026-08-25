using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Tests.Common.PartGenerator;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Tests.Common.RepairTaskGenerator;
using MechanicShop.Application.SubcutaneousTests.Common;
namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Command.CreateRepairTask;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandValidatorTests
{
    private readonly CreateRepairTaskCommandValidator _validator;

    public CreateRepairTaskCommandValidatorTests()
    {
        _validator = new CreateRepairTaskCommandValidator();
    }

    [Fact]
    public void Constructor_WhenValidCommand_ShouldNotReturnError()
    {
        // Given
        var command = RepairTaskFactory.CreateCommand();

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenNameIsEmpty_ShouldReturnError()
    {
        // Given
        var command = new CreateRepairTaskCommand(
        Name : string.Empty,
        LaborCost : 100.0m,
        EstimatedDurationInMins : RepairDurationInMinutes._60,
        Parts : [PartFactory.CreateCommand()]
        );

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.Name));
    }

    [Fact]
    public void Constructor_WhenLaborCostIsNegative_ShouldReturnError()
    {
        // Given
        var command = new CreateRepairTaskCommand(
            Name: "Brake Replacement",
            LaborCost: -50.0m,
            EstimatedDurationInMins: RepairDurationInMinutes._60,
            Parts: [PartFactory.CreateCommand()]
        );

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.LaborCost));
    }

    [Fact]
    public void Constructor_WhenEstimatedDurationIsZero_ShouldReturnError()
    {
        // Given
        var command = new CreateRepairTaskCommand(
            Name: "Brake Replacement",
            LaborCost: 100.0m,
            EstimatedDurationInMins: (RepairDurationInMinutes)0,
            Parts: [PartFactory.CreateCommand()]
        );

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.EstimatedDurationInMins));
    }

    [Fact]
    public void Constructor_WhenPartListIsEmpty_ShouldReturnError()
    {
        // Given
        var command = new CreateRepairTaskCommand(
            Name: "Brake Replacement",
            LaborCost: 100.0m,
            EstimatedDurationInMins: RepairDurationInMinutes._60,
            Parts: []
        );

        // When
        var result = _validator.Validate(command);

        // Then
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRepairTaskCommand.Parts));
    }
}
