using Eventbox.EventManagement.EventApi.Domains.Enums;
using Eventbox.Shared.Exceptions;
using Eventbox.Shared.SeedWorks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("TicketType")]
    public class TicketType : AuditableEntity<Guid>
    {
        private readonly List<PricingPhase> _pricingPhases = [];
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid EventId { get; set; }
        public int Quota { get; set; }
        public string Currency { get; set; } = "VND";
        public int MinPerOrder { get; set; } = 1;
        public int? MaxPerOrder { get; set; }
        public TicketVisibility Visibility { get; set; } = TicketVisibility.Public;
        public string? AccessCodeHash { get; set; }
        public Event Event { get; set; } = default!;
        public IReadOnlyCollection<PricingPhase> PricingPhases => _pricingPhases.AsReadOnly();

        public TicketType(
            string name,
            Guid eventId,
            int quota,
            string? description,
            string currency,
            int minPerOrder,
            int? maxPerOrder,
            TicketVisibility visibility,
            string? accessCodeHash,
            IEnumerable<PricingPhase> pricingPhases) : base(Guid.NewGuid())
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ticket type name is required.", nameof(name));

            if (eventId == Guid.Empty)
                throw new ArgumentException("Event id is required.", nameof(eventId));

            if (quota < 0)
                throw new ArgumentOutOfRangeException(nameof(quota), "Quota cannot be negative.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            ValidateOrderLimits(minPerOrder, maxPerOrder, quota);
            ValidateAccessPolicy(visibility, accessCodeHash);

            var phases = pricingPhases.ToList();
            if (phases.Count == 0)
                throw new ArgumentException("At least one pricing phase is required.", nameof(pricingPhases));

            EnsureNoOverlappingPricingPhases(phases);

            Name = name.Trim();
            EventId = eventId;
            Quota = quota;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Currency = currency.Trim().ToUpperInvariant();
            MinPerOrder = minPerOrder;
            MaxPerOrder = maxPerOrder;
            Visibility = visibility;
            AccessCodeHash = string.IsNullOrWhiteSpace(accessCodeHash) ? null : accessCodeHash.Trim();
            Id = Guid.NewGuid();
            _pricingPhases.AddRange(phases);
        }

        public void Update(
            string name,
            string? description,
            int quota,
            string currency,
            int minPerOrder,
            int? maxPerOrder,
            TicketVisibility visibility,
            string? accessCodeHash,
            IEnumerable<PricingPhase> pricingPhases)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ticket type name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            if (quota < 0)
                throw new ArgumentOutOfRangeException(nameof(quota), "Quota cannot be negative.");

            ValidateOrderLimits(minPerOrder, maxPerOrder, quota);
            ValidateAccessPolicy(visibility, accessCodeHash);

            var phases = pricingPhases.ToList();
            if (phases.Count == 0)
                throw new ArgumentException("At least one pricing phase is required.", nameof(pricingPhases));

            EnsureNoOverlappingPricingPhases(phases);

            Name = name.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Quota = quota;
            Currency = currency.Trim().ToUpperInvariant();
            MinPerOrder = minPerOrder;
            MaxPerOrder = maxPerOrder;
            Visibility = visibility;
            AccessCodeHash = string.IsNullOrWhiteSpace(accessCodeHash) ? null : accessCodeHash.Trim();
            _pricingPhases.Clear();
            _pricingPhases.AddRange(phases);
        }

        public void ValidatePricingPhasesAgainstEvent(DateTimeOffset eventFrom, DateTimeOffset eventTo)
        {
            foreach (var phase in _pricingPhases)
            {
                if (phase.StartTime.HasValue && phase.StartTime < eventFrom)
                    throw new ValidationApiException($"Pricing phase '{phase.Name}' starts before the event starts.");

                if (phase.EndTime.HasValue && phase.EndTime > eventTo)
                    throw new ValidationApiException($"Pricing phase '{phase.Name}' ends after the event ends.");
            }
        }

        public void IncreaseQuota(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to add must be positive.");
            Quota += quantity;
        }

        private static void ValidateOrderLimits(int minPerOrder, int? maxPerOrder, int quota)
        {
            if (minPerOrder < 1)
                throw new ArgumentOutOfRangeException(nameof(minPerOrder), "Minimum per order must be at least 1.");

            if (maxPerOrder.HasValue)
            {
                if (maxPerOrder < minPerOrder)
                    throw new ArgumentOutOfRangeException(nameof(maxPerOrder), "Maximum per order must be greater than or equal to minimum per order.");

                if (maxPerOrder > quota)
                    throw new ArgumentOutOfRangeException(nameof(maxPerOrder), "Maximum per order cannot exceed quota.");
            }
        }

        private static void ValidateAccessPolicy(TicketVisibility visibility, string? accessCodeHash)
        {
            if (visibility == TicketVisibility.AccessCode && string.IsNullOrWhiteSpace(accessCodeHash))
                throw new ArgumentException("Access code hash is required for access-code ticket types.", nameof(accessCodeHash));
        }

        private static void EnsureNoOverlappingPricingPhases(IReadOnlyList<PricingPhase> phases)
        {
            for (var i = 0; i < phases.Count; i++)
            {
                for (var j = i + 1; j < phases.Count; j++)
                {
                    if (phases[i].Overlaps(phases[j]))
                        throw new ValidationApiException("Pricing phases cannot overlap.");
                }
            }
        }
    }
}
