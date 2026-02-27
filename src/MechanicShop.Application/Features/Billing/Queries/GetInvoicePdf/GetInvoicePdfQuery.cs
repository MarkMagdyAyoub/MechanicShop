using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed record GetInvoicePdfQuery(
  Guid InvoiceId
) : ICachedQuery<Result<InvoicePdfDto>>
{
  public string CacheKey => $"pdf_{InvoiceId}";

  public string[] Tags => ["invoice"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}