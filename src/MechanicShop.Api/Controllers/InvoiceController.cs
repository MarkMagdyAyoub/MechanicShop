// <copyright file="InvoiceController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.Features.Billing.DTOs;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiVersion("1.0")]
[Route("api/invoices")]
public sealed class InvoiceController(ISender sender) : ApiController
{
  [HttpPost("workorders/{workOrderId:guid}")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("IssueInvoice")]
  [EndpointSummary("Issue An Invoice For A Work Order")]
  [EndpointDescription("Creates A New Invoice For Specified Work Order , Only Managers Role Are Authorized.")]
  [Tags("invoices")]
  public async Task<IActionResult> IssueInvoice(Guid workOrderId, CancellationToken ct)
  {
    var result = await sender.Send(new IssueInvoiceCommand(workOrderId), ct);

    return result.Match(
      response =>
        this.CreatedAtRoute(
          routeName: "GetInvoiceById",
          routeValues: new
          {
            invoiceId = response.InvoiceId,
            version = this.HttpContext.GetRequestedApiVersion()?.ToString(),
          },
          value: response),
      this.ProblemDetailsHandler);
  }

  [HttpPut("{invoiceId:guid}/payment")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("SettleInvoice")]
  [EndpointSummary("Mark Invoice As Paid.")]
  [EndpointDescription("Settles The Specified Invoice. Only Managers Role Are Authorized.")]
  [Tags("invoices")]
  public async Task<IActionResult> SettleInvoice(Guid invoiceId, CancellationToken ct)
  {
    var result = await sender.Send(new SettleInvoiceCommand(invoiceId), ct);

    return result.Match(
      _ => this.NoContent(),
      this.ProblemDetailsHandler);
  }

  [HttpGet("{invoiceId:guid}")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetInvoiceById")]
  [EndpointSummary("Get An Invoice By ID.")]
  [EndpointDescription("Get Detailed Information About Specific Invoice. Only Managers Role Are Authorized.")]
  [Tags("invoices")]
  public async Task<IActionResult> GetInvoice(Guid invoiceId, CancellationToken ct)
  {
    var result = await sender.Send(new GetInvoiceByIdQuery(invoiceId), ct);

    return result.Match(
      this.Ok,
      this.ProblemDetailsHandler);
  }

  [HttpGet("{invoiceId:guid}/pdf")]
  [Authorize(Policy = "ManagerOnly")]
  [ProducesResponseType(typeof(InvoicePdfDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
  [EndpointName("GetInvoicePdf")]
  [EndpointSummary("Get An Invoice Pdf File.")]
  [EndpointDescription("Gets An Invoice PDF File For Specific Invoice Id. Only Managers Role Are Authorized.")]
  [Tags("invoices")]
  public async Task<IActionResult> GetInvoicePdf(Guid invoiceId, CancellationToken ct)
  {
    var result = await sender.Send(new GetInvoicePdfQuery(invoiceId), ct);

    return result.Match(
      response => this.File(response.Content, response.ContentType, response.FileName),
      this.ProblemDetailsHandler);
  }
}
