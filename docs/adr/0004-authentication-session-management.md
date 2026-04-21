# 0004 - JWT Authentication with Multi-Session Refresh Tokens

- Status: Accepted
- Date: 2026-04-21
- Deciders: Sery Backend Team

## Context

The API needed authenticated user context for chat and account features, support for multiple active devices per user, and safer refresh token storage. The previous direction of keeping refresh token state directly on `User` did not support concurrent sessions and would have coupled transport concerns too closely to persistence.

The team also wanted the implementation to stay aligned with the repository architecture rules:

- API controllers remain transport-only
- application logic lives in `src/Sery.Application`
- infrastructure owns hashing, JWT generation, and EF Core persistence

## Decision

- Use short-lived JWT access tokens for authenticated API access.
- Use refresh tokens for session renewal, with one persisted row per active device/session in `user_sessions`.
- Store only a SHA-256 hash of the refresh token in the database.
- Model sessions explicitly in the domain with `UserSession` and a one-to-many relationship from `User`.
- Implement authentication use cases in `Sery.Application.Auth` behind `IAuthService` and `IAuthPersistence`.
- Keep `AuthController` thin and versioned under `src/Sery.API/Controllers/V1/`.
- Expose session-management endpoints:
  - `GET /api/v1/auth/sessions`
  - `DELETE /api/v1/auth/sessions/{id}`
  - `DELETE /api/v1/auth/sessions/other`
- Resolve authenticated user identity from JWT claims through `IUserContext`.
- Standardize auth error responses through `ProblemDetails` using internal codes and localization keys.

## Consequences

### Positive

- Users can stay signed in on multiple devices without evicting earlier sessions.
- A database leak does not expose usable refresh tokens because only token hashes are stored.
- Authentication behavior is isolated in the application layer and easier to test.
- Session revocation is explicit and auditable at the row level.
- Chat and future protected endpoints can rely on a shared authenticated user context instead of caller-provided `userId` payloads.

### Negative / Trade-offs

- Authentication now requires more moving parts than a single token column on `users`.
- Revoking "all other sessions" depends on the caller providing the current refresh token.
- Session metadata is intentionally lightweight for now (`DeviceInfo`, `IpAddress`) and does not yet implement stronger device fingerprinting or token reuse detection.

## Alternatives Considered

1. **Single refresh token stored on `users`** - rejected because it prevents multi-device auth and makes session revocation too coarse.
2. **Store refresh tokens in plain text** - rejected because a database leak would immediately expose impersonation tokens.
3. **Keep auth orchestration in the API controller** - rejected because it violates the repository rule that controllers must remain transport-only.
4. **Cookie-only session auth for the current API** - deferred; JWT bearer auth fits the current frontend/API separation better, while HttpOnly refresh cookies may be considered later.
