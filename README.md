# Sery

Sery is an AI-powered emotional companion focused on meaningful interaction, personal growth, and healthy usage boundaries.

## Tech Stack

- Backend: .NET 8, ASP.NET Core Web API
- Data: PostgreSQL, Redis
- Frontend (planned): Next.js
- Testing: xUnit

## Solution Structure

- `src/Sery.Domain`: Core business entities and rules.
- `src/Sery.Application`: Use cases and application services.
- `src/Sery.Infrastructure`: External integrations and infrastructure adapters.
- `src/Sery.API`: HTTP API and controllers.
- `tests/*`: Unit and integration tests.
- `docs`: Architecture docs and ADRs.

## Getting Started

### Prerequisites

- .NET SDK 8.0.x

### Restore, Build and Test

```bash
dotnet restore
dotnet build Sery.sln
dotnet test Sery.sln
```

### Run API

```bash
dotnet run --project src/Sery.API/Sery.API.csproj
```

Default Swagger endpoint:

- `https://localhost:5001/swagger` (or the port reported in terminal)

## Docker

Build and run:

```bash
docker build -t sery-api .
docker run --rm -p 8080:8080 sery-api
```

## Branching Model (initial)

- `main`: protected, production-ready.
- `develop`: integration branch for active development.
- `feature/*`: feature branches.
- `fix/*`: bugfix branches.

## Documentation

- Architecture: `docs/ARCHITECTURE.md`
- ADR index: `docs/adr/README.md`
- Product specification: `Sery_Product_Specification.md`
