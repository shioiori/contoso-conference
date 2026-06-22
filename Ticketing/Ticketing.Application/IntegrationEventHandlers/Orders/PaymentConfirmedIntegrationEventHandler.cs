using Eventbox.Contracts.IntegrationEvents;
using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Ticketing.Application.Commands.Orders;
using MediatR;

namespace Eventbox.Ticketing.Application.IntegrationEventHandlers.Orders;

public sealed class PaymentConfirmedIntegrationEventHandler
    : IIntegrationEventHandler<PaymentConfirmedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public PaymentConfirmedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task HandleAsync(
        PaymentConfirmedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        return _mediator.Send(
            new ConfirmOrderCommand { OrderId = @event.OrderId },
            cancellationToken);
    }
}
