using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;

public sealed class SendWorkOrderCompletedEmailEventHandler(
  IAppDbContext context,
  ILogger<SendWorkOrderCompletedEmailEventHandler> logger,
  INotificationService notificationService
) : INotificationHandler<WorkOrderCompleted>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<SendWorkOrderCompletedEmailEventHandler> _logger = logger;
  private readonly INotificationService _notificationService = notificationService;

  public async Task Handle(WorkOrderCompleted notification, CancellationToken cancellationToken)
  {
    var workOrder = await _context.WorkOrders
                          .Include(wo => wo.Vehicle!)
                            .ThenInclude(v => v.Customer)
                          .FirstOrDefaultAsync(wo => wo.Id == notification.WorkOrderId , cancellationToken);

    if(workOrder is null)
    {
      _logger.LogWarning("Work Order With Id `{workOrderId}` Is Not Found" , notification.WorkOrderId);
      return;
    }

    await _notificationService.SendEmailAsync(
      new UserEmailInfo
      {
        UserName = workOrder.Vehicle?.Customer?.Name!,
        Email = workOrder.Vehicle?.Customer?.Email!,
      }, 
      cancellationToken
    );

    await _notificationService.SendSmsAsync(
      new UserSmsInfo
      {
        UserName = workOrder.Vehicle?.Customer?.Name!,
        PhoneNumber = workOrder.Vehicle?.Customer?.PhoneNumber!
      },
      cancellationToken
    );
    
  }
}