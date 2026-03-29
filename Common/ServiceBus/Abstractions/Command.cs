namespace Contoso.ServiceBus.Abstractions;

public abstract record Command : ICommand
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
    public DateTime IssuedOn { get; init; } = DateTime.UtcNow;
}
