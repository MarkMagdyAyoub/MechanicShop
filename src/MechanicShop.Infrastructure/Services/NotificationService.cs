using MailKit.Net.Smtp;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using MechanicShop.Application.Common;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MechanicShop.Infrastructure.Services;

public sealed class NotificationService(
  ILogger<NotificationService> logger,
  IOptions<EmailSettings> emailSettings,
  IOptions<SmsSettings> smsSettings
) : INotificationService
{
  private readonly ILogger<NotificationService> _logger = logger;
  private readonly SmsSettings _smsSettings = smsSettings.Value;
  private readonly EmailSettings _emailSettings = emailSettings.Value;

  public async Task SendEmailAsync(UserEmailInfo userInfo, CancellationToken cancellationToken = default)
  {
    var maskedEmail = UtilityService.MaskEmail(userInfo.Email);
    try
    {
        _logger.LogInformation("Starting email send to {Email} for user {UserName}", maskedEmail, userInfo.UserName);
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _emailSettings.DisplayName,
            _emailSettings.From
        ));

        email.To.Add(MailboxAddress.Parse(userInfo.Email));

        email.Subject = "Vehicle Service Completed";

        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $"""
                <h2>Vehicle Service Completed</h2>
                <p>Dear {userInfo.UserName},</p>
                <p>Your vehicle is complete. You may collect it from the shop at your earliest convenience.</p>
                <p>Thank you for choosing <strong>Mechanic Shop</strong>.</p>
                <p>Best regards,<br/>Mechanic Shop Team</p>
                """
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _emailSettings.Host,
            _emailSettings.Port,
            SecureSocketOptions.StartTls,
            cancellationToken
        );

        _logger.LogInformation("Connected to SMTP server {Host}:{Port}", _emailSettings.Host, _emailSettings.Port);

        await smtp.AuthenticateAsync(
            _emailSettings.UserName,
            _emailSettings.Password,
            cancellationToken
        );

        _logger.LogInformation("Authenticated as {UserName}", maskedEmail);

        await smtp.SendAsync(email, cancellationToken);

        _logger.LogInformation("Email successfully sent to {Email}", maskedEmail);

        await smtp.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Disconnected from SMTP server");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send email to {Email} for user {UserName}", maskedEmail, userInfo.UserName);
        throw; 
    }
  }

  public async Task SendSmsAsync(UserSmsInfo userInfo, CancellationToken cancellationToken = default)
  {
    var maskedPhone = UtilityService.MaskPhoneNumber(userInfo.PhoneNumber);
    try
    {
        _logger.LogInformation("Starting SMS send to {PhoneNumber} for user {UserName}", maskedPhone , userInfo.UserName);

        TwilioClient.Init(_smsSettings.AccountSid, _smsSettings.AuthToken);

        var message = await MessageResource.CreateAsync(
            to: new PhoneNumber(userInfo.PhoneNumber),
            from: new PhoneNumber(_smsSettings.FromNumber),
            body: $"Dear {userInfo.UserName}, your vehicle service is complete. You may collect it from the shop at your earliest convenience."
        );

        _logger.LogInformation("SMS Successfully Sent To {PhoneNumber}", maskedPhone);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send SMS to {PhoneNumber} for user {UserName}", maskedPhone , userInfo.UserName);
        throw;
    }
  }
}