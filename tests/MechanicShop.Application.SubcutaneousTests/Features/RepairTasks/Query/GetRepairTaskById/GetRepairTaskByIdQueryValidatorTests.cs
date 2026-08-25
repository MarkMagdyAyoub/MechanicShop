using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _validator;

    public GetRepairTaskByIdQueryValidatorTests()
    {
        _validator = new GetRepairTaskByIdQueryValidator();
    }

    [Fact]
    public void Constructor_WhenQueryIsValid_ShouldNotReturnError()
    {
        // Given
        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        // When
        var result = _validator.Validate(query);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenTaskIdIsEmpty_ReturnError()
    {
        // Given
        var query = new GetRepairTaskByIdQuery(Guid.Empty);

        // When
        var result = _validator.Validate(query);

        // Then
        Assert.False(result.IsValid);
    }
}
