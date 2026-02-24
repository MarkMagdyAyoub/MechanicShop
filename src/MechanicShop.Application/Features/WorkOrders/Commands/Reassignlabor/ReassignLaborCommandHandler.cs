using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MechanicShop.Domain.Identity;
namespace MechanicShop.Application.Features.WorkOrders.Commands.ReassignLabor;


public sealed class ReassignLaborCommandHandler(
  IAppDbContext context,
  ILogger<ReassignLaborCommandHandler> logger,
  HybridCache cache
) : IRequestHandler<ReassignLaborCommand, Result<Updated>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<ReassignLaborCommandHandler> _logger = logger;
  private readonly HybridCache _cache = cache;

  public async Task<Result<Updated>> Handle(ReassignLaborCommand request, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders.FindAsync(request.WorkOrderId , cancellationToken);
    
    if (workOrder is null)
    {
        _logger.LogWarning("WorkOrder with Id `{WorkOrderId}` Does Not Exist.", request.WorkOrderId);
        return ApplicationErrors.WorkOrderNotFound;
    }

    var labor = await _context.Employees.SingleOrDefaultAsync(e => e.Id == request.LaborId && e.Role == Role.Labor , cancellationToken);
    
    if(labor is null)
    {
      _logger.LogWarning("Labor With Id `{LaborId}` Not Found", request.LaborId);
      return ApplicationErrors.LaborNotFound;
    }

    var isOccupied = await _context.WorkOrders
                          .AnyAsync(
                            wo => 
                              wo.LaborId == request.LaborId && 
                              wo.Id != request.WorkOrderId &&
                              wo.StartAtUtc < workOrder.EndAtUtc &&
                              wo.EndAtUtc > workOrder.StartAtUtc,
                            cancellationToken
                          );
    if (isOccupied)
    {
      _logger.LogError("Labor With Id '{LaborId}' Is Already Occupied During The Requested Time.", workOrder.LaborId);
      return ApplicationErrors.LaborOccupied;
    }

    var updateWorkOrderLaborResult = workOrder.UpdateLabor(request.LaborId);

    if (updateWorkOrderLaborResult.IsError)
    {
      _logger.LogWarning("Update Work Order Labor Failed: {Errors}" , updateWorkOrderLaborResult.Errors);
      return updateWorkOrderLaborResult.Errors;
    }

    await _context.SaveChangesAsync(cancellationToken);
    
    await _cache.RemoveByTagAsync("work-order" , cancellationToken);
    
    return Result.Updated;
  }
}