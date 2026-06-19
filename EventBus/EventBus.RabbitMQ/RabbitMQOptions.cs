using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EventBus.RabbitMQ
{
    public class EventMapping
    {
        public Type EventType { get; init; }
        public string Exchange { get; init; }
        public string ExchangeType { get; init; }
        public string Queue { get; init; }
        public string RoutingKey { get; init; }
    }

    public class QueueDeclaration
    {
        public string Queue { get; init; }
        public string Exchange { get; init; }
        public string RoutingKey { get; init; }
        public Dictionary<string, object?> Arguments { get; init; } = new();
    }

    public class RabbitMQOptions
    {
        public string HostName { get; set; }
        public int Port { get; set; } = 5672;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";

        public List<EventMapping> PublishMappings { get; } = new();
        public List<EventMapping> SubscribeMappings { get; } = new();
        public List<QueueDeclaration> ExtraQueues { get; } = new();

        public RabbitMQOptions Publish<TEvent>(
            string exchange,
            string exchangeType = ExchangeType.Direct,
            string? routingKey = null)
        {
            PublishMappings.Add(new EventMapping
            {
                EventType = typeof(TEvent),
                Exchange = exchange,
                ExchangeType = exchangeType,
                RoutingKey = routingKey ?? ToRoutingKey(typeof(TEvent))
            });
            return this;
        }

        public RabbitMQOptions Subscribe<TEvent>(
            string exchange,
            string exchangeType = ExchangeType.Direct,
            string? routingKey = null,
            string? queue = null)
        {
            var rk = routingKey ?? ToRoutingKey(typeof(TEvent));
            SubscribeMappings.Add(new EventMapping
            {
                EventType = typeof(TEvent),
                Exchange = exchange,
                ExchangeType = exchangeType,
                RoutingKey = rk,
                Queue = queue ?? $"{exchange}.{rk}"
            });
            return this;
        }

        public RabbitMQOptions DeclareQueue(
            string queue,
            string exchange,
            string routingKey,
            Dictionary<string, object?>? arguments = null)
        {
            ExtraQueues.Add(new QueueDeclaration
            {
                Queue = queue,
                Exchange = exchange,
                RoutingKey = routingKey,
                Arguments = arguments ?? new()
            });
            return this;
        }

        private static string ToRoutingKey(Type type)
        {
            var name = type.Name;
            if (name.EndsWith("IntegrationEvent")) name = name[..^"IntegrationEvent".Length];
            else if (name.EndsWith("IntergrationEvent")) name = name[..^"IntergrationEvent".Length];
            else if (name.EndsWith("Event")) name = name[..^"Event".Length];

            return Regex.Replace(name, "(?<!^)([A-Z])", ".$1").ToLowerInvariant();
        }
    }
}
