namespace Eventbox.Ticketing.Api.Options;

public sealed class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    public string ServiceToken { get; set; } = string.Empty;
}
