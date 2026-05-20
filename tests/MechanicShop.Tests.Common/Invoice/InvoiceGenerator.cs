using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.InvoiceGenerator;

public static class InvoiceFactory
{
  public static Result<Invoice> Create(
    Guid? id = null,
    Guid? workOrderId = null,
    List<InvoiceLineItem>? items = null,
    decimal? discountAmount = null,
    decimal? taxAmount = null,
    DateTimeOffset? datetime = null
  )
  {
    return Invoice.Create(
      id ?? Guid.NewGuid(),
      workOrderId ?? Guid.NewGuid(),
      items ?? [InvoiceLineItemFactory.Create().Value],
      discountAmount ?? 0,
      taxAmount: 10,
      datetime ?? DateTimeOffset.UtcNow
    );
  }
}