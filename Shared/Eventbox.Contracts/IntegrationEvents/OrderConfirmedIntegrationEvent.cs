using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class OrderConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string AccessCode { get; set; }
    public List<OrderItemInfo> Items { get; set; } 
    public List<TicketInfo> Tickets { get; set; }
}

public sealed class OrderItemInfo
{
    public string TicketTypeName { get; set; } 
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; }
}

public sealed class TicketInfo
{
    public Guid TicketId { get; set; }
    public string TicketTypeName { get; set; }
    public int SequenceNumber { get; set; }
    public string QrToken { get; set; }
}
