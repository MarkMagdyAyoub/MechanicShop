using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Domain.Employees;

public sealed class Employee : AuditableEntity
{
  public string FirstName { get; }
  public string LastName { get; }
  public string FullName { get; }
  public Role Role { get; }

#pragma warning disable CS8618
  private Employee(){}
#pragma warning restore CS8618

  private Employee(Guid id , string firstName , string lastName , string fullName , Role role) 
    : base (id)
  {
    FirstName = firstName;
    LastName = lastName;
    FullName = fullName;
    Role = role;
  }

  public static Result<Employee> Create(Guid id , string firstName , string lastName , string fullName , Role role)
  {
    if(id == Guid.Empty) 
      return EmployeeErrors.IdRequired;
    
    if(string.IsNullOrWhiteSpace(firstName))
      return EmployeeErrors.FirstNameRequired;

    if(string.IsNullOrWhiteSpace(lastName))
      return EmployeeErrors.LastNameRequired;
    
    if(!Enum.IsDefined(role))
      return EmployeeErrors.RoleInvalid;

    return new Employee(id , firstName , lastName , fullName , role);
  }

}