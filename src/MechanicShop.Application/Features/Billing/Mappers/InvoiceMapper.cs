
using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Application.Features.Billing.Enums;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
  public static InvoiceDto ToDto(this Invoice invoice)
  {
    ArgumentNullException.ThrowIfNull(invoice);

    return new InvoiceDto
    {
      InvoiceId = invoice.Id,
      WorkOrderId = invoice.WorkOrderId,
      Customer = invoice.WorkOrder!.Vehicle!.Customer!.ToDto(),
      IssuedAt = invoice.IssuedAtUtc,
      SubTotal = invoice.SubTotal,
      Tax = invoice.TaxAmount,
      Discount = invoice.DiscountAmount,
      Total = invoice.Total,
      PaymentStatus = PaymentStatus.Unpaid.ToString(),
      Items = invoice.LineItems.Select(li => li.ToDto()).ToList()
    };
  }
}