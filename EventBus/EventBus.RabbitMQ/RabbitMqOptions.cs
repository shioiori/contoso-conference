namespace Eventbox.EventBus.RabbitMQ;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string VirtualHost { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string EventExchange { get; set; } = default!;
    public string CommandExchange { get; set; } = default!;
    public string QueuePrefix { get; set; } = default!;

    public ushort PrefetchCount { get; set; }
}
