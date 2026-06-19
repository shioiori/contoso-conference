using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EventBus.RabbitMQ
{
    public class EventMapping
    {
        public Type EventType { get; init; } = default!;
        public string Exchange { get; init; } = default!;
        public string ExchangeType { get; init; } = default!;
        public string Queue { get; init; } = default!;
        public string RoutingKey { get; init; } = default!;
        public string DeadLetterQueue { get; init; } = default!;
        public string DelayQueue { get; init; } = default!;
        public int RetryDelayMilliseconds { get; init; } = 10000;
        public int MaxRetries { get; init; } = 3;
    }

    public class QueueDeclaration
    {
        public string Queue { get; init; } = default!;
        public string Exchange { get; init; } = default!;
        public string RoutingKey { get; init; } = default!;
        public Dictionary<string, object?> Arguments { get; init; } = new();
    }

    public class RabbitMQOptions
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
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
            var queueName = queue ?? $"{exchange}.{rk}";
            SubscribeMappings.Add(new EventMapping
            {
                EventType = typeof(TEvent),
                Exchange = exchange,
                ExchangeType = exchangeType,
                RoutingKey = rk,
                Queue = queueName,
                DeadLetterQueue = $"{queueName}.dlq",
                DelayQueue = $"{queueName}.delay"
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
