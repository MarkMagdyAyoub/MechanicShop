using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;


public sealed class GetWorkOrdersQueryHandler(
  IAppDbContext context,
  ILogger<GetWorkOrdersQueryHandler> logger
) : IRequestHandler<GetWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetWorkOrdersQueryHandler> _logger = logger;

  public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
  {
    var workOrders = _context.WorkOrders
                          .AsNoTracking()
                          .Include(wo => wo.Vehicle!)
                            .ThenInclude(v => v.Customer)
                          .Include(wo => wo.RepairTasks)
                            .ThenInclude(rt => rt.Parts)
                          .Include(wo => wo.Labor)
                          .Include(wo => wo.Invoice)
                          .AsQueryable();
    
    workOrders = workOrders.ApplyFilter(request);

    if(request.SearchTerm is not null)
    {
      workOrders = workOrders.ApplySearchTerm(request.SearchTerm);
    }

    workOrders = workOrders.ApplySorting(request.SearchColumn , request.SortDirection);

    var count = await workOrders.CountAsync(cancellationToken);

    var items = await workOrders
                      .Skip((request.PageNumber - 1) * request.PageSize)
                      .Take(request.PageSize)
                      .Select(
                        wo => new WorkOrderListItemDto
                          {
                            WorkOrderId = wo.Id,
                            InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
                            Spot = wo.Spot,
                            StartAtUtc = wo.StartAtUtc,
                            EndAtUtc = wo.EndAtUtc,
                            Vehicle = wo.Vehicle!.ToDto(),
                            Customer = wo.Vehicle!.Customer!.Name,
                            Labor = wo.Labor != null
                              ? wo.Labor.FirstName + " " + wo.Labor.LastName
                              : null,
                            State = wo.State,
                            RepairTasks = wo.RepairTasks.Select(rt => rt.Name).ToList()
                          }
                      )
                      .ToListAsync(cancellationToken);
    return new PaginatedList<WorkOrderListItemDto>
    {
      Items = items,
      PageNumber = request.PageNumber,
      PageSize = request.PageSize,
      TotalCount = count,
      TotalPages = (int)Math.Ceiling(count / (double)request.PageSize)
    };
  }
}