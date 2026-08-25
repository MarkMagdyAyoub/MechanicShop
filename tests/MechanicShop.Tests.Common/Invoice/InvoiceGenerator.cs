using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.WorkOrderGenerator;

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

  private static async Task<Invoice> CreateUnpaidInvoiceAsync(IAppDbContext _context)
  {
      var workOrder = await WorkOrderFactory.GetRandomWorkOrderAsync(_context);

      var invoice = Create(workOrderId: workOrder.Id).Value;

      _context.Invoices.Add(invoice);
      await _context.SaveChangesAsync(CancellationToken.None);

      return invoice;
  }
}