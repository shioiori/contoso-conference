# Contoso Conference

A conference management system built with .NET 10.

## Projects

- **ConferenceManagement/Conference.API** — REST API for managing conferences
- **Common/ServiceBus** — Shared service bus integration library

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core with SQL Server
- Mapster (object mapping)
- Docker support

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or Docker)

### Run locally

```bash
dotnet run --project ConferenceManagement/Conference.API
```

### Run with Docker

```bash
docker compose up
```

### Database migrations

```bash
dotnet ef database update --project ConferenceManagement/Conference.API
```

## Configuration

Update `appsettings.Development.json` with your local connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ContosoConference;Trusted_Connection=True;"
  }
}
```
