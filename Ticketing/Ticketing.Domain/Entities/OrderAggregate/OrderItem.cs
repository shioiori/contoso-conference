using Eventbox.TicketingDomain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventbox.TicketingDomain.Entities.OrderAggregate
{
    public class OrderItem : Entity<Guid>
    {
        private OrderItem()
        {
        }

        public OrderItem(int ticketTypeId, int quantity)
            : this(ticketTypeId, 1, $"Ticket type {ticketTypeId}", "Default", 0, "VND", quantity)
        {
        }

        public OrderItem(
            int ticketTypeId,
            int pricingPhaseId,
            string ticketTypeName,
            string pricingPhaseName,
            decimal unitPrice,
            string currency,
            int quantity)
        {
            if (ticketTypeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(ticketTypeId), "Ticket type id must be greater than zero.");

            if (pricingPhaseId <= 0)
                throw new ArgumentOutOfRangeException(nameof(pricingPhaseId), "Pricing phase id must be greater than zero.");

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

            Id = Guid.NewGuid();
            TicketTypeId = ticketTypeId;
            PricingPhaseId = pricingPhaseId;
            TicketTypeName = ticketTypeName.Trim();
            PricingPhaseName = pricingPhaseName.Trim();
            UnitPrice = unitPrice;
            Currency = currency.Trim().ToUpperInvariant();
            Quantity = quantity;
        }

        public int TicketTypeId { get; private set; }
        public int PricingPhaseId { get; private set; }
        public string TicketTypeName { get; private set; } = "";
        public string PricingPhaseName { get; private set; } = "";
        public decimal UnitPrice { get; private set; }
        public string Currency { get; private set; } = "";
        public int Quantity { get; private set; }
    }
}
