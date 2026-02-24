namespace MechanicShop.Application.Common.Interfaces;

public interface INotificationService
{
  Task SendEmailAsync(string to , CancellationToken cancellationToken = default);
  Task SendSmsAsync(string to , CancellationToken cancellationToken = default);
}