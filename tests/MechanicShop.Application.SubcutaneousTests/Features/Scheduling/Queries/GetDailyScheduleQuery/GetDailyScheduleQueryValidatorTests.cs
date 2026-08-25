using FluentValidation.TestHelper;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Scheduling.Queries.GetDailySchedule;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetDailyScheduleQueryValidatorTests
{
    private readonly GetDailyScheduleQueryValidator _validator;

    public GetDailyScheduleQueryValidatorTests()
    {
        _validator = new GetDailyScheduleQueryValidator();
    }

    [Fact]
    public void Validate_WhenTimeZoneIsNull_ShouldHaveValidationError()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            null!,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        // When
        var result = _validator.TestValidate(query);

        // Then
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenTimeZoneIsValid_ShouldNotHaveValidationErrorForTimeZone()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.TimeZone);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenScheduleDateIsDefault_ShouldHaveValidationError()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            default,
            null);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.ScheduleDate);
    }

    [Fact]
    public void Validate_WhenScheduleDateIsValid_ShouldNotHaveValidationErrorForScheduleDate()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.ScheduleDate);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenLaborIdIsNull_ShouldNotHaveValidationErrorForLaborId()
    {
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.LaborId);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenLaborIdIsProvided_ShouldNotHaveValidationErrorForLaborId()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid());

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.LaborId);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var query = new GetDailyScheduleQuery(
            TimeZoneInfo.FindSystemTimeZoneById("UTC"),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid());

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}