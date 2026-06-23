namespace Eventbox.Payment.Api.Options;

public class TicketingOptions
{
    public const string SectionName = "TicketingApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string InternalServiceToken { get; set; } = string.Empty;
}
