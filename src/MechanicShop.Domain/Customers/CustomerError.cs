using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
  public static Error NameRequired => Error.Validation(
    code: "Customer_Name_Required",
    description: "Customer Name Is Required"
  );

  public static Error PhoneNumberRequired => Error.Validation(
    code: "Customer_Number_Required", 
    description: "Phone number is required"
  );

  public static Error EmailRequired => Error.Validation(
    code: "Customer_Email_Required", 
    description: "Email is required"
  );

  public static Error EmailInvalid => Error.Validation(
    code: "Customer_Email_Invalid", 
    description: "Email is invalid"
  );

  public static Error CustomerExists => Error.Conflict(
      code: "Customer_Email_Exists", 
      description: "A customer with this email already exists."
  );

  public static Error InvalidPhoneNumber = Error.Conflict(
    code: "Customer_InvalidPhoneNumber", 
    description: "Phone number must be 7–15 digits and may start with '+'."
  );

  public static Error CannotDeleteCustomerWithWorkOrders = Error.Conflict(
    code: "Customer_CannotDelete", 
    description: "Customer Cannot Be Deleted Due To Existing Work Orders."
  );
}