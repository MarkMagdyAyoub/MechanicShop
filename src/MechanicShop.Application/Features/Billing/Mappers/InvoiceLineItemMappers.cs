using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Domain.WorkOrders.Billing;
namespace MechanicShop.Application.Features.Billing.Mappers;
public static class InvoiceLineItemMapper
{
  public static InvoiceLineItemDto ToDto(this InvoiceLineItem invoiceLineItem)
  {
    ArgumentNullException.ThrowIfNull(invoiceLineItem);

    return new InvoiceLineItemDto
    {
      InvoiceId = invoiceLineItem.InvoiceId,
      LineNumber = invoiceLineItem.LineNumber,
      Description = invoiceLineItem.Description,
      Quantity = invoiceLineItem.Quantity,
      UnitPrice = invoiceLineItem.UnitPrice,
      LineTotal = invoiceLineItem.LineTotal
    };
  }
}