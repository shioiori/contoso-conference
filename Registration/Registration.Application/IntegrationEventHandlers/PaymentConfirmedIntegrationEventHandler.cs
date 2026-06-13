using Eventbox.EventBus.Core.Abstractions;
using Eventbox.Registration.Application.Commands;
using Eventbox.Registration.Application.IntegrationEvents;
using MediatR;

namespace Eventbox.Registration.Application.IntegrationEventHandlers;

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
