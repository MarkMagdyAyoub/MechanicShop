using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandValidatorTests
{
  private readonly IssueInvoiceCommandValidator _validator;

  public IssueInvoiceCommandValidatorTests()
  {
    _validator = new IssueInvoiceCommandValidator();
  }

  [Fact]
  public void Constructor_WhenValidCommand_ShouldNotReturnErrors()
  {
    // Given
    var command = new IssueInvoiceCommand(Guid.NewGuid());

    // When
    var result = _validator.Validate(command);

    // Then
    Assert.True(result.IsValid);
  }


  [Fact]
  public void Constructor_WhenWorkOrderIdIsEmpty_ShouldReturnError()
  {
    // Given
    var command = new IssueInvoiceCommand(Guid.Empty);

    // When
    var result = _validator.Validate(command);

    // Then
    Assert.False(result.IsValid);
  }
}
