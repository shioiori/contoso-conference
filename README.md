# Eventbox API

Version 2 of Eventbox API, an event ticketing backend built with .NET 10, PostgreSQL, RabbitMQ, and an event-driven microservice layout.

## Overview

Eventbox v2 covers the core flow for organizers publishing events and customers registering for tickets:

- Organizer accounts create organizations, events, and ticket types.
- Public users discover published events and check ticket availability.
- Customers or guests create orders for an event.
- Orders reserve ticket availability and can expire automatically.
- Payment simulation confirms paid orders through RabbitMQ integration events.
- Organizers check attendees in by QR token at the door.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| Auth API | `Auth/Auth.API` | Customer/organizer registration, login, JWT issuance, current-user endpoint. |
| Event API | `EventManagement/Event.API` | Organizations, organizer event management, public event discovery, ticket type management. |
| Ticketing API | `Ticketing/Ticketing.API` | Public checkout, customer orders, order cancellation/confirmation, ticket availability, order expiration jobs, QR check-in. |
| Payment API | `Payment/Payment.API` | Payment intent creation and simulated payment provider callbacks. |
| EventBus | `EventBus/EventBus`, `EventBus/EventBus.Core`, `EventBus/EventBus.RabbitMQ` | Shared abstractions and RabbitMQ implementation for integration events, commands, and serialization. |
| Contracts | `Shared/Eventbox.Contracts` | Shared integration event contracts across services. |
| Shared | `Shared/Eventbox.Shared` | Cross-cutting concerns: outbox, exception handling, auditing. |
| CheckIn Domain | `CheckIn/CheckIn.Domain` | Domain model for check-in operations shared by Ticketing. |

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core with PostgreSQL
- ASP.NET Core Identity for Auth
- JWT Bearer authentication and account-type authorization policies
- RabbitMQ for integration events
- Transactional outbox pattern for reliable event publishing
- MediatR for Ticketing and Payment application flows
- Hangfire with PostgreSQL storage for order expiration reconciliation jobs
- Mapster for object mapping
- Serilog for structured request and application logging
- xUnit v3 unit tests
- Docker Compose for local infrastructure and API services

## Solution Structure

```text
Eventbox.slnx
├── Auth/
│   └── Auth.API/
├── EventManagement/
│   └── Event.API/
├── Ticketing/
│   ├── Ticketing.API/
│   ├── Ticketing.Application/
│   ├── Ticketing.Domain/
│   └── Ticketing.Infrastructure/
├── Payment/
│   ├── Payment.API/
│   ├── Payment.Core/
│   └── Payment.Infrastructure/
├── CheckIn/
│   └── CheckIn.Domain/
├── EventBus/
│   ├── EventBus/
│   ├── EventBus.Core/
│   └── EventBus.RabbitMQ/
├── Shared/
│   ├── Eventbox.Contracts/
│   └── Eventbox.Shared/
├── Tests/
├── postman/
└── specs/
```

## Main API Surface

### Auth API

Base URL when running locally: `http://localhost:8000`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/customers/register` | Public | Register a customer account and return a JWT. |
| `POST` | `/api/auth/organizers/register` | Public | Register an organizer account and return a JWT. |
| `POST` | `/api/auth/login` | Public | Login by email/password and return a JWT. |
| `GET` | `/api/auth/me` | Bearer token | Return current user id, email, and `account_type`. |

### Event API

Base URL when running locally: `http://localhost:8010`

Public routes:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/public/events` | Search/list published events. |
| `GET` | `/api/public/events/{slug}` | Get detail for a published event. |

Organizer routes require a JWT with `account_type = Organizer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/organizations` | Create an organization. |
| `GET` | `/api/organizations/{id}` | Get organization detail. |
| `PUT` | `/api/organizations/{id}` | Update an organization. |
| `DELETE` | `/api/organizations/{id}` | Delete an organization. |
| `GET` | `/api/organizations/{organizationId}/events` | Search/list organizer events. |
| `POST` | `/api/organizations/{organizationId}/events` | Create a draft event. |
| `GET` | `/api/organizations/{organizationId}/events/{id}` | Get organizer event detail. |
| `PUT` | `/api/organizations/{organizationId}/events/{id}` | Update an event. |
| `GET` | `/api/organizations/{organizationId}/events/{id}/publish-readiness` | Check whether an event can be published. |
| `POST` | `/api/organizations/{organizationId}/events/{id}/publish` | Publish an event. |
| `POST` | `/api/organizations/{organizationId}/events/{id}/unpublish` | Unpublish an event. |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | List ticket types for an event. |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | Create a ticket type. |
| `PATCH` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}` | Update a ticket type. |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/capacity` | Add capacity to a ticket type. |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/availability` | Get ticket type availability. |

### Ticketing API

Base URL when running locally: `http://localhost:8020`

Public routes:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/public/events/{eventId}/ticket-availability` | Get ticket availability by event id. |
| `POST` | `/api/public/events/{eventId}/orders` | Create an order/reservation. Supports guest checkout when email is supplied. |
| `POST` | `/api/public/order-lookup-requests` | Request a self-service order lookup email. |
| `GET` | `/api/public/self-service/orders?token={token}` | Get an order by self-service token. |

Customer routes require a JWT with `account_type = Customer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/customer/orders` | Get orders for the current customer. |
| `GET` | `/api/customer/orders/by-email?email={email}` | Get orders by the authenticated customer's email. |
| `GET` | `/api/customer/orders/{orderId}` | Get order detail if owned by current customer. |
| `POST` | `/api/customer/orders/{orderId}/confirm-free` | Confirm a free order. |
| `POST` | `/api/customer/orders/{orderId}/cancel` | Cancel an order. |

Organizer routes require a JWT with `account_type = Organizer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/events/{eventId}/check-ins` | Check in an attendee by QR token. |

Ticketing also hosts the Hangfire dashboard at `/hangfire`.

### Payment API

Base URL when running locally: `http://localhost:8030`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/payments/intents` | Bearer token or order access code | Create/reuse a payment intent. Supports `Idempotency-Key`. |
| `POST` | `/api/payments/simulated-callbacks` | `X-Provider-Signature` header | Simulate a provider callback for `Succeeded` or `Failed` payment status. |

## Event Flow

1. Organizer registers through Auth API and receives a JWT with `account_type = Organizer`.
2. Organizer creates an organization, creates an event, creates ticket types, adds capacity, and publishes the event.
3. Event API publishes integration events via the outbox pattern (event creation/publication, ticket capacity changes).
4. Ticketing service consumes those events and maintains ticket availability.
5. Customer or guest creates an order through Ticketing API.
6. Ticketing reserves seats, schedules order expiration via Hangfire, and exposes order/customer APIs.
7. Payment API verifies order access with Ticketing API, creates a payment intent, and handles simulated provider callbacks.
8. Successful payment publishes `PaymentConfirmedIntegrationEvent` via the outbox; Ticketing confirms the order.
9. Hangfire periodically reconciles expired orders.
10. Organizer checks in attendees at the door by scanning QR tokens through the check-in endpoint.

## Local Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run Docker Compose

```bash
docker compose up --build
```

Current compose configuration starts:

- `auth.api` on host ports `8000` (HTTP) and `8001` (HTTPS)
- `event.api` on host ports `8010` (HTTP) and `8011` (HTTPS)
- `ticketing.api` on host ports `8020` (HTTP) and `8021` (HTTPS)
- `payment.api` on host ports `8030` (HTTP) and `8031` (HTTPS)
- `eventdb` PostgreSQL on `localhost:5432`
- `rabbitmq` on `localhost:5672`, management UI on `http://localhost:15672`

Compose uses Docker service names for cross-container calls. For example, `payment.api` calls Ticketing through `http://ticketing.api:8080`, and API services connect to PostgreSQL and RabbitMQ through `eventdb` and `rabbitmq`.

Default Docker HTTP endpoints:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:8000` |
| Event API | `http://localhost:8010` |
| Ticketing API | `http://localhost:8020` |
| Payment API | `http://localhost:8030` |

### Run Locally With Docker Infrastructure

Start only PostgreSQL and RabbitMQ:

```bash
docker compose up -d eventdb rabbitmq
```

Then run the APIs:

```bash
dotnet run --project Auth/Auth.API
dotnet run --project EventManagement/Event.API
dotnet run --project Ticketing/Ticketing.API
dotnet run --project Payment/Payment.API
```

Default local HTTP ports from `launchSettings.json`:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:8000` |
| Event API | `http://localhost:8010` |
| Ticketing API | `http://localhost:8020` |
| Payment API | `http://localhost:8030` |

## Configuration

Each API reads settings from `appsettings.json`, `appsettings.Development.json`, user secrets, or environment variables.

Common settings:

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=EventDb;Username=postgres;Password=Eventbox123!"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "Issuer": "Eventbox.Auth",
    "Audience": "Eventbox.Api",
    "SigningKey": "eventbox-development-signing-key-change-me"
  }
}
```

Payment API also requires:

```json
{
  "TicketingApi": {
    "BaseUrl": "http://localhost:8020"
  },
  "Payment": {
    "ProviderSignature": "your-development-signature"
  }
}
```

Auth API requires `Jwt:SigningKey` at runtime. The signing key must be at least 32 characters and can be supplied through user secrets or environment variables.

## Database Migrations

Auth API:

```bash
dotnet ef migrations add <MigrationName> --project Auth/Auth.API
```

Event API:

```bash
dotnet ef migrations add <MigrationName> --project EventManagement/Event.API --output-dir Migrations
```

Ticketing API:

```bash
dotnet ef migrations add <MigrationName> --project Ticketing/Ticketing.Infrastructure --startup-project Ticketing/Ticketing.API
```

Payment API:

```bash
dotnet ef migrations add <MigrationName> --project Payment/Payment.Infrastructure --startup-project Payment/Payment.API
```

All services apply migrations automatically on startup.

## Tests

Run unit tests:

```bash
dotnet test Eventbox.slnx
```

Current unit test coverage focuses on:

- Ticketing ticket availability behavior
- Ticketing concurrency for event registration
- Payment callback behavior

## Postman

The `postman/` folder contains:

- `Event.API.postman_collection.json`
- `Eventbox.Docker.postman_environment.json`

Import both files into Postman and select the **Eventbox Docker** environment to test against the Docker Compose setup.
