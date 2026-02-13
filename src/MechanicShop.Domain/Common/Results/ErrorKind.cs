namespace MechanicShop.Domain.Common.Results;
public enum ErrorKind
{
    // Generic failure not covered by other categories
    Failure,

    // Unexpected or unhandled error
    Unexpected,

    // Business-rule or input validation error
    Validation,

    // Conflict with the current state
    Conflict,

    // Resource was not found
    NotFound,

    // User is not authenticated
    Unauthorized,

    // User is authenticated but not allowed to access the resource
    Forbidden
}
