using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Commands;
using Eventbox.Ticketing.Application.Messages;
using MediatR;

namespace Eventbox.Ticketing.Application.MessageHandlers;

public sealed class OrderExpirationDueMessageHandler
    : IIntegrationEventHandler<OrderExpirationDueMessageIntergrationEvent>
{
    private readonly IMediator _mediator;

    public OrderExpirationDueMessageHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task HandleAsync(
        OrderExpirationDueMessageIntergrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(
            new ExpireOrderCommand { OrderId = @event.OrderId },
            cancellationToken);
    }
}
