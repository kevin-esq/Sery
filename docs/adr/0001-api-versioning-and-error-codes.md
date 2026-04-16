# 0001 - API Versioning and Internal Error Codes

- Status: Accepted
- Date: 2026-04-15
- Deciders: Sery Backend Team

## Context

The API required a stable contract evolution strategy and machine-readable error metadata to support client integrations and future localization.

## Decision

- Use URL segment API versioning with `v1` as current version.
- Define a centralized error catalog with:
  - internal error code (example: `SERY-API-400-002`)
  - localization key (example: `error.chat.required_userid_message`)
  - default fallback message
- Attach `code` and `messageKey` to `ProblemDetails` extensions.

## Consequences

### Positive

- Breaking changes can be managed by explicit version boundaries.
- Client applications can rely on stable error codes.
- Localization can be introduced without changing API contracts.

### Negative / Trade-offs

- Slightly more boilerplate for each error.
- Versioned routes increase route verbosity.

## Alternatives Considered

1. Header-based versioning - rejected to keep routing explicit and simple for MVP.
2. Free-form string errors - rejected due to poor client-side reliability and i18n readiness.
