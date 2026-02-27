using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
  IAppDbContext context,
  ILogger<GetInvoicePdfQueryHandler> logger,
  IPdfGenerator pdfGenerator
) : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetInvoicePdfQueryHandler> _logger = logger;
  private readonly IPdfGenerator _pdfGenerator = pdfGenerator;

  public async Task<Result<InvoicePdfDto>> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
  {
    var invoice = await _context.Invoices.Include(i => i.LineItems).FirstOrDefaultAsync(i => i.Id == request.InvoiceId , cancellationToken);

    if(invoice is null)
    {
      _logger.LogWarning("Invoice With Id `{invoiceId}` Is Not Found" , request.InvoiceId);
      
      return ApplicationErrors.InvoiceNotFound;
    }
    try
    {
      var pdf = _pdfGenerator.Generate(invoice);

      return new InvoicePdfDto
      {
        FileName = $"invoice-{invoice.Id}.pdf",
        Content = pdf
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex , "Failed To Generate Pdf For Invoice With Id `{invoiceId}`" , request.InvoiceId);

      return ApplicationErrors.InvoicePdfCannotBeGenerated;
    }
  }
}