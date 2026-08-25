using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQueryValidatorTests
{
  private readonly GetInvoicePdfQueryValidator _validator;

  public GetInvoicePdfQueryValidatorTests()
  {
    _validator = new GetInvoicePdfQueryValidator();
  }

  [Fact]
  public void Constructor_WhenValidQuery_ShouldNotReturnErrors()
  {
    // Given
    var query = new GetInvoicePdfQuery(Guid.NewGuid());

    // When
    var result = _validator.Validate(query);

    // Then
    Assert.True(result.IsValid);
  }

  [Fact]
  public void Constructor_WhenInvoiceIdIsEmpty_ShouldReturnError()
  {
    // Given
    var query = new GetInvoicePdfQuery(Guid.Empty);

    // When
    var result = _validator.Validate(query);

    // Then
    Assert.False(result.IsValid);
  }
}