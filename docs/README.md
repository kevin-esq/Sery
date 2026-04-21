# Sery documentation

Start here for technical documentation. Paths are relative to the repository root.

Editor and formatting rules for the whole repo: [.editorconfig](../.editorconfig) (repository root).

## Map

| Area                         | Location                                                       |
| ---------------------------- | -------------------------------------------------------------- |
| Architecture                 | [architecture/ARCHITECTURE.md](architecture/ARCHITECTURE.md)   |
| Coding standards             | [standards/CODING_STANDARDS.md](standards/CODING_STANDARDS.md) |
| API standards                | [standards/API_STANDARDS.md](standards/API_STANDARDS.md)       |
| Swagger / OpenAPI            | [standards/SWAGGER.md](standards/SWAGGER.md)                   |
| Errors                       | [standards/ERROR_HANDLING.md](standards/ERROR_HANDLING.md)     |
| Localization                 | [standards/LOCALIZATION.md](standards/LOCALIZATION.md)         |
| Observability                | [standards/OBSERVABILITY.md](standards/OBSERVABILITY.md)       |
| API versioning               | [api/API_VERSIONING.md](api/API_VERSIONING.md)                 |
| Authentication               | [api/AUTHENTICATION.md](api/AUTHENTICATION.md)                 |
| Controller reference         | [api/controllers/README.md](api/controllers/README.md)         |
| Repository setup             | [process/REPOSITORY_SETUP.md](process/REPOSITORY_SETUP.md)     |
| Architecture decisions (ADR) | [adr/README.md](adr/README.md)                                 |

## Source code layout (backend)

| Path                      | Role                                                                  |
| ------------------------- | --------------------------------------------------------------------- |
| `src/Sery.Domain`         | Entities and domain rules                                             |
| `src/Sery.Application`    | Use cases and application services                                    |
| `src/Sery.Infrastructure` | Adapters (DB, cache, external APIs)                                   |
| `src/Sery.API`            | HTTP API, middleware, OpenAPI, contracts                              |
| `tests/*`                 | Automated tests (including `Sery.Architecture.Tests` for layer rules) |

### `src/Sery.API` (HTTP layer)

| Path                                      | Role                                                                                              |
| ----------------------------------------- | ------------------------------------------------------------------------------------------------- |
| `Controllers/V1/`, `Controllers/V2/`, ... | One folder + namespace per API major version (`Sery.API.Controllers.V1`); delegate to Application |
| `Contracts/`                              | Request/response DTOs exposed by the API                                                          |
| `Common/`                                 | Shared API helpers (e.g. error catalog, ProblemDetails factory)                                   |
| `Configuration/`                          | Strongly typed options                                                                            |
| `Extensions/`                             | `Program` composition helpers                                                                     |
| `Middleware/`                             | Cross-cutting HTTP behavior                                                                       |
| `Swagger/`                                | OpenAPI / Swashbuckle configuration (e.g. per-version documents)                                  |
| `Properties/`                             | Launch profiles                                                                                   |
