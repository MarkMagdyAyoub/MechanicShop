using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.Infrastructure.RealTime;

public sealed class WorkOrderHub : Hub
{
  public const string HUB_URL = "/hubs/work-orders";
}