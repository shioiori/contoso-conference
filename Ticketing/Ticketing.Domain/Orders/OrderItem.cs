using Eventbox.Shared.SeedWorks;

namespace Eventbox.Ticketing.Domain.Orders
{
    public class OrderItem : AuditableEntity<Guid>
    {
        public OrderItem(Guid ticketTypeId, int quantity)
            : this(ticketTypeId, Guid.NewGuid(), $"Ticket type {ticketTypeId}", "Default", 0, "VND", quantity)
        {

        }

        public OrderItem(
            Guid ticketTypeId,
            Guid pricingPhaseId,
            string ticketTypeName,
            string pricingPhaseName,
            decimal unitPrice,
            string currency,
            int quantity) : base(Guid.NewGuid())
        {
            if (ticketTypeId == Guid.Empty)
                throw new ArgumentException("Ticket type id must not be empty.", nameof(ticketTypeId));

            if (pricingPhaseId == Guid.Empty)
                throw new ArgumentException("Pricing phase id must not be empty.", nameof(pricingPhaseId));

            if (string.IsNullOrWhiteSpace(ticketTypeName))
                throw new ArgumentException("Ticket type name is required.", nameof(ticketTypeName));

            if (string.IsNullOrWhiteSpace(pricingPhaseName))
                throw new ArgumentException("Pricing phase name is required.", nameof(pricingPhaseName));

            if (unitPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            TicketTypeId = ticketTypeId;
            PricingPhaseId = pricingPhaseId;
            TicketTypeName = ticketTypeName.Trim();
            PricingPhaseName = pricingPhaseName.Trim();
            UnitPrice = unitPrice;
            Currency = currency.Trim().ToUpperInvariant();
            Quantity = quantity;
        }

        public Guid TicketTypeId { get; private set; }
        public Guid PricingPhaseId { get; private set; }
        public string TicketTypeName { get; private set; } = "";
        public string PricingPhaseName { get; private set; } = "";
        public decimal UnitPrice { get; private set; }
        public string Currency { get; private set; } = "";
        public int Quantity { get; private set; }
    }
}
