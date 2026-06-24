using System.Collections;
using System.Reflection;

namespace Eventbox.Shared.Auditing;

public static class AuditRedactor
{
    private static readonly string[] SensitiveNameParts =
    [
        "password",
        "token",
        "secret",
        "signature",
        "accesscode",
        "access_code",
        "apikey",
        "api_key",
        "key"
    ];

    public static object? RedactObject(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var type = value.GetType();
        if (IsSimple(type))
        {
            return value;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            return enumerable.Cast<object?>().Select(RedactObject).ToArray();
        }

        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetIndexParameters().Length == 0)
            .ToDictionary(
                property => property.Name,
                property => IsSensitive(property.Name)
                    ? "***REDACTED***"
                    : RedactObject(property.GetValue(value)));
    }

    public static object? RedactProperty(string propertyName, object? value)
        => IsSensitive(propertyName) ? "***REDACTED***" : value;

    private static bool IsSensitive(string name)
    {
        var normalized = name.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveNameParts.Any(part =>
            normalized.Contains(part.Replace("_", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal));
    }

    private static bool IsSimple(Type type)
        => type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(Guid);
}
