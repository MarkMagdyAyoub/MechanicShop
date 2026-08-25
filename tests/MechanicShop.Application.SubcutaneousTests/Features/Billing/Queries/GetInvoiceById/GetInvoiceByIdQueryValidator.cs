using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;


[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetInvoiceByIdQueryValidatorTests
{
  private readonly GetInvoiceByIdQueryValidator _validator;

  public GetInvoiceByIdQueryValidatorTests()
  {
    _validator = new GetInvoiceByIdQueryValidator();
  }

  [Fact]
  public void Constructor_WhenValidCommand_ShouldNotReturnError()
  {
    // Given
    var command = new GetInvoiceByIdQuery(Guid.NewGuid());
  
    // When
    var result = _validator.Validate(command);
  
    // Then
    Assert.True(result.IsValid);
  }

  [Fact]
  public void Constructor_WhenInvoiceIdIsEmpty_ShouldReturnError()
  {
    // Given
    var command = new GetInvoiceByIdQuery(Guid.Empty);
  
    // When
    var result = _validator.Validate(command);
  
    // Then
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetInvoiceByIdQuery.InvoiceId));
  }
}