using System.Text.Json;

namespace Eventbox.Shared.Auditing;

internal static class AuditSerialization
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string Serialize(object? value)
        => JsonSerializer.Serialize(value, Options);
}
