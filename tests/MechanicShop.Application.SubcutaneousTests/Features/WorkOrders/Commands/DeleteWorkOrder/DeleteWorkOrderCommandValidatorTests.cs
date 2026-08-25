using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.DeleteWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class DeleteWorkOrderCommandValidatorTests
{
    private readonly DeleteWorkOrderCommandValidator _validator;

    public DeleteWorkOrderCommandValidatorTests()
    {
        _validator = new DeleteWorkOrderCommandValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new DeleteWorkOrderCommand(Guid.Empty);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsProvided_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new DeleteWorkOrderCommand(Guid.NewGuid());

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}