using System.Net;
using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Notification.Api.Email;
using Eventbox.Notification.Api.Resources;
using QRCoder;

namespace Eventbox.Notification.Api.IntegrationEvents.EventHandlers;

public sealed class OrderConfirmedIntegrationEventHandler(
        IEmailSender emailSender,
        IResourceLocalizer<EmailTemplates> localizer,
        ILogger<OrderConfirmedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    public async Task HandleAsync(OrderConfirmedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending confirmation email for order {OrderId} to {Email}", @event.OrderId, @event.CustomerEmail);

        var itemRows = string.Join("", @event.Items.Select(i => string.Format(
            localizer["OrderConfirmedItemRow"],
            WebUtility.HtmlEncode(i.TicketTypeName),
            i.Quantity,
            i.UnitPrice.ToString("N0"),
            i.Currency)));

        var ticketCards = string.Join("", @event.Tickets.Select(t => string.Format(
            localizer["OrderConfirmedTicketCard"],
            GenerateQrCodeBase64(t.QrToken),
            WebUtility.HtmlEncode(t.TicketTypeName),
            t.SequenceNumber)));

        var body = string.Format(
            localizer["OrderConfirmedBody"],
            WebUtility.HtmlEncode(@event.CustomerName),
            WebUtility.HtmlEncode(@event.AccessCode),
            itemRows,
            ticketCards,
            @event.OrderId);

        await emailSender.SendEmailAsync(
            [@event.CustomerEmail],
            localizer["OrderConfirmedSubject"],
            body,
            cancellationToken);

        logger.LogInformation("Confirmation email sent for order {OrderId}", @event.OrderId);
    }

    private static string GenerateQrCodeBase64(string content)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var code = new PngByteQRCode(data);
        return Convert.ToBase64String(code.GetGraphic(4));
    }
}
