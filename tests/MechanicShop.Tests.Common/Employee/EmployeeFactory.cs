using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.EmployeeGenerator;

public static class EmployeeFactory
{
  public static Result<Employee> Create(
    Guid? id = null,
    string? firstName = null, 
    string? lastName = null, 
    string? fullName = null, 
    Role? role = null
  )
  {
    return Employee.Create(
      id ?? Guid.NewGuid(),
      firstName ?? "John",
      lastName ?? "Doe",
      fullName ?? "John Doe",
      role ?? Role.Labor
    );
  }
}
