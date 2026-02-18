using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labor.Mappers;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.DTOs;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;

public sealed class GetDailyScheduleQueryHandler(
  IAppDbContext context,
  ILogger<GetDailyScheduleQueryHandler> logger,
  TimeProvider timeProvider
) : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<GetDailyScheduleQueryHandler> _logger = logger;
  private readonly TimeProvider _timeProvider = timeProvider;

  public async Task<Result<ScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken cancellationToken)
  {
    _logger.LogInformation(
      "Input -> Date: {Date}, TimeZone: {TimeZone}, LaborId: {LaborId}",
      request.ScheduleDate,
      request.TimeZone.DisplayName,
      request.LaborId
    );

    var localStart = request.ScheduleDate.ToDateTime(TimeOnly.MinValue); 
    var localEnd = localStart.AddDays(1); 

    _logger.LogInformation("Local Start: {LocalStart}", localStart);
    _logger.LogInformation("Local End: {LocalEnd}", localEnd);

    var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart , request.TimeZone); 
    var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd , request.TimeZone); 

    _logger.LogInformation("UTC Start: {UtcStart}", utcStart);
    _logger.LogInformation("UTC End: {UtcEnd}", utcEnd);

    var workOrders = await _context.WorkOrders
                          .Where( w => 
                                w.StartAtUtc < utcEnd &&
                                w.EndAtUtc > utcStart &&
                                (request.LaborId == null || w.LaborId == request.LaborId)
                          )
                          .Include(w => w.Vehicle)
                          .Include(w => w.RepairTasks)
                          .Include(w => w.Labor)
                          .ToListAsync(cancellationToken);

    _logger.LogInformation("Retrieved {Count} Work Orders", workOrders.Count);
      
    // Get the current time in UTC from the TimeProvider.
    // Then convert it to the requested timezone (e.g., Egypt UTC+2).
    //
    // Example:
    // If current UTC time is 11:00
    // and the requested timezone is Egypt (UTC+2),
    // the local time becomes 13:00.
    //
    // We do this because availability is calculated
    // relative to the user's local time, not UTC.
    var now = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), request.TimeZone);
    _logger.LogInformation("Current Local Time: {Now}", now);

    var result = new ScheduleDto
    {
      OnDate = request.ScheduleDate,
      IsPastDate =  localEnd < now, 
      Spots = []
    };

    foreach (var spot in Enum.GetValues<Spot>())
    {
      var current = localStart;
      var segments = new List<SegmentInfoDto>();

      var workOrdersRelatedToSpot = workOrders
                                    .Where(w => w.Spot == spot)
                                    .OrderBy(w => w.StartAtUtc)
                                    .ToList();

      while(current < localEnd)
      {
        var nextSegment = current.AddMinutes(15);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(current , request.TimeZone);
        var endUtc   = TimeZoneInfo.ConvertTimeToUtc(nextSegment , request.TimeZone);

        var isThereWorkOrderInThisSegment = workOrdersRelatedToSpot
                                            .FirstOrDefault(w => w.StartAtUtc < endUtc && w.EndAtUtc > startUtc);
        
        if(isThereWorkOrderInThisSegment is not null)
        {
          if(!segments.Any(s => s.WorkOrderId == isThereWorkOrderInThisSegment.Id))
          {
            segments.Add(new SegmentInfoDto
            {
              WorkOrderId = isThereWorkOrderInThisSegment.Id,
              Spot = spot,
              StartAt = isThereWorkOrderInThisSegment.StartAtUtc,
              EndAt = isThereWorkOrderInThisSegment.EndAtUtc,
              IsAvailable = false,
              WorkOrderLocked = isThereWorkOrderInThisSegment.IsEditable, 
              IsOccupied = true,
              RepairTasks = isThereWorkOrderInThisSegment.RepairTasks.Select(rt => rt.ToDto()).ToList(),
              Labor = isThereWorkOrderInThisSegment.Labor!.ToDto(),
              WorkOrderState = isThereWorkOrderInThisSegment.State,
              Vehicle = FormatVehicleInfo(isThereWorkOrderInThisSegment.Vehicle!)
            });
          }
        }
        else
        {
          // free segment
          segments.Add(new SegmentInfoDto
          {
            Spot = spot,
            StartAt = startUtc,
            EndAt = endUtc,
            WorkOrderLocked = false,
            IsAvailable = current >= now
          });
        }

        current = nextSegment;
      }

      result.Spots.Add(new SpotDto
      {
        Spot = spot,
        Segments = segments
      });
    }

    return result;
  }

  public static string? FormatVehicleInfo(Vehicle vehicle) => 
    vehicle != null ? $"{vehicle.Make} | {vehicle.LicensePlate}" : null;
}