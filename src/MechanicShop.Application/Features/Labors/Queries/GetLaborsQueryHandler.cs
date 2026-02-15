using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.Identity;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.Labor.Queries;
namespace MechanicShop.Application.Features.Labors.Queries;

public class GetLaborsQueryHandler(
  IAppDbContext context,
  ILogger< GetLaborsQueryHandler> logger
)
: IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetLaborsQueryHandler> _logger = logger;

  public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
  { 
    return await 
      _context.Employees
      .AsNoTracking()
      .Where(e => e.Role == Role.Labor)
      .Select(e => e.ToDto())
      .ToListAsync(cancellationToken);
  }
}