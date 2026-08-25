using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderState;

public class UpdateWorkOrderStateCommandValidatorTests
{
    private readonly UpdateWorkOrderStateCommandValidator _validator;

    public UpdateWorkOrderStateCommandValidatorTests()
    {
        _validator = new UpdateWorkOrderStateCommandValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new UpdateWorkOrderStateCommand(Guid.Empty, WorkOrderState.InProgress);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenStateIsInvalidEnumValue_ShouldHaveValidationError()
    {
        // Given
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), (WorkOrderState)999);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.State);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new UpdateWorkOrderStateCommand(Guid.NewGuid(), WorkOrderState.InProgress);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}