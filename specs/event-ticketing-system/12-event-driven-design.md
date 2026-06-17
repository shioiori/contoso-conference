# Event-Driven Design

## EDA Goal

Event-driven architecture is used to decouple bounded contexts while preserving the business facts needed for registration, notification, reporting and support workflows. Events should be explicit, versioned and safe to retry.

## Event Principles

- Events are named as past-tense facts.
- Events carry stable business identifiers.
- Events include occurredAt, correlationId and causationId.
- Consumers must be idempotent.
- Producers publish only after database transaction commit.
- Event contracts are versioned when payload meaning changes.

## Event Catalog

### EventPublished

Producer: Event Management.

Consumers:

- Registration creates or updates sellability snapshot.
- Reporting updates public catalog/read model.

Payload:

- eventId.
- organizationId.
- name.
- slug.
- startDate.
- endDate.
- timezone.
- occurredAt.

### TicketTypeCreated

Producer: Event Management.

Consumers:

- Registration creates seat availability.
- Reporting updates ticket inventory view.

Payload:

- eventId.
- ticketTypeId.
- name.
- quota.
- price.
- currency.
- salesWindow.
- visibility.

### SeatsAdded

Producer: Event Management.

Consumers:

- Registration increases available quota.
- Reporting updates inventory summary.

Payload:

- eventId.
- ticketTypeId.
- quantityAdded.
- newQuota.

### OrderCreated

Producer: Registration.

Consumers:

- Reporting updates funnel metrics.
- Payment creates payment intent when total amount is greater than zero.

Payload:

- orderId.
- eventId.
- buyerEmail.
- items.
- totalAmount.
- currency.
- reservationExpiresAt.

### SeatsReserved

Producer: Registration.

Consumers:

- Reporting updates reserved count.
- Public availability read model refreshes availability.

Payload:

- orderId.
- eventId.
- reservedItems.
- reservationExpiresAt.

### ReservationExpired

Producer: Registration expiration worker.

Consumers:

- Reporting decreases reserved count.
- Notification may send expiration message if needed.

Payload:

- orderId.
- eventId.
- expiredItems.
- expiredAt.

### PaymentConfirmed

Producer: Payment Adapter.

Consumers:

- Registration confirms order.
- Reporting updates revenue.

Payload:

- paymentId.
- providerEventId.
- orderId.
- amount.
- currency.
- paidAt.

### OrderConfirmed

Producer: Registration.

Consumers:

- Notification sends confirmation email.
- Reporting updates sold count and revenue.
- Check-in creates searchable ticket lookup after ticket issuance.

Payload:

- orderId.
- eventId.
- buyerEmail.
- confirmedItems.
- totalAmount.
- currency.

### TicketIssued

Producer: Registration.

Consumers:

- Notification includes ticket codes in confirmation.
- Check-in updates ticket lookup.
- Reporting updates active ticket count.

Payload:

- ticketId.
- ticketCode.
- orderId.
- eventId.
- ticketTypeId.
- attendeeEmail.
- attendeeName.

### CheckInCompleted

Producer: Check-in.

Consumers:

- Reporting updates attendance count.
- Audit records operator activity.

Payload:

- ticketId.
- ticketCode.
- eventId.
- operatorId.
- checkedInAt.
- wasOverride.

## Delivery Patterns

### Outbox Pattern

Use outbox for critical state changes:

- Save aggregate changes and outbox messages in the same database transaction.
- A background publisher reads pending outbox rows.
- Mark outbox messages as published after successful broker publish.
- Retry failed publishes with backoff.

Required for:

- OrderConfirmed.
- TicketIssued.
- PaymentConfirmed.
- ReservationExpired.

### Inbox / Idempotent Consumer

Each consumer stores processed message ids:

- Reject duplicate processing by messageId or providerEventId.
- Keep handler logic safe to retry.
- Store processing status and failure reason for support.

Required for:

- Payment callback handling.
- Order confirmation from PaymentConfirmed.
- Notification sending.
- Reporting read model updates.

### Dead-Letter Handling

Messages that fail repeatedly should move to a dead-letter queue or failed message table.

Support view should expose:

- Message type.
- Message id.
- Correlation id.
- Error reason.
- Last attempted at.
- Retry action.

## Consistency Choices

- Seat reservation is strongly consistent inside Registration.
- Public availability is eventually consistent and may lag briefly.
- Reporting is eventually consistent.
- Notification is eventually consistent and should not block checkout.
- Payment confirmation is eventually consistent but must be idempotent and auditable.

## Event Versioning

Rules:

- Additive fields are backward compatible.
- Removing or changing field meaning requires a new version.
- Consumers should ignore unknown fields.
- Event names may include version suffix for external contracts, such as OrderConfirmed.v1.

## Local And Cloud Broker Mapping

Local development:

- RabbitMQ topic exchange for integration events.
- Durable queues per consumer.
- Manual ack, retry and dead-letter configuration.

AWS deployment option:

- EventBridge for domain-wide event routing.
- SQS queues per service consumer.
- SNS for fan-out notification scenarios.
- Lambda or ECS worker for background consumers.

## Interview Talking Points

- Outbox avoids losing events after database commit.
- Inbox/idempotency makes retries safe.
- Public availability can be eventually consistent because reserve command rechecks inventory.
- Notification and reporting should be async so checkout remains fast and reliable.

