using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.Common.Constants;
using MechanicShop.Application.Features.Billing.Mappers;
namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;


public sealed class IssueInvoiceCommandHandler(
  IAppDbContext context,
  ILogger<IssueInvoiceCommandHandler> logger,
  HybridCache cache,
  TimeProvider timeProvider
) : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<IssueInvoiceCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;
  private readonly TimeProvider _timeProvider = timeProvider;

  public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders
                          .Include(wo => wo.Vehicle)
                            .ThenInclude(v => v!.Customer)
                          .Include(wo => wo.RepairTasks)
                            .ThenInclude(rt => rt.Parts)
                          .FirstOrDefaultAsync(wo => wo.Id == request.WorkOrderId);
    
    if(workOrder is null)
    {
      _logger.LogWarning("WorkOrder With Id `{workOrderId} Is Not Found`" , request.WorkOrderId);
      
      return ApplicationErrors.WorkOrderNotFound;
    }

    if(workOrder.State != WorkOrderState.Completed)
    {
      _logger.LogWarning("Invoice Issuance Rejected. WorkOrder With Id {workOrderId} Is Not Completed" , request.WorkOrderId);

      return ApplicationErrors.WorkOrderMustBeCompletedForInvoicing;
    }

    Guid invoiceId = Guid.NewGuid();
    var lineItems = new List<InvoiceLineItem>();
    var lineNumber = 1;

    foreach(var (task , taskIndex) in workOrder.RepairTasks.Select((rt , i) => (rt , i + 1)))
    {
      var partsSummary = task.Parts.Any() ?
          string.Join(Environment.NewLine , task.Parts.Select(p => $"    • {p.Name} X {p.Quantity} = {p.Cost:C}")) :
          "    • No Parts";
      
      var lineDescription = 
        $"{lineNumber}: {task.Name}{Environment.NewLine}" +
        $"   Labor = {task.LaborCost:C}{Environment.NewLine}" +
        $"   Parts:{Environment.NewLine}" +
        $"{partsSummary}";

      var unitPrice = task.LaborCost + task.Parts.Sum(p => p.Cost * p.Quantity);

      var lineItemResult = InvoiceLineItem.Create(
        invoiceId: invoiceId,
        lineNumber: lineNumber++,
        description: lineDescription,
        quantity: task.Parts.Sum(p => p.Quantity),
        unitPrice: unitPrice
      );

      if (lineItemResult.IsError)
      {
        _logger.LogWarning("Failed To Create LineItem: {ErrorDescription}" , lineItemResult.TopError.Description);
        return lineItemResult.Errors;
      }

      lineItems.Add(lineItemResult.Value);
    }

    var subTotal = lineItems.Sum(li => li.LineTotal);
    var tax = subTotal * Constants.TaxRate;
    var discount = workOrder.Discount ?? 0m;
    
    var createInvoiceResult = Invoice.Create(
      invoiceId,
      workOrder.Id,
      lineItems,
      discount,
      tax,
      _timeProvider.GetUtcNow()
    );

    if (createInvoiceResult.IsError)
    {
      _logger.LogWarning(
                  "Invoice Creation Failed For WorkOrderId `{WorkOrderId}`. Errors: {Errors}",
                  request.WorkOrderId,
                  createInvoiceResult.Errors);
      return createInvoiceResult.Errors;
    }

    var invoice = createInvoiceResult.Value;

    await _context.Invoices.AddAsync(invoice , cancellationToken);

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("invoice" , cancellationToken);

    _logger.LogInformation("Invoice `{invoiceId}` Issued For WorkOrder {workOrderId}" , invoiceId , workOrder.Id);

    return invoice.ToDto();
  }
}