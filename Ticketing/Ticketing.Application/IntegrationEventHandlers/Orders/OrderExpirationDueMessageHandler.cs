using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Commands.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Orders;

public sealed class OrderExpirationDueMessageHandler
    : IIntegrationEventHandler<OrderExpirationDueMessageIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderExpirationDueMessageHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task HandleAsync(
        OrderExpirationDueMessageIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(
            new ExpireOrderCommand { OrderId = @event.OrderId },
            cancellationToken);
    }
}
