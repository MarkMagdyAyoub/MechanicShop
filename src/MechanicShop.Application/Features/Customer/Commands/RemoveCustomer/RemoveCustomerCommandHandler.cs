using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MediatR;

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

    // TODO: check if the customer has work orders or not

    _context.Customers.Remove(customer);

    await _context.SaveChangeAsync(cancellationToken);

    await _cache.RemoveByTagAsync("customer" , cancellationToken);

    _logger.LogInformation("Customer {CustomerId} Deleted Successfully" , request.CustomerId);

    return Result.Deleted;
  }
}