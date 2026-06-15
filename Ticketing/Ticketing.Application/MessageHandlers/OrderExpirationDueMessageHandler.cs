using Eventbox.EventBus.Core.Abstractions;
using Eventbox.TicketingApplication.Commands;
using Eventbox.TicketingApplication.Messages;
using MediatR;

namespace Eventbox.TicketingApplication.MessageHandlers;

public sealed class OrderExpirationDueMessageHandler
    : IIntegrationEventHandler<OrderExpirationDueMessage>
{
    private readonly IMediator _mediator;

    public OrderExpirationDueMessageHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task HandleAsync(
        OrderExpirationDueMessage @event,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(
            new ExpireOrderCommand { OrderId = @event.OrderId },
            cancellationToken);
    }
}
