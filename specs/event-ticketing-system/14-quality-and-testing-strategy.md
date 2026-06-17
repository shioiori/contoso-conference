# Quality And Testing Strategy

## Quality Goal

The system should be demonstrably reliable in the areas that matter most: seat reservation, payment confirmation, ticket issuance, check-in and event processing. Tests should prove business behavior, not only controller wiring.

## Test Pyramid

### Unit Tests

Focus:

- Aggregates.
- Value objects.
- Domain services.
- Application command validation.

Examples:

- Event cannot be published without sellable ticket type.
- SeatAvailability cannot reserve more seats than available.
- Order cannot be confirmed twice.
- Payment mismatch does not confirm order.
- Duplicate check-in is rejected without override permission.

### Integration Tests

Focus:

- API endpoints.
- EF Core persistence.
- Transaction boundaries.
- Event outbox/inbox behavior.

Examples:

- Start registration creates order and reserves seats in one transaction.
- Confirm payment changes order state and issues tickets.
- Expire reservation releases seats.
- Public Event detail hides private ticket types without access code.
- Organizer endpoint rejects unauthorized Event access.

### Contract Tests

Focus:

- Integration event schemas.
- Consumer expectations.
- Backward-compatible changes.

Examples:

- OrderConfirmed event contains required stable identifiers.
- PaymentConfirmed consumer handles duplicate providerEventId.
- Unknown event fields do not break consumers.

### Concurrency Tests

Focus:

- Prevent oversell under concurrent registration attempts.

Scenarios:

- 100 concurrent requests compete for 10 seats.
- Reservation expiration runs while confirmation is attempted.
- Two payment callbacks arrive for the same order.
- Duplicate check-in requests arrive for the same ticket.

Expected result:

- Sold + reserved never exceeds quota.
- Exactly one successful confirmation per order.
- Exactly one successful check-in unless override is authorized.

### Performance Tests

Focus:

- Checkout latency.
- Check-in speed.
- Attendee search.
- Reporting isolation.

Targets from NFR:

- Public Event detail p95 <= 300 ms.
- Registration command p95 <= 800 ms excluding payment provider time.
- Check-in command p95 <= 300 ms.
- Attendee search p95 <= 500 ms for a large Event.

### Security Tests

Focus:

- Authentication.
- Authorization.
- PII handling.
- Payment callback trust boundary.

Checklist:

- Organizer cannot access another organization's Event.
- CSV export requires explicit permission.
- Payment callback requires signature verification.
- Logs do not contain card data or unnecessary attendee PII.
- Sensitive configuration is loaded from secrets, not source code.

## Definition Of Done

A feature is complete when:

- Business acceptance criteria are implemented.
- Unit tests cover aggregate rules.
- Integration tests cover the main API path.
- Critical events are published through outbox.
- Consumer handlers are idempotent.
- Structured logs include correlation id.
- Errors are mapped to useful API responses.
- README or architecture docs are updated when behavior changes.

## Portfolio Readiness Checklist

Before the project is used in CV:

- Solution builds from a clean checkout.
- Docker compose starts required local infrastructure.
- README explains architecture, local setup and critical workflows.
- At least one end-to-end registration flow works.
- Tests cover seat reservation, payment confirmation and check-in duplicate prevention.
- Architecture docs include DDD, EDA and AWS deployment mapping.
- Sample API requests are available through OpenAPI, Postman collection or documented curl examples.

## Interview Talking Points

- The highest-risk tests are concurrency and idempotency tests, not simple CRUD tests.
- Event-driven systems need contract tests because events become service boundaries.
- Reporting should be tested separately from transactional workflows.
- Observability is part of quality because support cases require traceable order/payment/ticket history.

