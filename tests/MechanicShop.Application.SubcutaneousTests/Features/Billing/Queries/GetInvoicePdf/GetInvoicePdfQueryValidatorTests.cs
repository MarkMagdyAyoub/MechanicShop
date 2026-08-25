using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoicePdf;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetInvoicePdfQueryHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
  private readonly IAppDbContext _context = factory.CreateDbContext();
  private readonly ISender _sender = factory.CreateSender();

  [Fact]
  public async Task Handle_WhenValidQuery_ReturnsInvoicePdf()
  {
    var workOrder = await WorkOrderFactory.GetRandomCompletedWorkOrderAsync(_context);

    var command = new IssueInvoiceCommand(workOrder.Id);

    var result = await _sender.Send(command);

    var query = new GetInvoicePdfQuery(result.Value.InvoiceId);

    var queryResult = await _sender.Send(query);
    
    Assert.True(queryResult.IsSuccess);
    Assert.NotNull(queryResult.Value.Content);
  }

  [Fact]
  public async Task Handle_WhenInvoiceNotFound_ReturnsInvoiceNotFoundError()
  {
    var query = new GetInvoicePdfQuery(Guid.NewGuid());

    var result = await _sender.Send(query);

    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
  }

  public Task InitializeAsync() => factory.ResetDatabaseAsync();

  public Task DisposeAsync() => factory.ResetDatabaseAsync();
}