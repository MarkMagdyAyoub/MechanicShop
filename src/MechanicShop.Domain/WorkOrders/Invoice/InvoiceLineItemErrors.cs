using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceLineItemErrors
{
    public static Error InvoiceIdRequired => Error.Validation(
        code: "InvoiceLineItem.InvoiceId.Required",
        description: "InvoiceId Is Required");

    public static Error LineNumberInvalid => Error.Validation(
        code: "InvoiceLineItem.LineNumber.Invalid",
        description: "Line Number Must Be Greater Than 0");

    public static Error DescriptionRequired => Error.Validation(
        code: "InvoiceLineItem.Description.Required",
        description: "Description Is Required");

    public static Error QuantityInvalid => Error.Validation(
        code: "InvoiceLineItem.Quantity.Invalid",
        description: "Quantity Must Be Greater Than 0");

    public static Error UnitPriceInvalid => Error.Validation(
        code: "InvoiceLineItem.UnitPrice.Invalid",
        description: "Unit Price Must Be Greater Than 0");
}