using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsInvoiceDto()
    {
        var workOrder = await WorkOrderFactory.GetRandomCompletedWorkOrderAsync(_context);

        var command = new IssueInvoiceCommand(workOrder.Id);

        var result = await _sender.Send(command);
        
        Assert.True(result.IsSuccess);

        Assert.Equal(workOrder.Id, result.Value.WorkOrderId);
    }

    [Fact]
    public async Task Handle_WhenValidCommand_InvoiceIsPersistedToDatabase()
    {
        var workOrder = await WorkOrderFactory.GetRandomCompletedWorkOrderAsync(_context);

        var command = new IssueInvoiceCommand(workOrder.Id);

        var result = await _sender.Send(command);

        Assert.True(result.IsSuccess);

        var saved = await _context.Invoices
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.WorkOrderId == workOrder.Id);

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ReturnsWorkOrderNotFoundError()
    {
      var command = new IssueInvoiceCommand(Guid.NewGuid());

      var result = await _sender.Send(command);

      Assert.False(result.IsSuccess);
      Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderIsNotCompleted_ReturnsWorkOrderMustBeCompletedForInvoicingError()
    {
        var notCompleted = await WorkOrderFactory.GetRandomWorkOrderAsync(_context);

        var command = new IssueInvoiceCommand(notCompleted.Id);

        var result = await _sender.Send(command);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderMustBeCompletedForInvoicing.Code, result.TopError.Code);
    }

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => factory.ResetDatabaseAsync();
}
