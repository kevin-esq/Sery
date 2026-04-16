# API Standards

## Design

- Controllers are transport-only and delegate to `Application`.
- Implement controllers under **`src/Sery.API/Controllers/V{n}/`** with namespace **`Sery.API.Controllers.V{n}`** (see [API versioning](../api/API_VERSIONING.md)).
- Keep actions focused on one use case.
- Use DTOs/records for request and response contracts.

## Routing and Status Codes

- Prefix with `api/[controller]`.
- Version routes via URL segment (`api/v{version}/[controller]`).
- Use semantic status codes:
  - `200` for successful reads.
  - `202` for accepted async/queued operations.
  - `400` for validation failures.
  - `401/403` for auth and authorization failures.
  - `500` only for unexpected failures.

## Error Contract

- Return errors with `ProblemDetails`.
- Include correlation id in response headers.
- Never include sensitive data in error payloads.
- Include `code` (internal code) and `messageKey` (localization key) in error payload extensions.

## Documentation

- Follow [SWAGGER.md](SWAGGER.md).
- XML comments are mandatory for public endpoint actions.
- Maintain per-controller docs under [api/controllers/](../api/controllers/).
- Keep error contract documentation aligned with [ERROR_HANDLING.md](ERROR_HANDLING.md).
