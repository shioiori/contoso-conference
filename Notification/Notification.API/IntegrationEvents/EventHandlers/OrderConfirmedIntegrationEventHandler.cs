using System.Net;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Notification.Api.Email;

namespace Eventbox.Notification.Api.IntegrationEvents.EventHandlers;

public sealed class OrderConfirmedIntegrationEventHandler(IEmailSender emailSender,
        ILogger<OrderConfirmedIntegrationEventHandler> logger) 
    : IIntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    public async Task HandleAsync(OrderConfirmedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending confirmation email for order {OrderId} to {Email}", @event.OrderId, @event.CustomerEmail);

        var subject = "Your Eventbox order is confirmed!";
        var body = BuildEmailBody(@event);

        await emailSender.SendAsync(@event.CustomerEmail, @event.CustomerName, subject, body, cancellationToken);

        logger.LogInformation("Confirmation email sent for order {OrderId}", @event.OrderId);
    }

    private static string BuildEmailBody(OrderConfirmedIntegrationEvent @event)
    {
        var itemRows = string.Join("", @event.Items.Select(i =>
            $"<tr>" +
            $"<td style='padding:8px 16px;border-bottom:1px solid #eee'>{WebUtility.HtmlEncode(i.TicketTypeName)}</td>" +
            $"<td style='padding:8px 16px;border-bottom:1px solid #eee;text-align:center'>{i.Quantity}</td>" +
            $"<td style='padding:8px 16px;border-bottom:1px solid #eee;text-align:right'>{i.UnitPrice:N0} {i.Currency}</td>" +
            $"</tr>"));

        return
            "<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:0 auto;padding:20px'>" +
            $"<h2 style='color:#4f46e5'>Order Confirmed!</h2>" +
            $"<p>Hi {WebUtility.HtmlEncode(@event.CustomerName)},</p>" +
            "<p>Your order has been confirmed. Use the access code below to look up your tickets:</p>" +
            "<div style='background:#f5f5f5;border-radius:8px;padding:24px;text-align:center;margin:24px 0'>" +
            "<p style='margin:0 0 8px;color:#666;font-size:14px'>ACCESS CODE</p>" +
            $"<p style='margin:0;font-size:32px;font-weight:bold;letter-spacing:6px;color:#4f46e5'>{WebUtility.HtmlEncode(@event.AccessCode)}</p>" +
            "</div>" +
            "<table style='width:100%;border-collapse:collapse;margin-top:24px'>" +
            "<thead><tr style='background:#4f46e5;color:white'>" +
            "<th style='padding:10px 16px;text-align:left'>Ticket</th>" +
            "<th style='padding:10px 16px;text-align:center'>Qty</th>" +
            "<th style='padding:10px 16px;text-align:right'>Price</th>" +
            "</tr></thead>" +
            $"<tbody>{itemRows}</tbody></table>" +
            $"<p style='margin-top:32px;color:#666;font-size:13px'>Order ID: {@event.OrderId}</p>" +
            "<p style='color:#666;font-size:13px'>If you have any questions, please contact our support team.</p>" +
            "</body></html>";
    }
}
