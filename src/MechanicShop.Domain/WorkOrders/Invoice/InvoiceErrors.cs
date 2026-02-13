using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceErrors
{
    public static readonly Error WorkOrderIdInvalid = Error.Validation(
        code: "Invoice.WorkOrderId.Invalid",
        description: "WorkOrderId Is Invalid");

    public static readonly Error LineItemsEmpty = Error.Validation(
        code: "Invoice.LineItems.Empty",
        description: "Invoice Must Have Line Items");

    public static readonly Error InvoiceLocked = Error.Validation(
        code: "Invoice.Locked",
        description: "Invoice Is Locked");

    public static readonly Error DiscountNegative = Error.Validation(
        code: "Invoice.Discount.Negative",
        description: "Discount Cannot Be Negative");

    public static readonly Error DiscountExceedsSubtotal = Error.Validation(
        code: "Invoice.Discount.ExceedsSubtotal",
        description: "Discount Exceeds Subtotal");
}