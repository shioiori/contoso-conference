# Domain Model And DDD

## DDD Goal

DDD is used to make the business rules explicit, not to add layers for appearance. The design should help a developer explain where each rule lives, which aggregate protects each invariant, and which bounded context owns each piece of data.

## Bounded Contexts

### Event Management

Purpose:

- Manage organizer-facing Event setup.
- Manage ticket types, quotas, visibility and sales windows.

Core concepts:

- Event.
- TicketType.
- OrganizerWorkspace.
- AccessCode.

Main invariants:

- Event end date must be after start date.
- Published Event must have at least one sellable ticket type.
- Ticket quota cannot be reduced below sold/reserved count.
- Private ticket type requires a valid access code.

### Registration

Purpose:

- Convert attendee intent into orders, reservations and tickets.
- Protect inventory from oversell.

Core concepts:

- Order.
- OrderItem.
- SeatAvailability.
- Reservation.
- Ticket.

Main invariants:

- Requested quantity must respect ticket min/max per order.
- Reserved + sold quantity must never exceed quota.
- Reservation expiration releases unpaid reserved seats.
- Confirmed order issues tickets exactly once.

### Payment

Purpose:

- Normalize payment provider interactions.
- Verify callbacks and protect order confirmation from duplicated or mismatched payment events.

Core concepts:

- PaymentIntent.
- PaymentAttempt.
- PaymentCallback.
- ProviderReference.

Main invariants:

- Payment amount and currency must match the order.
- Callback signature must be valid before processing.
- Callback processing must be idempotent by provider event id.
- A late payment or captured payment that cannot produce a confirmed order and issued tickets must be routed to rollback/reconciliation.
- After an order is confirmed and tickets are issued, paid transactions are final-sale and are not refundable.

### Notification

Purpose:

- Send transactional communication without coupling email delivery to order transactions.

Core concepts:

- NotificationRequest.
- Template.
- DeliveryAttempt.

Main invariants:

- A notification request is deduplicated by notification id.
- Delivery attempts are tracked for support and retry.

### Check-in

Purpose:

- Admit attendees at the event and prevent duplicate entry.

Core concepts:

- CheckInRecord.
- TicketCode.
- Operator.
- OverrideReason.

Main invariants:

- Ticket must be active and belong to the selected Event.
- Ticket can be checked in only once unless authorized override is granted.
- Duplicate override requires audit data.

### Reporting

Purpose:

- Provide organizer-friendly sales, seat and attendance views without loading transactional databases.

Core concepts:

- SalesSummary.
- AttendanceSummary.
- ExportJob.

Main invariants:

- Reporting is eventually consistent.
- Export requires permission and audit trail.

## Aggregates

### Event Aggregate

Root: Event.

Contains:

- Event metadata.
- Visibility state.
- Date/time rules.

Protects:

- Draft/published lifecycle.
- Required fields before publish.
- Public visibility rules.

### TicketType Aggregate

Root: TicketType.

Contains:

- Name.
- Quota.
- Price.
- Currency.
- Sales window.
- Per-order limit.
- Visibility/access policy.

Protects:

- Quota changes.
- Sellability rules.
- Price and currency consistency.

### SeatAvailability Aggregate

Root: SeatAvailability.

Contains:

- Event id.
- Ticket type availability rows.
- Sold, reserved and available counters.
- Version/concurrency token.

Protects:

- No oversell.
- Atomic reserve/release/sell transitions.
- Per-ticket consistency under concurrent checkout.

### Order Aggregate

Root: Order.

Contains:

- Buyer information.
- Order items.
- State.
- Reservation expiration.
- Payment reference.

States:

- PendingReservation.
- Reserved.
- Confirmed.
- Cancelled.
- Expired.

Protects:

- Valid state transitions.
- Confirmation only after valid payment or zero amount.
- Cancellation policy.
- Ticket issuance trigger.

### Ticket Aggregate

Root: Ticket.

Contains:

- Ticket code.
- Attendee identity.
- Ticket status.
- Check-in state.

Protects:

- Ticket code uniqueness.
- Active/cancelled status.
- Check-in eligibility.

## Value Objects

- Money: amount and currency.
- DateRange: start and end date/time.
- SalesWindow: open and close time for a ticket selling phase; different ticket types may use different windows for phases such as Early Bird, Regular and Last Minute.
- EmailAddress: normalized email format.
- PersonalInfo: attendee full name and email.
- TicketCode: unique human/scannable ticket identifier.
- AccessCode: private registration access.

## Domain Events

Domain events stay inside the same bounded context and represent important facts after aggregate state changes:

- EventCreated.
- EventPublished.
- TicketTypeCreated.
- SeatsReserved.
- ReservationExpired.
- OrderConfirmed.
- TicketIssued.
- TicketCheckedIn.

## Integration Events

Integration events cross service boundaries and should be versioned contracts:

- EventPublishedIntegrationEvent.
- TicketInventoryChangedIntegrationEvent.
- OrderCreatedIntegrationEvent.
- OrderConfirmedIntegrationEvent.
- PaymentConfirmedIntegrationEvent.
- TicketIssuedIntegrationEvent.
- CheckInCompletedIntegrationEvent.

## Anti-Corruption Boundaries

- Payment provider payloads must be translated into internal payment concepts before affecting orders.
- Email provider failures must be translated into delivery attempt results.
- AWS-specific messaging details must stay behind event bus abstractions.

## Interview Talking Points

- SeatAvailability is separate from Order because oversell prevention needs a focused consistency boundary.
- Reporting is separated because organizer reports should not pressure transactional checkout tables.
- Payment callback idempotency is a business invariant, not just technical retry handling.
- Domain events express internal state changes; integration events are stable contracts for other contexts.
