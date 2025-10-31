using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace dotnet.Service.Email;

public interface IEmailService
{
  Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
  private readonly EmailSettings _settings;
  private readonly ILogger<EmailService> _logger;

  public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
  {
    _settings = options.Value;
    _logger = logger;
  }

  public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromAddress))
    {
      _logger.LogWarning("Email settings are incomplete. Skipping sending email to {Email}", toEmail);
      return;
    }

    using var message = new MailMessage
    {
      From = new MailAddress(_settings.FromAddress, string.IsNullOrWhiteSpace(_settings.FromName) ? _settings.FromAddress : _settings.FromName),
      Subject = subject,
      Body = htmlBody,
      IsBodyHtml = true
    };

    message.To.Add(toEmail);

    using var client = new SmtpClient(_settings.Host, _settings.Port)
    {
      EnableSsl = _settings.EnableSsl,
      Credentials = !string.IsNullOrWhiteSpace(_settings.Username)
        ? new NetworkCredential(_settings.Username, _settings.Password)
        : CredentialCache.DefaultNetworkCredentials
    };

    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      await client.SendMailAsync(message);
      _logger.LogInformation("Verification email sent to {Email}", toEmail);
    }
    catch (OperationCanceledException)
    {
      _logger.LogWarning("Sending email to {Email} was cancelled", toEmail);
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
      throw;
    }
  }
}
