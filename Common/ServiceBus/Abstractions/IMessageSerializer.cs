namespace Contoso.ServiceBus.Abstractions;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);
    T? Deserialize<T>(byte[] data);
    object? Deserialize(byte[] data, Type type);
}
