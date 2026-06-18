# Functional Requirements

## Event Management

### FR-CM-001 Create Event

System shall allow an organizer to create an event in draft/private state.

Acceptance criteria:

- Required fields: name, slug, start date, end date, timezone.
- End date must be after start date.
- Slug must be unique according to platform policy.
- System generates access code if private access is enabled.
- New event appears in organizer Events list as Draft.

### FR-CM-002 Update Event

System shall allow organizer to update draft or published event details.

Acceptance criteria:

- Updating dates validates all ticket sales windows.
- Audit log records before/after critical fields.
- If event is published, attendee-facing page reflects the latest saved information.

### FR-CM-003 Publish/Unpublish Event

System shall allow organizer to publish or unpublish an event.

Acceptance criteria:

- Publish requires valid date/time and at least one sellable ticket type.
- Published event appears in public catalog.
- Unpublished event is hidden from public catalog.
- Publish/unpublish action is audit logged.

### FR-CM-004 Create Ticket Type

System shall allow organizer to create ticket/seat type.

Acceptance criteria:

- Required fields: name, eventId, quota.
- Price, currency, sales window and per-order limit follow event defaults when not explicitly set.
- Organizer may configure multiple ticket types with different sales windows to support selling phases such as Early Bird, Regular and Last Minute.
- Ticket appears in Tickets overview after save.

### FR-CM-005 Add Seats

System shall allow organizer to increase quota for a ticket type.

Acceptance criteria:

- Quantity must be positive.
- Available ticket count is recalculated after seats are added.
- Quota change is audit logged.

## Public Catalog

### FR-PC-001 View Published Events

System shall expose published events for attendee browsing.

Acceptance criteria:

- Draft/private events are excluded.
- Response includes ticket availability summary.
- Sold out events are clearly marked.

### FR-PC-002 View Event Detail

System shall expose full public details for one event.

Acceptance criteria:

- Shows event metadata, ticket types, price, sales window, availability.
- Hidden ticket types are excluded unless valid access code is supplied.

## Registration

### FR-RG-001 Start Registration

System shall allow attendee to create an order by selecting ticket quantities and buyer info.

Acceptance criteria:

- Event must be published.
- Ticket type must be visible/sellable or accessible by access code.
- Requested quantity respects min/max per order.
- System reserves seats atomically.
- Reservation expiration is returned.
- Attendee sees a checkout countdown after successful reservation.

### FR-RG-002 Complete Free Order

System shall confirm order without external payment when total amount is zero.

Acceptance criteria:

- Order moves to Confirmed.
- Attendee tickets are issued.
- Confirmation page and confirmation email are available.

### FR-RG-003 Confirm Paid Order

System shall confirm order after successful payment provider event.

Acceptance criteria:

- Payment amount and currency must match order.
- Payment provider reference is stored.
- Order moves to Confirmed.
- Reserved seats become Sold.
- Tickets are issued.
- Confirmation notification is requested.
- If payment succeeds but the order cannot be confirmed or tickets cannot be issued, system must start a payment rollback/reconciliation workflow and must not silently create invalid tickets.

### FR-RG-004 Expire Reservation

System shall expire unpaid reserved order after expiration time.

Acceptance criteria:

- Order moves to Expired.
- Reserved seats are released.
- Payment intent is cancelled if possible.
- Attendee sees reservation expired state if checkout page is still open.

### FR-RG-005 Cancel Order

System shall support cancellation by attendee or organizer subject to policy.

Acceptance criteria:

- Reserved unpaid order cancellation releases seats.
- Confirmed paid orders are final-sale and are not refundable for any reason.
- Confirmed paid order cancellation, if allowed operationally, only invalidates tickets and records activity; it does not return money to the attendee.
- Cancelled tickets cannot be checked in.
- Cancellation confirmation is visible in order activity.

## Attendee Management

### FR-AM-001 View Attendees

System shall allow organizer to view attendee list per event.

Acceptance criteria:

- Supports filtering by ticket type, status, checked-in state, email/name.
- Supports pagination and export.

### FR-AM-002 Update Attendee Info

System shall allow authorized staff to edit attendee profile fields.

Acceptance criteria:

- Ticket identity fields have audit log.
- Email change can resend confirmation.

### FR-AM-003 Resend Confirmation

System shall allow confirmation email to be resent.

Acceptance criteria:

- Only active tickets can receive confirmation.
- Delivery attempt is tracked.

## Check-in

### FR-CI-001 Check In By QR

System shall allow operator to scan QR code and check in attendee.

Acceptance criteria:

- Ticket must exist and be active.
- Ticket must belong to selected event.
- Duplicate check-in is rejected unless override permission is granted.
- System stores check-in timestamp and operator.

### FR-CI-002 Manual Lookup

System shall allow operator to search attendee by name/email/ticket code.

Acceptance criteria:

- Search is scoped to event.
- Result shows ticket status and check-in state.

## Reporting

### FR-RP-001 Sales Report

System shall provide organizer with ticket sales and revenue report.

Acceptance criteria:

- Shows sold, reserved, available by ticket type.
- Shows gross revenue, payment rollbacks/reconciliation adjustments, and recognized revenue.
- Includes time range filter.

### FR-RP-002 Attendance Report

System shall provide check-in and no-show report.

Acceptance criteria:

- Shows total active tickets, checked-in count, no-show count.
- Supports ticket type breakdown.
