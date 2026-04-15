# Contoso Conference

A conference management system built with .NET 10, following a microservices and event-driven architecture.

## Services

- **ConferenceManagement/Conference.API** — REST API for managing conferences and seat types (SQL Server)
- **Registration/Registration.API** — Handles orders and seat availability (PostgreSQL, CQRS with MediatR)

Services communicate asynchronously via RabbitMQ integration events.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (code-first) with SQL Server and PostgreSQL
- RabbitMQ (via `EventBus/EventBus.RabbitMQ`)
- MediatR (CQRS in Registration service)
- Mapster (object mapping)
- Docker

## Project Structure

```
contoso-conference/
├── ConferenceManagement/
│   └── Conference.API/           # Conferences and seat types REST API
├── Registration/
│   ├── Registration.API/         # Orders and seat availability minimal APIs
│   ├── Registration.Application/ # Application layer (MediatR handlers)
│   ├── Registration.Domain/      # Domain entities and repository interfaces
│   └── Registration.Infrastructure/ # EF Core DbContext and repositories
├── EventBus/
│   ├── EventBus/                 # Abstractions (IEventBus, IIntegrationEvent)
│   └── EventBus.RabbitMQ/       # RabbitMQ implementation
├── docker-compose.yml
└── docker-compose.override.yml
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop

### Run with Docker

```bash
docker compose up
```

This starts:
- `conference.api` — Conference Management API on port 8080
- `registration.api` — Registration API
- `conferencedb` — SQL Server on port 1433
- `rabbitmq` — RabbitMQ on port 5672 (management UI on port 15672)

EF Core migrations run automatically on startup for both services.

### Run locally (without Docker)

Start the infrastructure first:

```bash
docker compose up conferencedb rabbitmq -d
```

Then run each service:

```bash
dotnet run --project ConferenceManagement/Conference.API
dotnet run --project Registration/Registration.API
```

### Adding EF Core migrations

For Conference.API:

```bash
dotnet ef migrations add <MigrationName> --project ConferenceManagement/Conference.API --output-dir Infrastructure/Migrations
```

For Registration:

```bash
dotnet ef migrations add <MigrationName> --project Registration/Registration.Infrastructure --startup-project Registration/Registration.API
```

## Configuration

Update `appsettings.Development.json` in each service if using a non-Docker database:

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=ConferenceDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
  }
}
```
