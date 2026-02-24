using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;

public static class GetWorkOrdersQueryExtension
{
  public static IQueryable<WorkOrder> ApplySearchTerm(this IQueryable<WorkOrder> query , string searchTerm)
  {
    var normalized = searchTerm.Trim().ToLower();

    return query.Where(
      wo => 
        wo.Vehicle != null && 
        (
          wo.Vehicle.Make.ToLower().Contains(normalized) ||
          wo.Vehicle.Model.ToLower().Contains(normalized) ||
          wo.Vehicle.LicensePlate.ToLower().Contains(normalized)
        ) ||

        wo.Labor != null && 
        (
          wo.Labor.FirstName.ToLower().Contains(normalized) ||
          wo.Labor.LastName.ToLower().Contains(normalized) ||
          (wo.Labor.FirstName + " " + wo.Labor.LastName).ToLower().Contains(normalized)
        ) ||

        wo.RepairTasks.Any(
          rt => 
            rt.Name.ToLower().Contains(normalized) ||
            rt.Id.ToString().ToLower().Contains(normalized)
        )
    );
  }

  public static IQueryable<WorkOrder> ApplyFilter(this IQueryable<WorkOrder> query , GetWorkOrdersQuery searchQuery)
  {
    if (searchQuery.State.HasValue)
    {
      query = query.Where(wo => wo.State == searchQuery.State.Value);
    }
    if(searchQuery.VehicleId.HasValue && searchQuery.VehicleId != Guid.Empty)
    {
      query = query.Where(wo => wo.VehicleId == searchQuery.VehicleId.Value);
    }
    if (searchQuery.LaborId.HasValue && searchQuery.LaborId != Guid.Empty)
    {
        query = query.Where(wo => wo.LaborId == searchQuery.LaborId.Value);
    }
    if (searchQuery.StartDateFrom.HasValue)
    {
      query = query.Where(wo => wo.StartAtUtc >= searchQuery.StartDateFrom.Value);
    }
    if (searchQuery.StartDateTo.HasValue)
    {
        query = query.Where(wo => wo.StartAtUtc <= searchQuery.StartDateTo.Value);
    }
    if (searchQuery.EndDateFrom.HasValue)
    {
        query = query.Where(wo => wo.EndAtUtc >= searchQuery.EndDateFrom.Value);
    }
    if (searchQuery.EndDateTo.HasValue)
    {
        query = query.Where(wo => wo.EndAtUtc <= searchQuery.EndDateTo.Value);
    }
    if (searchQuery.Spot.HasValue)
    {
        query = query.Where(wo => wo.Spot == searchQuery.Spot.Value);
    }
    return query;
  }

  public static IQueryable<WorkOrder> ApplySorting(this IQueryable<WorkOrder> query , string searchColumn , string sortDirection)
  {
    var isDesc = sortDirection.ToUpper().Equals("DESC");
    return searchColumn.ToLower() switch
    {
      "CreatedAt" => isDesc ? query.OrderByDescending(wo => wo.CreatedAtUtc)    : query.OrderBy(wo => wo.CreatedAtUtc),
      "UpdatedAt" => isDesc ? query.OrderByDescending(wo => wo.LastModifiedUtc) : query.OrderBy(wo => wo.LastModifiedUtc),
      "StartAt"   => isDesc ? query.OrderByDescending(wo => wo.StartAtUtc)      : query.OrderBy(wo => wo.StartAtUtc),
      "EndAt"     => isDesc ? query.OrderByDescending(wo => wo.EndAtUtc)        : query.OrderBy(wo => wo.EndAtUtc),
      "State"     => isDesc ? query.OrderByDescending(wo => wo.State)           : query.OrderBy(wo => wo.State),
      "Spot"      => isDesc ? query.OrderByDescending(wo => wo.Spot)            : query.OrderBy(wo => wo.Spot),
      "Total"     => isDesc ? query.OrderByDescending(wo => wo.Total)           : query.OrderBy(wo => wo.Total),
      "VehicleId" => isDesc ? query.OrderByDescending(wo => wo.VehicleId)       : query.OrderBy(wo => wo.VehicleId),
      "LaborId"   => isDesc ? query.OrderByDescending(wo => wo.LaborId)         : query.OrderBy(wo => wo.LaborId),
      _           => query.OrderBy(wo => wo.CreatedAtUtc)
    };
  }
}