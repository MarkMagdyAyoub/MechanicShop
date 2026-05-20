using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Tests.Common.InvoiceGenerator;

public static class InvoiceLineItemFactory
{
  public static Result<InvoiceLineItem> Create(
    Guid? invoiceId = null,
    int? lineNumber = null,
    string? description = null,
    int? quantity = null,
    decimal? unitPrice = null
  )
  {
    return InvoiceLineItem.Create(
      invoiceId ?? Guid.NewGuid(),
      lineNumber ?? 1,
      description ?? "Oil Change",
      quantity ?? 1,
      unitPrice ?? 50
    );
  }
}