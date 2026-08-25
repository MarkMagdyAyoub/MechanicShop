using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Interfaces;

public sealed class NoOpNotificationService : INotificationService
{
    public Task SendEmailAsync(UserEmailInfo userInfo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SendSmsAsync(UserSmsInfo userInfo, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}