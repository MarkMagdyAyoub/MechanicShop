namespace MechanicShop.Application.Features.Billing.DTOs;


public class InvoiceLineItemDto
{
  public Guid InvoiceId { get; set; }
  public int LineNumber { get; set; }
  public string Description { get; set; } = null!;
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public decimal LineTotal { get; set; }
}