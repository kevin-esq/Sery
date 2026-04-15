# Sery Architecture

## Overview

Sery follows a layered architecture using a modular monolith approach for MVP speed and maintainability.

## Layers

### Domain (`src/Sery.Domain`)

- Core business concepts and rules.
- No dependencies on other layers.

### Application (`src/Sery.Application`)

- Use cases and orchestration.
- Depends on `Domain` only.

### Infrastructure (`src/Sery.Infrastructure`)

- External integrations (database, cache, AI providers, payments).
- Implements ports/abstractions defined in `Application`.

### API (`src/Sery.API`)

- HTTP transport, controllers, validation, auth boundary.
- Composition root for dependency injection.

## Testing Strategy

- `tests/Sery.Domain.Tests`: domain unit tests.
- `tests/Sery.Application.Tests`: application behavior/unit tests.
- `tests/Sery.API.IntegrationTests`: API integration endpoints.

## Dependency Rules

- `Domain` -> nothing
- `Application` -> `Domain`
- `Infrastructure` -> `Application`, `Domain`
- `API` -> `Application`, `Infrastructure`

## Future Evolution

- Keep modular monolith while validating product.
- Extract services by bounded context only when needed (scale/team/cost reasons).
- Record architecture decisions in ADRs under `docs/adr`.
