namespace Contoso.ServiceBus.RabbitMQ;

public sealed class SubscriptionRegistry
{
    private readonly Dictionary<string, (Type EventType, Type HandlerType)> _subscriptions = new();

    public void Register(string routingKey, Type eventType, Type handlerType)
        => _subscriptions[routingKey] = (eventType, handlerType);

    public bool TryGet(string routingKey, out Type eventType, out Type handlerType)
    {
        if (_subscriptions.TryGetValue(routingKey, out var entry))
        {
            eventType = entry.EventType;
            handlerType = entry.HandlerType;
            return true;
        }

        eventType = typeof(object);
        handlerType = typeof(object);
        return false;
    }

    public IReadOnlyCollection<string> RoutingKeys => _subscriptions.Keys;
}
