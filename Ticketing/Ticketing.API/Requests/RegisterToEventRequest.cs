namespace Eventbox.TicketingApi.Requests;

public class RegisterToEventRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public int TicketTypeId { get; init; }
    public int Quantity { get; init; } = 1;
    public string? AccessCode { get; init; }
}
