# AuthController

## Purpose

Provides authentication endpoints for registration, login, token refresh, logout, and multi-device session management.

Path: `src/Sery.API/Controllers/V1/AuthController.cs`  
Namespace: `Sery.API.Controllers.V1`

## Routes

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/sessions`
- `DELETE /api/v1/auth/sessions/{id}`
- `DELETE /api/v1/auth/sessions/other`
- `GET /api/v1/auth/me`

## Notes

- Refresh tokens are stored hashed in the database.
- Active sessions are persisted in `user_sessions`.
- Session revocation is scoped to the authenticated user.
- Protected endpoints identify the user from JWT claims, not request payloads.

## Contracts

- `RegisterRequest`, `LoginRequest`
- `RefreshRequest`, `LogoutRequest`
- `RevokeOtherSessionsRequest`
- `AuthResponse`
- `SessionResponse`
- `MeResponse`

## Error outcomes

- `401 Unauthorized`
  - Invalid credentials
  - Invalid or expired refresh token
  - Missing authenticated user context on protected endpoints
- `409 Conflict`
  - User already exists

## Example register/login response

```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "3Cw5m0S3..."
}
```

## Example session item

```json
{
  "id": "1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
  "deviceInfo": "Mozilla/5.0 ...",
  "ipAddress": "203.0.113.10",
  "createdAt": "2026-04-21T20:56:52Z",
  "expiresAt": "2026-04-28T20:56:52Z"
}
```
