using FluentValidation.TestHelper;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrderById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetWorkOrderByIdQueryValidatorTests
{
    private readonly GetWorkOrderByIdQueryValidator _validator;

    public GetWorkOrderByIdQueryValidatorTests()
    {
        _validator = new GetWorkOrderByIdQueryValidator();
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsEmpty_ShouldHaveValidationError()
    {
        // Given
        var query = new GetWorkOrderByIdQuery(Guid.Empty);

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenWorkOrderIdIsProvided_ShouldNotHaveAnyValidationErrors()
    {
        // Given
        var query = new GetWorkOrderByIdQuery(Guid.NewGuid());

        // When
        var result = _validator.TestValidate(query);

        // Then
        result.ShouldNotHaveAnyValidationErrors();
        Assert.True(result.IsValid);
    }
}