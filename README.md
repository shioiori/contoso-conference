# Eventbox API

Version 1 of Eventbox API, an event ticketing backend built with .NET 10, PostgreSQL, RabbitMQ, and a small event-driven microservice layout.

## Overview

Eventbox v1 covers the core flow for organizers publishing events and customers registering for tickets:

- Organizer accounts create organizations, events, and ticket types.
- Public users discover published events and check ticket availability.
- Customers or guests create orders for an event.
- Orders reserve ticket availability and can expire automatically.
- Payment simulation confirms paid orders through RabbitMQ integration events.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| Auth API | `Auth/Auth.API` | Customer/organizer registration, login, JWT issuance, current-user endpoint. |
| Event API | `EventManagement/Event.API` | Organizations, organizer event management, public event discovery, ticket type management. |
| Registration API | `Registration/Registration.API` | Public checkout, customer orders, order cancellation/confirmation, ticket availability, order expiration jobs. |
| Payment API | `Payment/Payment.API` | Payment intent creation and simulated payment provider callbacks. |
| EventBus | `EventBus/EventBus` and `EventBus/EventBus.RabbitMQ` | Shared abstractions and RabbitMQ implementation for events, commands, delayed scheduling, and serialization. |

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core with PostgreSQL
- ASP.NET Core Identity for Auth
- JWT Bearer authentication and account-type authorization policies
- RabbitMQ for integration events and delayed order-expiration messages
- MediatR for Registration and Payment application flows
- Hangfire with PostgreSQL storage for Registration reconciliation jobs
- Mapster for object mapping
- xUnit v3 unit tests
- Docker Compose for local infrastructure and API services

## Solution Structure

```text
Eventbox.slnx
├── Auth/
│   └── Auth.API/
├── EventManagement/
│   └── Event.API/
├── Registration/
│   ├── Registration.API/
│   ├── Registration.Application/
│   ├── Registration.Domain/
│   └── Registration.Infrastructure/
├── Payment/
│   ├── Payment.API/
│   ├── Payment.Core/
│   └── Payment.Infrastructure/
├── EventBus/
│   ├── EventBus/
│   └── EventBus.RabbitMQ/
├── Tests/
│   └── Eventbox.UnitTests/
├── postman/
└── specs/
```

## Main API Surface

### Auth API

Base URL when running locally: `http://localhost:5104`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/customers/register` | Public | Register a customer account and return a JWT. |
| `POST` | `/api/auth/organizers/register` | Public | Register an organizer account and return a JWT. |
| `POST` | `/api/auth/login` | Public | Login by email/password and return a JWT. |
| `GET` | `/api/auth/me` | Bearer token | Return current user id, email, and `account_type`. |

### Event API

Base URL when running locally: `http://localhost:5145`

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
| `PATCH` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}` | Fetch/update ticket type placeholder route in current code. |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/capacity` | Add capacity to a ticket type. |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/availability` | Get ticket type availability. |

### Registration API

Base URL when running locally: `http://localhost:5151`

Public routes:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/public/events/{eventId}/ticket-availability` | Get ticket availability by event id. |
| `POST` | `/api/public/events/{eventId}/orders` | Create an order/reservation. Supports guest checkout when email is supplied. |
| `POST` | `/api/public/order-lookup-requests` | Placeholder for requesting a self-service order lookup email. |
| `GET` | `/api/public/self-service/orders?token={token}` | Get an order by self-service token. |

Customer routes require a JWT with `account_type = Customer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/customer/orders` | Get orders for the current customer email. |
| `GET` | `/api/customer/orders/by-email?email={email}` | Get orders by the authenticated customer's email. |
| `GET` | `/api/customer/orders/{orderId}` | Get order detail if owned by current customer. |
| `POST` | `/api/customer/orders/{orderId}/confirm-free` | Confirm a free order. |
| `POST` | `/api/customer/orders/{orderId}/cancel` | Cancel an order. |

Registration also hosts the Hangfire dashboard at `/hangfire`.

### Payment API

Base URL when running locally: `http://localhost:5193`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/payments/intents` | Bearer token or order access code | Create/reuse a payment intent. Supports `Idempotency-Key`. |
| `POST` | `/api/payments/simulated-callbacks` | `X-Provider-Signature` header | Simulate a provider callback for `Succeeded` or `Failed` payment status. |

## Event Flow

1. Organizer registers through Auth API and receives a JWT with `account_type = Organizer`.
2. Organizer creates an organization, creates an event, creates ticket types, adds seats, and publishes the event.
3. Event API publishes integration events such as event creation/publication and ticket capacity changes.
4. Registration service consumes relevant events and maintains ticket availability.
5. Customer or guest creates an order through Registration API.
6. Registration reserves seats, schedules order expiration, and exposes order/customer APIs.
7. Payment API verifies order access with Registration API, creates a payment intent, and handles simulated provider callbacks.
8. Successful payment publishes `PaymentConfirmedIntegrationEvent`; Registration confirms the order.
9. Hangfire periodically reconciles expired orders.

## Local Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- PostgreSQL/RabbitMQ through Docker Compose, or equivalent local services

### Run Docker Compose

```bash
docker compose up --build
```

Current compose configuration starts:

- `auth.api` on host ports `8082` and `8083`
- `event.api` with dynamically published host ports for container ports `8080` and `8081`
- `registration.api` on host ports `5151` and `7291`
- `payment.api` on host ports `5193` and `7016`
- `eventdb` PostgreSQL on `localhost:5432`
- `rabbitmq` on `localhost:5672`, management UI on `http://localhost:15672`

Compose uses Docker service names for cross-container calls. For example, `payment.api` calls Registration through `http://registration.api:8080`, and API services connect to PostgreSQL and RabbitMQ through `eventdb` and `rabbitmq`.

Default Docker HTTP endpoints:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:8082` |
| Registration API | `http://localhost:5151` |
| Payment API | `http://localhost:5193` |

`event.api` currently publishes container ports dynamically. Use `docker compose ps` to see the assigned host ports.

### Run Locally With Docker Infrastructure

Start only PostgreSQL and RabbitMQ:

```bash
docker compose up -d eventdb rabbitmq
```

Then run the APIs:

```bash
dotnet run --project Auth/Auth.API
dotnet run --project EventManagement/Event.API
dotnet run --project Registration/Registration.API
dotnet run --project Payment/Payment.API
```

Default local HTTP ports from `launchSettings.json`:

| Service | URL |
| --- | --- |
| Auth API | `http://localhost:5104` |
| Event API | `http://localhost:5145` |
| Registration API | `http://localhost:5151` |
| Payment API | `http://localhost:5193` |

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
    "Password": "guest",
    "EventExchange": "eventbox.events",
    "CommandExchange": "eventbox.commands",
    "QueuePrefix": "eventbox",
    "PrefetchCount": 10
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
  "RegistrationApi": {
    "BaseUrl": "http://localhost:5151"
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

Registration API:

```bash
dotnet ef migrations add <MigrationName> --project Registration/Registration.Infrastructure --startup-project Registration/Registration.API
```

Payment API:

```bash
dotnet ef migrations add <MigrationName> --project Payment/Payment.Infrastructure --startup-project Payment/Payment.API
```

Auth API and Event API apply migrations automatically on startup in current code. Registration API and Payment API currently configure DbContexts but do not apply migrations automatically in `Program.cs`.

## Tests

Run unit tests:

```bash
dotnet test Eventbox.slnx
```

Current unit test coverage focuses on:

- Registration ticket availability behavior
- Registration concurrency for event registration
- Payment callback behavior

## Postman

The `postman/` folder contains:

- `Event.API.postman_collection.json`
- `Eventbox.Docker.postman_environment.json`

Use these as a starting point for manual API testing against the Docker environment.
