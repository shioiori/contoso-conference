namespace Eventbox.Notification.Api.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string[] emails, string subject, string htmlMessage, CancellationToken cancellationToken = default);
}
