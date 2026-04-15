namespace Contoso.EventBus.Abstractions;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);
    T? Deserialize<T>(byte[] data);
    object? Deserialize(byte[] data, Type type);
}
