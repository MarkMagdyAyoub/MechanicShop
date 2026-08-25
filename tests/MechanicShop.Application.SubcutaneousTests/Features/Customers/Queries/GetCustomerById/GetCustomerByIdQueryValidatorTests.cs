using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Tests.Common.CustomerGenerator;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidatorTests
{
    private readonly GetCustomerByIdQueryValidator _validator;

    public GetCustomerByIdQueryValidatorTests()
    {
        _validator = new GetCustomerByIdQueryValidator();
    }

    [Fact]
    public void Constructor_WhenCustomerIdIsEmpty_ReturnCustomerError()
    {
        var query = CustomerFactory.GetCustomerByIdQuery(Guid.Empty);

        var result = _validator.Validate(query);

        Assert.False(result.IsValid);
    }
}
