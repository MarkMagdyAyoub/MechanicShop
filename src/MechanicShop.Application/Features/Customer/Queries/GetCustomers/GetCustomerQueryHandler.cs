using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.DTOs;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler(
  IAppDbContext context,
  ILogger<GetCustomersQueryHandler> logger
) : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetCustomersQueryHandler> _logger = logger;

  public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Get All Customers With Their Vehicles");
    return await _context.Customers.AsNoTracking().Include(c => c.Vehicles).Select(c => c.ToDto()).ToListAsync(cancellationToken);
  }
}