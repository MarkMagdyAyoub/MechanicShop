using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Common.ValueObjects.EmailAddress;
using MechanicShop.Domain.Common.ValueObjects.PhoneNumber;
using MechanicShop.Domain.Customers.Vehicles;

namespace MechanicShop.Domain.Customers;

public sealed class Customer : AuditableEntity
{
  public string? Name { get; private set; }
  public PhoneNumber? PhoneNumber { get; private set; }
  public EmailAddress? Email { get; private set; }
  private readonly List<Vehicle> _vehicles = [];
  public IEnumerable<Vehicle> Vehicles => _vehicles.AsReadOnly();

#pragma warning disable CS8618
  private Customer(){}
#pragma warning restore CS8618

  private Customer(Guid id , string name , PhoneNumber? phoneNumber , EmailAddress? email , List<Vehicle> vehicles)
    : base(id)
  {
    Name = name;
    PhoneNumber = phoneNumber;
    Email = email;
    _vehicles = vehicles;
  }

  public static Result<Customer> Create(Guid id , string name , string? phoneNumber , string? email , List<Vehicle> vehicles)
  {
    if(string.IsNullOrWhiteSpace(name))
      return CustomerErrors.NameRequired;

    EmailAddress? emailAddress = null;
    PhoneNumber? phone = null;

    if(!string.IsNullOrWhiteSpace(email))
    {
      var emailResult = EmailAddress.Create(email);

      if(emailResult.IsError)
        return emailResult.Errors;

      emailAddress = emailResult.Value;
    }

    if(!string.IsNullOrWhiteSpace(phoneNumber))
    {
      var phoneResult = PhoneNumber.Create(phoneNumber);

      if(phoneResult.IsError)
        return phoneResult.Errors;

      phone = phoneResult.Value;
    }
    
    return new Customer(id , name,  phone , emailAddress , vehicles);
  }

  public Result<Updated> Update(string name, string? email, string? phoneNumber)
  {
    if (string.IsNullOrWhiteSpace(name))
        return CustomerErrors.NameRequired;

    EmailAddress? emailAddress = null;
    PhoneNumber? phone = null;

    if(!string.IsNullOrWhiteSpace(email))
    {
      var emailResult = EmailAddress.Create(email);

      if(emailResult.IsError)
        return emailResult.Errors;

      emailAddress = emailResult.Value;
    }

    if(!string.IsNullOrWhiteSpace(phoneNumber))
    {
      var phoneResult = PhoneNumber.Create(phoneNumber);

      if(phoneResult.IsError)
        return phoneResult.Errors;

      phone = phoneResult.Value;
    }

    Name = name;
    Email = emailAddress;
    PhoneNumber = phone;

    return Result.Updated;
  }

  public Result<Updated> UpsertParts(List<Vehicle> incomingVehicles)
  {
    // Remove vehicles that no longer exist in incomingVehicles
    _vehicles.RemoveAll(existing => incomingVehicles.All(v => v.Id != existing.Id));

    foreach(var incoming in incomingVehicles)
    {
      var existing = _vehicles.FirstOrDefault(v => v.Id == incoming.Id);
      if(existing is null)
      {
        _vehicles.Add(incoming);
      }
      else
      {
        var updatedVehicleResult = existing.Update(incoming.Make , incoming.Model , incoming.Year , incoming.LicensePlate);
        if(updatedVehicleResult.IsError)
          return updatedVehicleResult.Errors;
      }
    }
    return Result.Updated;
  }
}
