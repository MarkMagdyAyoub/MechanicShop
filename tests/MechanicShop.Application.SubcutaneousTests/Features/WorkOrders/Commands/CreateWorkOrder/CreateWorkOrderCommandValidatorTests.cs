using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator;

    public CreateWorkOrderCommandValidatorTests()
    {
        _validator = new CreateWorkOrderCommandValidator();
    }

    private static CreateWorkOrderCommand ValidCommand() => new(
        Spot.A,
        Guid.NewGuid(),
        DateTimeOffset.UtcNow.AddDays(1),
        [Guid.NewGuid()],
        Guid.NewGuid());

    [Fact]
    public void Constructor_WhenSpotIsInvalidEnumValue_ShouldHaveValidationError()
    {
        // Given
        var command = ValidCommand() with { Spot = (Spot)999 };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Spot);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenRepairTaskIdsIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = ValidCommand() with { RepairTaskIds = [] };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.RepairTaskIds);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenRepairTaskIdsHasDuplicates_ShouldHaveValidationError()
    {
        // Given
        var duplicateId = Guid.NewGuid();
        var command = ValidCommand() with { RepairTaskIds = [duplicateId, duplicateId] };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.RepairTaskIds);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenVehicleIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = ValidCommand() with { VehicleId = Guid.Empty };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenLaborIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var command = ValidCommand() with { LaborId = Guid.Empty };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.LaborId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenStartAtIsInThePast_ShouldHaveValidationError()
    {
        // Given
        var command = ValidCommand() with { StartAt = DateTimeOffset.UtcNow.AddDays(-1) };

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.StartAt);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenCommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var command = ValidCommand();

        // When
        var result = _validator.TestValidate(command);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}