using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.ReassignLabor;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class ReassignLaborCommandValidatorTests
{
    private readonly ReassignLaborCommandValidator _validator;

    public ReassignLaborCommandValidatorTests()
    {
        _validator = new ReassignLaborCommandValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new ReassignLaborCommand(Guid.Empty, Guid.NewGuid());

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenLaborIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new ReassignLaborCommand(Guid.NewGuid(), Guid.Empty);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.LaborId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new ReassignLaborCommand(Guid.NewGuid(), Guid.NewGuid());

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}