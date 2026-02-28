using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;

public sealed class RemoveCustomerCommandHandler(
  IAppDbContext context,
  ILogger<RemoveCustomerCommandHandler> logger,
  HybridCache cache
)
: IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Deleted>> Handle(RemoveCustomerCommand request, CancellationToken cancellationToken)
  {
    var customer = await _context.Customers.FindAsync([request.CustomerId] , cancellationToken);
    if(customer is null)
    {
      _logger.LogWarning("Customer With Id {CustomerId} Not Found For Deletion" , request.CustomerId);
      return ApplicationErrors.CustomerNotFound;
    }

    var hasWorkOrders = await _context.WorkOrders
                              .Include(wo => wo.Vehicle)
                              .Where(wo => wo.Vehicle != null)
                              .AnyAsync(wo => wo.Vehicle!.CustomerId == request.CustomerId , cancellationToken);

    if (hasWorkOrders)
    {
      _logger.LogWarning("Customer With Id `{customerId}` Cannot Be Deleted Because They Have Associated Work Orders" , request.CustomerId);
      
      return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
    }

    _context.Customers.Remove(customer);

    await _context.SaveChangesAsync(cancellationToken);

    await _cache.RemoveByTagAsync("customer" , cancellationToken);

    _logger.LogInformation("Customer {CustomerId} Deleted Successfully" , request.CustomerId);

    return Result.Deleted;
  }
}