# Architecture Overview

## Architecture Goal

Hệ thống được thiết kế như một SaaS backend có nhiều bounded context, giao tiếp qua REST API cho luồng đồng bộ và integration events cho luồng bất đồng bộ. Mục tiêu chính là bảo vệ seat inventory, giữ order/payment/ticketing nhất quán, đồng thời cho phép reporting, notification và check-in mở rộng độc lập.

Kiến trúc cần chứng minh được các năng lực trong JD:

- Thiết kế RESTful APIs bằng .NET.
- Tách service theo business capability.
- Giải thích được Component Context từ architecture design.
- Giao tiếp bất đồng bộ bằng EDA.
- Có chiến lược reliability, security, testing và observability.

## System Context

Actors:

- Organizer quản lý Event, ticket inventory, attendee list, check-in operation và reporting.
- Attendee xem Event public, đăng ký, thanh toán, nhận ticket và tự tra cứu order.
- Check-in Operator scan QR hoặc lookup thủ công tại venue.
- Payment Provider gửi callback xác nhận thanh toán.
- Email Provider gửi transactional emails.
- Platform Admin vận hành, audit và hỗ trợ technical support cases.

External systems:

- Identity Provider cho authentication/authorization.
- Payment Provider cho paid order.
- Email Provider cho notification.
- Object Storage cho export/report/badge artifacts.
- Observability platform cho logs, metrics, traces và alerts.

## Container View

### Event API

Responsibilities:

- Event lifecycle: draft, update, publish, unpublish.
- Ticket type/quota management.
- Public Event detail and availability summary.
- Publish Event/ticket inventory integration events.

Owned data:

- Events.
- Ticket types.
- Organizer-facing configuration.

### Registration API

Responsibilities:

- Create order and reserve seats.
- Confirm free/paid orders.
- Cancel or expire reservations.
- Issue tickets after order confirmation.
- Expose attendee self-service order lookup.

Owned data:

- Orders.
- Order items.
- Seat availability.
- Tickets.

### Payment Adapter

Responsibilities:

- Create payment intent for paid orders.
- Receive and verify payment callbacks.
- Normalize provider-specific payloads into payment integration events.
- Enforce idempotency for repeated callbacks.

Owned data:

- Payment attempts.
- Provider references.
- Callback processing records.

### Notification Worker

Responsibilities:

- Consume notification requests.
- Send confirmation, resend, cancellation and operational emails.
- Track delivery attempts.

Owned data:

- Notification requests.
- Delivery attempts.

### Check-in API

Responsibilities:

- QR-based check-in.
- Manual attendee lookup.
- Duplicate check-in prevention and override policy.

Owned data:

- Check-in records.
- Operator action audit.

### Reporting API / Read Model Worker

Responsibilities:

- Build read models from order, ticket, payment and check-in events.
- Expose sales and attendance summaries.
- Support export without loading transactional databases.

Owned data:

- Sales summary read model.
- Attendance summary read model.
- Export job records.

## Component Context

### Registration API Components

- API Endpoints validate request shape, authentication and authorization scope.
- Application Commands coordinate use cases such as Start Registration, Confirm Payment and Cancel Order.
- Domain Aggregates enforce business invariants for Order and SeatAvailability.
- Repositories persist aggregate changes inside transactions.
- Integration Event Publisher emits events after successful transaction commit through outbox.
- Background Expiration Worker expires unpaid reservations.

Key decisions:

- Seat reservation is strongly consistent per ticket type.
- Payment confirmation is idempotent.
- Ticket issuance is triggered only after an order reaches Confirmed.

### Event API Components

- Controllers expose organizer and public APIs.
- Application Services enforce publish/update rules.
- Event and TicketType aggregates protect lifecycle rules.
- Event Publisher sends inventory and visibility changes to downstream contexts.

Key decisions:

- Published Event details may be cached.
- Registration always revalidates sellability and availability before reserving seats.

### Notification Worker Components

- Event Consumer receives notification request events.
- Template Resolver selects email template based on event type.
- Email Provider Client sends the message.
- Delivery Tracker stores attempts and failure reasons.

Key decisions:

- Notification failure must not roll back a confirmed order.
- Messages are retryable and deduplicated by notification request id.

## Data Ownership

- A service may read another service's data only through API, event-fed read model, or explicitly documented query endpoint.
- No service writes directly to another service's database.
- Reporting uses read models and exports instead of joining transactional databases.
- Integration events carry stable business identifiers and enough facts for downstream processing.

## Communication Rules

- Use REST for request/response workflows where the caller needs immediate feedback.
- Use integration events for state changes that other contexts need to react to.
- Use commands/background jobs for scheduled or delayed work such as reservation expiration.
- Include correlation id and causation id in logs, events and key database records.

## MVP Architecture Slice

The portfolio-ready MVP should implement this minimum slice:

- Event API.
- Registration API.
- Payment callback endpoint with simulated provider.
- Notification contract or worker stub.
- Reporting summary endpoint.
- RabbitMQ locally for event bus.
- Docker compose for local infrastructure.
- Unit and integration tests for critical flows.

