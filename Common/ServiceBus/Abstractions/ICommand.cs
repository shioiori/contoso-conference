namespace Contoso.ServiceBus.Abstractions;

public interface ICommand
{
    Guid CommandId { get; }
    DateTime IssuedOn { get; }
}
