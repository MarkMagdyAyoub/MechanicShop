using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;


public sealed record UpdateVehicleCommand(
  Guid? VehicleId,
  string Make , 
  string Model , 
  string LicensePlate , 
  int Year
) : IRequest<Result<Updated>>;