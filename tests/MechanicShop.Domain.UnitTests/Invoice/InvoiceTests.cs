using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.InvoiceGenerator;

namespace MechanicShop.Domain.UnitTests.Invoice;

public class InvoiceTests
{
  [Fact]
  public void Create_ValidData_ReturnApplyDiscount()
  {
    var result = InvoiceFactory.Create();

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void Create_WorkOrderIdIsEmptyGuid_ReturnWorkOrderIdInvalidError()
  {
    var result = InvoiceFactory.Create(workOrderId: Guid.Empty);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.WorkOrderIdInvalid.Code , result.TopError.Code);
  }

  [Fact]
  public void Create_ItemsIsEmpty_ReturnLineItemsEmptyError()
  {
    var result = InvoiceFactory.Create(items: []);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.LineItemsEmpty.Code , result.TopError.Code);
  }

  [Fact]
  public void ApplyDiscount_ValidDiscount_ReturnsUpdated()
  {
    var invoice = InvoiceFactory.Create().Value;

    var result = invoice.ApplyDiscount(5m);

    Assert.True(result.IsSuccess);
    Assert.Equal(5m, invoice.DiscountAmount);
  }

  [Fact]
  public void ApplyDiscount_NegativeAmount_ReturnsDiscountNegativeError()
  {
    var invoice = InvoiceFactory.Create().Value;

    var result = invoice.ApplyDiscount(-1m);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.DiscountNegative.Code, result.TopError.Code);
  }

  [Fact]
  public void ApplyDiscount_AmountExceedsSubtotal_ReturnsDiscountExceedsSubtotalError()
  {
    var invoice = InvoiceFactory.Create().Value;
    var tooMuch = invoice.SubTotal + 1m;

    var result = invoice.ApplyDiscount(tooMuch);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.DiscountExceedsSubtotal.Code, result.TopError.Code);
  }

  [Fact]
  public void ApplyDiscount_InvoiceAlreadyPaid_ReturnsInvoiceLockedError()
  {
    var invoice = InvoiceFactory.Create().Value;
    invoice.MarkAsPaid(DateTimeOffset.UtcNow);

    var result = invoice.ApplyDiscount(5m);

    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
  }

  [Fact]
  public void MarkAsPaid_InvoiceAlreadyPaid_ReturnInvoiceLockedError()
  {
    var invoice = InvoiceFactory.Create().Value;

    invoice.MarkAsPaid(DateTimeOffset.UtcNow);

    var result = invoice.MarkAsPaid(DateTimeOffset.UtcNow);


    Assert.False(result.IsSuccess);
    Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
  }
}