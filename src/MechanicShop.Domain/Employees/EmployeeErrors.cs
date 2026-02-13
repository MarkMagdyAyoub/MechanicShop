using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Employees;

public static class EmployeeErrors
{
  public static readonly Error IdRequired = 
    Error.Validation(code: "Employee.Id.Required" , description: "Employee Id Is Required");
  public static readonly Error FirstNameRequired = 
    Error.Validation(code: "Employee.FirstName.Required" , description: "Employee's FirstName Is Required");
  public static readonly Error LastNameRequired = 
    Error.Validation(code: "Employee.LastName.Required" , description: "Employee's LastName Is Required");
  public static readonly Error RoleInvalid = 
    Error.Validation(code: "Employee.Role.Invalid" , description: "Invalid Role Assigned To Employee");
}