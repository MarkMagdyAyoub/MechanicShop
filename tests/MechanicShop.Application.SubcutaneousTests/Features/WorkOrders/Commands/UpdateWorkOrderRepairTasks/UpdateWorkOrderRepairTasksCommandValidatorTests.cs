using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandValidatorTests
{
    private readonly UpdateWorkOrderRepairTasksCommandValidator _validator;

    public UpdateWorkOrderRepairTasksCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderRepairTasksCommandValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.Empty, [Guid.NewGuid()]);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenRepairTaskIdsIsNull_ShouldHaveValidationError()
    {
        // Given
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), null!);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.RepairTaskIds);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenRepairTaskIdsIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), []);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.RepairTaskIds);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenRepairTaskIdsHasDuplicates_ShouldHaveValidationError()
    {
        // Given
        var duplicateId = Guid.NewGuid();
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [duplicateId, duplicateId]);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.RepairTaskIds);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}