using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandHandler(
    IAppDbContext context,
    ILogger<SettleInvoiceCommandHandler> logger,
    HybridCache cache,
    TimeProvider timeProvider

) : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<SettleInvoiceCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;
  private readonly TimeProvider _timeProvider = timeProvider;

  public async Task<Result<Success>> Handle(SettleInvoiceCommand request, CancellationToken cancellationToken)
  {
    var invoice = await _context.Invoices.FindAsync(request.InvoiceId , cancellationToken);

    if (invoice is null)
    {
        _logger.LogWarning("Invoice {InvoiceId} Not Found.", request.InvoiceId);
        return ApplicationErrors.InvoiceNotFound;
    }

    var payInvoiceResult = invoice.MarkAsPaid(_timeProvider.GetUtcNow());

    if (payInvoiceResult.IsError)
    {
      _logger.LogWarning(
          "Invoice Payment Failed For InvoiceId: {InvoiceId}. Errors: {Errors}",
          invoice.Id,
          payInvoiceResult.Errors);

      return payInvoiceResult.Errors;
    }

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("invoice" , cancellationToken);

    _logger.LogInformation("Invoice With Id `{invoiceId} Paid Successfully`" , request.InvoiceId);

    return Result.Success;
  }
}