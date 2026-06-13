using System.Text;
using Eventbox.EventBus.Core.Abstractions;
using Newtonsoft.Json;

namespace Eventbox.EventBus.Serialization;

public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerSettings _settings;

    public JsonMessageSerializer() : this(DefaultSettings()) { }

    public JsonMessageSerializer(JsonSerializerSettings settings)
    {
        _settings = settings;
    }

    public byte[] Serialize<T>(T message)
    {
        var json = JsonConvert.SerializeObject(message, _settings);
        return Encoding.UTF8.GetBytes(json);
    }

    public T? Deserialize<T>(byte[] data)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject(json, type, _settings);
    }

    private static JsonSerializerSettings DefaultSettings() => new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Formatting = Formatting.None,
    };
}
