using FluentValidation.TestHelper;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStatistics;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStatistics;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatisticsQueryValidatorTests
{
    private readonly GetWorkOrderStatisticsQueryValidator _validator;

    public GetWorkOrderStatisticsQueryValidatorTests()
    {
        _validator = new GetWorkOrderStatisticsQueryValidator();
    }

    [Fact]
    public void Validate_WhenDateIsDefault_ShouldHaveValidationError()
    {
        // Given
        var query = new GetWorkOrderStatisticsQuery(default);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.Date);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenDateIsProvided_ShouldNotHaveValidationErrorForDate()
    {
        // Given
        var query = new GetWorkOrderStatisticsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenDateIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var query = new GetWorkOrderStatisticsQuery(DateOnly.FromDateTime(DateTime.UtcNow));

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}