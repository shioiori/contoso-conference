# Ticketing Internal Module Boundaries

`Ticketing` is deployed as one service in the MVP, but it is organized as a set of internal modules so the domain can evolve without turning the service into a catch-all boundary.

## Orders

Owns the checkout/order lifecycle:

- Creating orders and reservations.
- Confirming, cancelling, and expiring orders.
- Coordinating ticket issuance when an order is confirmed.
- Handling payment confirmation facts from the Payment context.

## Inventory

Owns ticket-type availability inside the Ticketing service:

- Event and ticket-type availability snapshots.
- Reserved, sold, and available counters.
- Capacity changes consumed from Event Management.

This module can become a separate Inventory bounded context if the product later supports assigned seating.

## Tickets

Owns issued ticket artifacts:

- Ticket identifiers and QR token generation.
- Ticket state such as active or cancelled.
- Ticket DTOs exposed to other Ticketing modules.

For the MVP, tickets are issued inside the order confirmation transaction. If ticket lifecycle rules grow, this module is the natural extraction point for a separate Ticketing service.

## CheckIn

Owns the check-in use case while remaining inside the MVP service:

- QR validation.
- Event/time-window validation.
- Cancelled-ticket rejection.
- Atomic duplicate check-in prevention.

`CheckInCompletedIntegrationEvent` is emitted after a successful check-in. Reporting, audit, and a future standalone Check-in service should consume this event instead of reading order tables directly.

Future extraction path:

1. Build a Check-in ticket lookup read model from `TicketIssued`, `TicketInvalidated`, and event schedule facts.
2. Keep the atomic "mark checked in once" operation inside the Check-in boundary.
3. Emit `CheckInCompletedIntegrationEvent` for Reporting and Audit.
4. Leave Orders/Tickets as upstream producers; Check-in should not mutate order state directly.
