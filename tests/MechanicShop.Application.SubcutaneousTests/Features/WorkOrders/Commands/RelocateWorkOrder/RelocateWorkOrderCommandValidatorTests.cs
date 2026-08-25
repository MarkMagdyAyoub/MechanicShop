using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.RelocateWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class RelocateWorkOrderCommandValidatorTests
{
    private readonly RelocateWorkOrderCommandValidator _validator;

    public RelocateWorkOrderCommandValidatorTests()
    {
        _validator = new RelocateWorkOrderCommandValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = new RelocateWorkOrderCommand(Guid.Empty, DateTimeOffset.UtcNow.AddDays(1), Spot.A);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNewStartAtIsInThePast_ShouldHaveValidationError()
    {
        // Given
        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1), Spot.A);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.NewStartAt);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNewSpotIsInvalidEnumValue_ShouldHaveValidationError()
    {
        // Given
        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), (Spot)999);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.NewSpot);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = new RelocateWorkOrderCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), Spot.A);

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}