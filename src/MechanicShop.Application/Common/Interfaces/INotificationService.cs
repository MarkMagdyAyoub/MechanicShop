using MechanicShop.Application.Common.Models;

namespace MechanicShop.Application.Common.Interfaces;

public interface INotificationService
{
  Task SendEmailAsync(UserEmailInfo userInfo , CancellationToken cancellationToken = default);
  Task SendSmsAsync(UserSmsInfo userInfo , CancellationToken cancellationToken = default);
}