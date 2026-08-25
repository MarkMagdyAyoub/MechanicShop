using MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.SettleInvoice;

public class SettleInvoiceCommandValidatorTests
{
    private readonly SettleInvoiceCommandValidator _validator;

    public SettleInvoiceCommandValidatorTests()
    {
        _validator = new SettleInvoiceCommandValidator();
    }

    [Fact]
    public void Constructor_WhenInvoiceIdIsEmpty_ThrowsArgumentException()
    {
        var command = new SettleInvoiceCommand(InvoiceId: Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Constructor_WhenValidCommand_ReturnsValidResult()
    {
        // Arrange
        var command = new SettleInvoiceCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}