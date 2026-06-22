using Eventbox.EventBus.Events;

namespace Eventbox.Contracts.IntegrationEvents;

public sealed class OrderConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public List<OrderItemInfo> Items { get; set; } = new();

    public sealed class OrderItemInfo
    {
        public string TicketTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
