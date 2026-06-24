# Eventbox API

Eventbox API is an event ticketing backend built with .NET 10, PostgreSQL, RabbitMQ, and an event-driven microservice layout.

## Overview

Eventbox covers the core flow for organizers publishing events and customers registering for tickets:

- Organizer accounts create organizations, events, ticket types, and ticket capacity.
- Public users discover published events and check ticket availability.
- Customers or guests create orders for an event.
- Orders reserve ticket availability, can be paid, cancelled, confirmed for free, or expired automatically.
- Payment simulation confirms or fails paid orders through RabbitMQ integration events.
- Notification consumes order confirmation events and sends email through SMTP.
- Organizers check attendees in by QR token at the door.

## Services

| Service | Project | Responsibility |
| --- | --- | --- |
| YarpApiGateway | `ApiGateway/YarpApiGateway` | YARP reverse proxy for the public API surface. |
| Auth API | `Auth/Auth.API` | Customer/organizer registration, login, logout, JWT issuance, and current-user endpoint. |
| Event API | `EventManagement/Event.API` | Organizations, organizer event management, public event discovery, ticket type management, and event outbox publishing. |
| Ticketing API | `Ticketing/Ticketing.API` | Public checkout, customer orders, internal payment start endpoint, ticket availability, order expiration jobs, and QR check-in. |
| Payment API | `Payment/Payment.API` | Payment intent creation, order access verification against Ticketing, and simulated payment provider callbacks. |
| Notification API | `Notification/Notification.API` | RabbitMQ subscriber that sends transactional emails through SMTP/MailHog. |
| EventBus | `EventBus/EventBus.Core`, `EventBus/EventBus.RabbitMQ` | Shared abstractions and RabbitMQ implementation for integration events and serialization. |
| Contracts | `Shared/Eventbox.Contracts` | Shared integration event contracts across services. |
| Shared | `Shared/Eventbox.Shared`, `Shared/Eventbox.Shared.Infrastructure` | Cross-cutting concerns: seedwork, exceptions, auditing, logging, repositories, and outbox support. |

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core with PostgreSQL
- ASP.NET Core Identity for Auth
- JWT Bearer authentication and account-type authorization policies
- YARP reverse proxy for the public API gateway
- RabbitMQ for integration events
- Transactional outbox pattern for reliable event publishing
- MediatR for Ticketing and Payment application flows
- Hangfire with PostgreSQL storage for outbox and order expiration jobs
- Mapster for object mapping
- Serilog for structured request and application logging
- MailKit/QRCoder in Notification
- xUnit v3 unit tests
- Docker Compose for local infrastructure and API services

## Solution Structure

```text
Eventbox.slnx
├── ApiGateway/
│   └── YarpApiGateway/
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
├── Notification/
│   └── Notification.API/
├── EventBus/
│   ├── EventBus.Core/
│   └── EventBus.RabbitMQ/
├── Shared/
│   ├── Eventbox.Contracts/
│   ├── Eventbox.Shared/
│   └── Eventbox.Shared.Infrastructure/
├── Tests/
├── postman/
└── specs/
```

## Main API Surface

### YarpApiGateway

Base URL in Docker Compose: `http://localhost:8050`

| External route | Proxied route | Target service |
| --- | --- | --- |
| `/auth/{**catch-all}` | `/api/auth/{**catch-all}` | Auth API |
| `/events/{**catch-all}` | `/api/events/{**catch-all}` | Event API |
| `/organizations/{**catch-all}` | `/api/organizations/{**catch-all}` | Event API |
| `/ticketing/public/{**catch-all}` | `/api/public/{**catch-all}` | Ticketing API |
| `/ticketing/customer/{**catch-all}` | `/api/customer/{**catch-all}` | Ticketing API |
| `/ticketing/events/{**catch-all}` | `/api/events/{**catch-all}` | Ticketing API |
| `/payments/{**catch-all}` | `/api/payments/{**catch-all}` | Payment API |

The gateway also exposes `GET /health`.

### Auth API

Direct local base URL: `http://localhost:8000`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/customers/register` | Public | Register a customer account and return a JWT. |
| `POST` | `/api/auth/organizers/register` | Public | Register an organizer account and return a JWT. |
| `POST` | `/api/auth/customers/login` | Public | Login a customer by email/password and return a JWT. |
| `POST` | `/api/auth/organizers/login` | Public | Login an organizer by email/password and return a JWT. |
| `GET` | `/api/auth/me` | Bearer token | Return current user id, email, and account type. |
| `POST` | `/api/auth/logout` | Bearer token | Stateless logout endpoint; returns no content. |

### Event API

Direct local base URL: `http://localhost:8010`

Public routes:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/public/events` | Search/list published events. |
| `GET` | `/api/public/events/{slug}` | Get detail for a published event; supports optional `accessCode` query. |

Organizer routes require a JWT with `account_type = Organizer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/organizations` | Create an organization. |
| `GET` | `/api/organizations/me` | Get organizations for the current organizer. |
| `GET` | `/api/organizations/{id}` | Get organization detail. |
| `PUT` | `/api/organizations/{id}` | Update an organization. |
| `DELETE` | `/api/organizations/{id}` | Delete an organization. |
<<<<<<< HEAD
| `POST` | `/api/organizations/{id}/organizers?organizerId={organizerId}` | Add an organizer account to an organization. |
=======
| `POST` | `/api/organizations/{id}/organizers` | Add an organizer account to an organization. |
| `POST` | `/api/organizations/{organizationId}/events` | Create a draft event. |
| `PUT` | `/api/organizations/{organizationId}/events/{id}` | Update an event. |
| `GET` | `/api/organizations/{organizationId}/events/{id}/publish-readiness` | Check whether an event can be published. |
| `POST` | `/api/organizations/{organizationId}/events/{id}/publish` | Publish an event. |
| `POST` | `/api/organizations/{organizationId}/events/{id}/unpublish` | Unpublish an event. |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | List ticket types for an event. |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types` | Create a ticket type. |
| `PUT` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}` | Update a ticket type. |
| `POST` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/capacity` | Add capacity to a ticket type. |
| `GET` | `/api/organizations/{organizationId}/events/{eventId}/ticket-types/{ticketTypeId}/availability` | Get ticket type availability. |

### Ticketing API

Direct local base URL: `http://localhost:8020`

Public routes:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/public/events/{eventId}/ticket-availability` | Get ticket availability by event id. |
<<<<<<< HEAD
| `POST` | `/api/public/events/{eventId}/orders` | Create an order/reservation. Supports guest checkout when email is supplied. |
=======
| `POST` | `/api/public/events/{eventId}/orders` | Create an order/reservation. Authenticated customers are linked by token; guests must supply email. |
>>>>>>> d8007297ae23d4c4a6801e92b766908f565b99e5
| `GET` | `/api/public/self-service/orders?token={token}` | Get an order by self-service token. |

Customer routes require a JWT with `account_type = Customer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/customer/orders` | Get orders for the current customer email. |
| `GET` | `/api/customer/orders/by-email?email={email}` | Get orders by the authenticated customer's email. |
| `GET` | `/api/customer/orders/{orderId}` | Get order detail if owned by current customer. |
| `POST` | `/api/customer/orders/{orderId}/confirm-free` | Confirm a free order. |
| `POST` | `/api/customer/orders/{orderId}/cancel` | Cancel an order. |

Organizer routes require a JWT with `account_type = Organizer`:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/events/{eventId}/check-ins` | Check in an attendee by QR token. |

Internal routes require the `X-Internal-Service-Token` header:

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/internal/orders/{orderId}/start-payment` | Mark an order as payment-started before Payment creates an intent. |

Ticketing also hosts the Hangfire dashboard at `/hangfire`.

### Payment API

Direct local base URL: `http://localhost:8030`

| Method | Route | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/payments/intents` | Bearer token or order access code | Start payment in Ticketing and create/reuse a payment intent. Supports `Idempotency-Key`. |
| `POST` | `/api/payments/simulated-callbacks` | `X-Provider-Signature` header | Simulate a provider callback for `Succeeded` or `Failed` payment status. |

### Notification API

Direct local base URL in Docker Compose: `http://localhost:8040`

Notification currently runs as a RabbitMQ subscriber. It consumes order confirmation events, renders email templates, generates QR content, and sends email through SMTP. In local Docker Compose it uses MailHog at `smtp://mailhog:1025`; the MailHog UI is available at `http://localhost:8025`.

## Event Flow

1. Organizer registers through Auth API and receives a JWT with `account_type = Organizer`.
2. Organizer creates an organization, creates an event, creates ticket types, adds capacity, and publishes the event.
3. Event API publishes integration events via the outbox pattern for event creation/publication and ticket inventory changes.
4. Ticketing consumes those events and maintains event snapshots plus ticket type availability.
5. Customer or guest creates an order through Ticketing API.
6. Ticketing reserves seats, creates a self-service token, schedules order expiration, and exposes order/customer APIs.
7. Payment API verifies order access with Ticketing API, calls the internal start-payment endpoint, creates a payment intent, and handles simulated provider callbacks.
8. Successful payment publishes `PaymentConfirmedIntegrationEvent`; failed payment publishes `PaymentFailedIntegrationEvent`; Ticketing updates the order accordingly.
9. Ticketing reconciles expired orders through Hangfire.
10. Notification consumes confirmed-order events and sends email.
11. Organizer checks in attendees by scanning QR tokens through the check-in endpoint.

## Local Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run Docker Compose

```bash
docker compose up --build
```

Current local Compose configuration starts:

| Service | Host URL or port |
| --- | --- |
| YarpApiGateway | `http://localhost:8050` |
| Auth API | `http://localhost:8000` and `https://localhost:8001` |
| Event API | `http://localhost:8010` and `https://localhost:8011` |
| Ticketing API | `http://localhost:8020` and `https://localhost:8021` |
| Payment API | `http://localhost:8030` and `https://localhost:8031` |
| Notification API | `http://localhost:8040` |
| PostgreSQL | `localhost:5432` |
| RabbitMQ | `localhost:5672`, management UI `http://localhost:15672` |
| MailHog | SMTP `localhost:1025`, UI `http://localhost:8025` |

Compose uses Docker service names for cross-container calls. For example, `payment.api` calls Ticketing through `http://ticketing.api:8080`, and API services connect to PostgreSQL and RabbitMQ through `eventdb` and `rabbitmq`.

PostgreSQL initializes `EventDb` from `POSTGRES_DB`; `AuthDb`, `TicketingDb`, and `PaymentDb` are created by `docker/postgres/initdb/01-create-service-databases.sql` on first volume initialization.

### AWS EC2 Staging With Docker Compose

Use the staging Compose override instead of the local `docker-compose.override.yml` file. Copy the example environment file on the EC2 host and replace every placeholder with staging secrets:

```bash
cp .env.staging.example .env.staging
```

Start the stack with explicit Compose files so Docker does not automatically merge the local development override:

```bash
docker compose --env-file .env.staging -f docker-compose.yml -f docker-compose.staging.yml up -d --build
```

The staging override:

- runs APIs with `ASPNETCORE_ENVIRONMENT=Staging`
- serves HTTP inside containers on port `8080`
- uses Docker service names for PostgreSQL, RabbitMQ, and internal API calls
- needs `TicketingApi__InternalServiceToken` on Payment API and `InternalApi__ServiceToken` on Ticketing API to share the same secret for payment-start calls
- stores PostgreSQL and RabbitMQ data in named Docker volumes
- does not publish PostgreSQL or RabbitMQ ports to the EC2 host
- creates `AuthDb`, `TicketingDb`, and `PaymentDb` on first PostgreSQL volume initialization; `EventDb` is created by `POSTGRES_DB`

For EC2, keep the instance security group limited to the API or reverse-proxy ports you intend to expose. If you terminate HTTPS at an ALB, Nginx, or Caddy, forward traffic to the API HTTP ports and keep database and RabbitMQ ports private.

### AWS EC2 Staging With Docker Compose

Use the staging Compose override instead of the local `docker-compose.override.yml` file. Copy the example environment file on the EC2 host and replace every placeholder with staging secrets:

```bash
cp .env.staging.example .env.staging
```

Start the stack with explicit Compose files so Docker does not automatically merge the local development override:

```bash
docker compose --env-file .env.staging -f docker-compose.yml -f docker-compose.staging.yml up -d --build
```

The staging override:

- runs APIs with `ASPNETCORE_ENVIRONMENT=Staging`
- serves HTTP only inside the containers on port `8080`
- uses Docker service names for PostgreSQL, RabbitMQ, and internal API calls
- stores PostgreSQL and RabbitMQ data in named Docker volumes
- does not publish PostgreSQL or RabbitMQ ports to the EC2 host
- creates `AuthDb`, `TicketingDb`, and `PaymentDb` on first PostgreSQL volume initialization; `EventDb` is created by `POSTGRES_DB`

For EC2, keep the instance security group limited to the API or reverse-proxy ports you intend to expose. If you terminate HTTPS at an ALB, Nginx, or Caddy, forward traffic to the API HTTP ports and keep database and RabbitMQ ports private.

### Run Locally With Docker Infrastructure

Start only infrastructure services:

```bash
docker compose up -d eventdb rabbitmq mailhog
```

Then run the APIs:

```bash
dotnet run --project ApiGateway/YarpApiGateway
dotnet run --project Auth/Auth.API
dotnet run --project EventManagement/Event.API
dotnet run --project Ticketing/Ticketing.API
dotnet run --project Payment/Payment.API
dotnet run --project Notification/Notification.API
```

Default project launch URLs:

| Service | URL |
| --- | --- |
| YarpApiGateway | `http://localhost:53884` |
| Auth API | `http://localhost:8000` |
| Event API | `http://localhost:8010` |
| Ticketing API | `http://localhost:8020` |
| Payment API | `http://localhost:8030` |
| Notification API | `http://localhost:50062` |

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

Ticketing internal API settings:

```json
{
  "InternalApi": {
    "ServiceToken": "dev-internal-service-token"
  }
}
```

Payment API settings:

```json
{
  "TicketingApi": {
    "BaseUrl": "http://localhost:8020",
    "InternalServiceToken": "dev-internal-service-token"
  },
  "Payment": {
    "ProviderSignature": "your-development-signature"
  }
}
```

Notification SMTP settings:

```json
{
  "Smtp": {
    "Host": "localhost",
    "Port": 1025,
    "FromAddress": "noreply@eventbox.com",
    "FromName": "Eventbox"
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
dotnet ef migrations add <MigrationName> --project EventManagement/Event.API --output-dir Infrastructure/Migrations
```

Ticketing API:

```bash
dotnet ef migrations add <MigrationName> --project Ticketing/Ticketing.Infrastructure --startup-project Ticketing/Ticketing.API
```

Payment API:

```bash
dotnet ef migrations add <MigrationName> --project Payment/Payment.Infrastructure --startup-project Payment/Payment.API
```

All database-backed services apply migrations automatically on startup.

## Tests

Run unit tests:

```bash
dotnet test Eventbox.slnx
```

Current unit test coverage focuses on:

- Ticketing ticket availability and inventory integration events
- Ticketing registration, order state, expiration, issuing, and concurrency behavior
- Ticketing QR token check-in behavior
- Payment callback behavior
- Event Management domain behavior

## Postman

<<<<<<< HEAD
The v2 API contract is tracked in `specs/event-ticketing-system/16-api-contract.md`. Product features such as attendee list APIs, manual attendee lookup/check-in, order lookup email requests, reporting/export, notification settings, and richer team roles are v3 backlog unless code is added.
=======
The API contract is tracked in `specs/event-ticketing-system/16-api-contract.md`. Product features such as attendee list APIs, manual attendee lookup/check-in, order lookup email requests, reporting/export, notification settings APIs, and richer team roles are backlog unless code is added.
>>>>>>> d8007297ae23d4c4a6801e92b766908f565b99e5

The `postman/` folder contains:

- `Event.API.postman_collection.json`
- `Eventbox.Docker.postman_environment.json`
- `Eventbox.Staging.postman_environment.json`

Import the collection and select the **Eventbox Docker** environment to test against the Docker Compose setup.
