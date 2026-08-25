using FluentValidation.TestHelper;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Identity.Queries.GetUserInfoById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetUserInfoByIdQueryValidatorTests
{
    private readonly GetUserInfoByIdQueryValidator _validator;

    public GetUserInfoByIdQueryValidatorTests()
    {
        _validator = new GetUserInfoByIdQueryValidator();
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var query = new GetUserInfoByIdQuery(Guid.Empty);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUserIdIsProvided_ShouldNotHaveValidationErrorForUserId()
    {
        // Given
        var query = new GetUserInfoByIdQuery(Guid.NewGuid());

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var query = new GetUserInfoByIdQuery(Guid.NewGuid());

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}