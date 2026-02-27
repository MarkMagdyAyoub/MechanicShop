using MechanicShop.Application.Features.Customers.DTOs;

namespace MechanicShop.Application.Features.Billing.DTOs;

public class InvoiceDto
{
  public Guid InvoiceId { get; set; }
  public Guid WorkOrderId { get; set; }
  public DateTimeOffset IssuedAt { get; set; }
  public string PaymentStatus { get; set; } = null!;
  public CustomerDto? Customer { get; set; }
  public VehicleDto? Vehicle { get; set; }
  public decimal SubTotal { get; set; }
  public decimal Discount { get; set; }
  public decimal Tax { get; set; }
  public decimal Total { get; set; }
  public List<InvoiceLineItemDto> Items { get; set; } = [];
}