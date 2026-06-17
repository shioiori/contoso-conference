using Eventbox.Ticketing.Domain.Enums;

namespace Eventbox.Ticketing.Domain.Entities.TicketAvailabilityAggregate
{
    public class TicketTypeAvailability
    {
        private readonly List<PricingPhaseAvailability> _pricingPhases = new();

        private TicketTypeAvailability()
        {
            Name = default!;
            Currency = default!;
        }

        public TicketTypeAvailability(int ticketTypeId, int quantity)
            : this(
                ticketTypeId,
                $"Ticket type {ticketTypeId}",
                quantity,
                "VND",
                1,
                null,
                TicketVisibility.Public,
                null,
                [new PricingPhaseAvailability(1, "Default", 0, null, null)])
        {
        }

        public TicketTypeAvailability(
            int id,
            string name,
            int quantity,
            string currency,
            int minPerOrder,
            int? maxPerOrder,
            TicketVisibility visibility,
            string? accessCodeHash,
            IEnumerable<PricingPhaseAvailability> pricingPhases)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Ticket type id must be greater than zero.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ticket type name is required.", nameof(name));

            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            if (minPerOrder < 1)
                throw new ArgumentOutOfRangeException(nameof(minPerOrder), "Minimum per order must be at least 1.");

            if (maxPerOrder.HasValue && maxPerOrder < minPerOrder)
                throw new ArgumentOutOfRangeException(nameof(maxPerOrder), "Maximum per order must be greater than or equal to minimum per order.");

            if (maxPerOrder.HasValue && maxPerOrder > quantity)
                throw new ArgumentOutOfRangeException(nameof(maxPerOrder), "Maximum per order cannot exceed quantity.");

            if (visibility == TicketVisibility.AccessCode && string.IsNullOrWhiteSpace(accessCodeHash))
                throw new ArgumentException("Access code hash is required for access-code ticket types.", nameof(accessCodeHash));

            var phases = pricingPhases.ToList();
            if (phases.Count == 0)
                throw new ArgumentException("At least one pricing phase is required.", nameof(pricingPhases));

            Id = id;
            Name = name.Trim();
            Quantity = quantity;
            Remaining = quantity;
            Currency = currency.Trim().ToUpperInvariant();
            MinPerOrder = minPerOrder;
            MaxPerOrder = maxPerOrder;
            Visibility = visibility;
            AccessCodeHash = string.IsNullOrWhiteSpace(accessCodeHash) ? null : accessCodeHash.Trim();
            _pricingPhases.AddRange(phases);
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public int Remaining { get; private set; }
        public string Currency { get; private set; }
        public int MinPerOrder { get; private set; }
        public int? MaxPerOrder { get; private set; }
        public TicketVisibility Visibility { get; private set; }
        public string? AccessCodeHash { get; private set; }
        public IReadOnlyCollection<PricingPhaseAvailability> PricingPhases => _pricingPhases.AsReadOnly();

        public PricingPhaseAvailability GetActivePricingPhase(DateTimeOffset utcNow)
            => _pricingPhases.FirstOrDefault(phase => phase.IsActive(utcNow))
                ?? throw new InvalidOperationException("Ticket type is not currently on sale.");

        public void Reserve(int quantity)
        {
            EnsureOrderQuantityAllowed(quantity);

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            if (Remaining < quantity)
                throw new InvalidOperationException("Not enough tickets remaining.");

            Remaining -= quantity;
        }

        public void Release(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            Remaining = Math.Min(Quantity, Remaining + quantity);
        }

        public void IncreaseQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to add must be positive.");

            Quantity += quantity;
            Remaining += quantity;
        }

        public void EnsureCanAccess(string? accessCodeHash)
        {
            if (Visibility == TicketVisibility.Public)
                return;

            if (Visibility == TicketVisibility.AccessCode &&
                !string.IsNullOrWhiteSpace(accessCodeHash) &&
                string.Equals(AccessCodeHash, accessCodeHash, StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidOperationException("Ticket type is not available for public Ticketing");
        }

        public void EnsureOrderQuantityAllowed(int quantity)
        {
            if (quantity < MinPerOrder)
                throw new InvalidOperationException($"Minimum quantity for this ticket type is {MinPerOrder}.");

            if (MaxPerOrder.HasValue && quantity > MaxPerOrder)
                throw new InvalidOperationException($"Maximum quantity for this ticket type is {MaxPerOrder}.");
        }
    }
}
