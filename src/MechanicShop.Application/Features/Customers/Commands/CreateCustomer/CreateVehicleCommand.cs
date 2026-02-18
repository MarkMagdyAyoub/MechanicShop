using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateVehicleCommand(
  string Make , 
  string Model , 
  string LicensePlate , 
  int Year
) : IRequest<Result<VehicleDto>>;