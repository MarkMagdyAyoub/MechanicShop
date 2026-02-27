using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed record IssueInvoiceCommand(
  Guid WorkOrderId
) : IRequest<Result<InvoiceDto>>;