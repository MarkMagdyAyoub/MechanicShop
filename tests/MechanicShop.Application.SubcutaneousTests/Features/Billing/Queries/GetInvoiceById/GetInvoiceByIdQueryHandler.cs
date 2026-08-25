using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Queries.GetInvoiceById;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class GetInvoiceByIdQueryHandlerTests(SubcutaneousTestAppFactory factory , ITestOutputHelper output) : IAsyncLifetime
{
  private readonly IAppDbContext _context = factory.CreateDbContext();
  private readonly ISender _sender = factory.CreateSender();

  [Fact]
  public async Task Handle_WhenValidQuery_ReturnsInvoiceDto()
  {
    var workOrder = await WorkOrderFactory.GetRandomCompletedWorkOrderAsync(_context);

    var command = new IssueInvoiceCommand(workOrder.Id);

    var result = await _sender.Send(command);

    var query = new GetInvoiceByIdQuery(result.Value.InvoiceId);

    var queryResult = await _sender.Send(query);
    
    Assert.True(queryResult.IsSuccess);
    Assert.Equal(result.Value.WorkOrderId, queryResult.Value.WorkOrderId);
  }

  [Fact]
  public async Task Handle_WhenInvoiceNotFound_ReturnsInvoiceNotFoundError()
  {
    var query = new GetInvoiceByIdQuery(Guid.NewGuid());

    var result = await _sender.Send(query);

    Assert.False(result.IsSuccess);
    Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
  }

  public Task InitializeAsync() => factory.ResetDatabaseAsync();

  public Task DisposeAsync() => factory.ResetDatabaseAsync();
}