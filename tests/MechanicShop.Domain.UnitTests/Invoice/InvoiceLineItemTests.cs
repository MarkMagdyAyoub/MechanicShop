using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.InvoiceGenerator;

namespace MechanicShop.Domain.UnitTests.Invoice;

public class InvoiceLineItemTests
{
  [Fact]
  public void Create_ValidData_ReturnInvoiceLineItemInstance()
  {
    var result = InvoiceLineItemFactory.Create();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_InvoiceIdIsEmptyGuid_ReturnInvoiceIdRequiredError()
  {
    var result = InvoiceLineItemFactory.Create(invoiceId: Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_LineNumberIsInvalid_ReturnLineNumberInvalidError()
  {
    var result = InvoiceLineItemFactory.Create(lineNumber: -1);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Code , result.TopError.Code);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_DescriptionIsEmptyOrWhiteSpace_ReturnDescriptionRequiredError(string description)
  {
    var result = InvoiceLineItemFactory.Create(description: description);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_QuantityIsInvalid_ReturnQuantityInvalidError()
  {
    var result = InvoiceLineItemFactory.Create(quantity: 0);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_UnitPriceIsInvalid_ReturnUnitPriceInvalidError()
  {
    var result = InvoiceLineItemFactory.Create(unitPrice: 0);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Code , result.TopError.Code);
  }
}