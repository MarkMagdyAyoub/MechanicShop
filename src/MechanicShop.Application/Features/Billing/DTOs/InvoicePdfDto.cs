namespace MechanicShop.Application.Features.Billing.DTOs;

public sealed class InvoicePdfDto
{
  public string FileName { get; init; } = null!;
  public string ContentType { get; init; } = "application/pdf";
  public byte[] Content { get; init; } = null!;
}