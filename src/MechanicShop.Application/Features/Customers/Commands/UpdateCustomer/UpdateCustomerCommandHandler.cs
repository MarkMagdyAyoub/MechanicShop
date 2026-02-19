using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler(
  IAppDbContext context,
  ILogger<UpdateCustomerCommandHandler> logger,
  HybridCache cache
)
  : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
  {
    var customer = await _context.Customers
                        .Include(c => c.Vehicles)
                        .FirstOrDefaultAsync(c => c.Id == request.CustomerId , cancellationToken);

    if(customer is null)
    {
      _logger.LogWarning("Customer With Id `{CustomerId}` Not Found For Update" , request.CustomerId);
      return ApplicationErrors.CustomerNotFound;
    }

    var vehicles = new List<Vehicle>();

    foreach (var vehicle in request.Vehicles)
    {
      var vehicleId = vehicle.VehicleId ?? Guid.NewGuid();
      var updateVehicleResult = Vehicle.Create(vehicleId , vehicle.Make , vehicle.Model , vehicle.Year , vehicle.LicensePlate);

      if (updateVehicleResult.IsError)
      {
        return updateVehicleResult.Errors;
      }

      vehicles.Add(updateVehicleResult.Value);
    }

    var updateCustomerResult = customer.Update(request.Name , request.Email , request.PhoneNumber);

    if (updateCustomerResult.IsError)
    {
      return updateCustomerResult.Errors;
    }

    var upsertPartsResult = customer.UpsertParts(vehicles);

    if (upsertPartsResult.IsError)
    {
      return upsertPartsResult.Errors;
    }

    await _context.SaveChangesAsync(cancellationToken);
    
    await _cache.RemoveByTagAsync("customer" ,  cancellationToken);
    
    _logger.LogInformation("Customer With Id `{CustomerId}` Updated Successfully" , request.CustomerId);
    
    return Result.Updated;
  }
}