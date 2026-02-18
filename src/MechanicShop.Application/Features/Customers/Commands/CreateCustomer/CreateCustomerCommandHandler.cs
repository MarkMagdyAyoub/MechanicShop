using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler (
    IAppDbContext context , 
    ILogger<CreateCustomerCommandHandler> logger , 
    HybridCache cache
  ) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
  {
    var email = command.Email.Trim().ToLower();

    var exists = await _context.Customers.AnyAsync(c => c.Email == email , ct);

    if (exists)
    {
      _logger.LogWarning("Customer Creation Failed, Email Already Exists");
      return CustomerErrors.CustomerExists;
    }  

    List<Vehicle> vehicles = [];

    var licensePlates = command.Vehicles
                                .Select(v => v.LicensePlate)
                                .ToList();

    var existingPlates = await _context.Vehicles
                              .Where(v => licensePlates.Contains(v.LicensePlate))
                              .Select(v => v.LicensePlate)
                              .ToListAsync();

    if (existingPlates.Any())
    {
        _logger.LogWarning($"Duplicate License Plates Already Exist: {string.Join(" , " , licensePlates)}");

        return VehicleErrors.UniqueLicensePlateRequired;
    }

    foreach(var vehicle in command.Vehicles)
    {
      var vehicleResult = Vehicle.Create(Guid.NewGuid() , vehicle.Make , vehicle.Model , vehicle.Year , vehicle.LicensePlate);
      
      if(vehicleResult.IsError)
        return vehicleResult.Errors;
      
      vehicles.Add(vehicleResult.Value);
    }

    var createCustomerResult = Customer.Create(
      Guid.NewGuid(),
      command.Name,
      command.PhoneNumber,
      command.Email,
      vehicles
    );

    if(createCustomerResult.IsError)
      return createCustomerResult.Errors;
    
    var customer = createCustomerResult.Value;

    _context.Customers.Add(customer);
    
    await _context.SaveChangeAsync(ct);

    _logger.LogInformation($"Customer Created Successfully. Id: {customer.Id}");

    await _cache.RemoveAsync("customer" , ct);

    return customer.ToDto();
  }
}