using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing.Enums;
using MechanicShop.Tests.Common.InvoiceGenerator;
using MechanicShop.Tests.Common.WorkOrderGenerator;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

[Collection(SubcutaneousTestAppFactoryCollection.CollectionName)]
public class SettleInvoiceCommandHandlerTests(SubcutaneousTestAppFactory factory) : IAsyncLifetime
{
    private readonly IAppDbContext _context = factory.CreateDbContext();
    private readonly ISender _sender = factory.CreateSender();

    [Fact]
    public async Task Handle_WhenCommandIsValid_ShouldReturnSuccessResult()
    {
        // Given
        var invoice = await CreateUnpaidInvoiceAsync();

        // When
        var command = new SettleInvoiceCommand(invoice.Id);
        var result = await _sender.Send(command);

        // Then
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_InvoiceIsMarkedPaidInDatabase()
    {
        // Given
        var invoice = await CreateUnpaidInvoiceAsync();

        // When
        var command = new SettleInvoiceCommand(invoice.Id);
        await _sender.Send(command);

        // Then
        var saved = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        Assert.NotNull(saved);
        Assert.Equal(InvoiceStatus.Paid, saved.Status);
        Assert.NotNull(saved.PaidAt);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsInvoiceNotFoundError()
    {
        // Given
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        // When
        var result = await _sender.Send(command);

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WhenInvoiceAlreadyPaid_ReturnsInvoiceLockedError()
    {
        // Given
        var invoice = await CreateUnpaidInvoiceAsync();

        var firstCommand = new SettleInvoiceCommand(invoice.Id);
        await _sender.Send(firstCommand); // pay it once

        // When
        var secondCommand = new SettleInvoiceCommand(invoice.Id);
        var result = await _sender.Send(secondCommand); // try again

        // Then
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
    }

    private async Task<Invoice> CreateUnpaidInvoiceAsync()
    {
        var workOrder = await WorkOrderFactory.GetRandomWorkOrderAsync(_context);

        var invoice = InvoiceFactory.Create(workOrderId: workOrder.Id).Value;
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(CancellationToken.None);

        return invoice;
    }

    public Task DisposeAsync() => factory.ResetDatabaseAsync();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
}