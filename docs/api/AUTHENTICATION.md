# Authentication

This document describes the current authentication model implemented in the API.

## Overview

- Access model: JWT bearer tokens
- Refresh model: database-backed refresh tokens
- Session model: one row per active device/session in `user_sessions`
- Current endpoints: `api/v1/auth/*`

## Token lifecycle

1. Client calls `register` or `login`.
2. API returns:
   - `accessToken`: short-lived JWT used in `Authorization: Bearer ...`
   - `refreshToken`: long-lived token used only for session refresh/revocation flows
3. Client sends the access token to protected endpoints such as `POST /api/v1/chat/stream`.
4. Client calls `POST /api/v1/auth/refresh` with the refresh token to rotate the session token pair.

## Storage model

- `User` owns many `UserSession` rows.
- `UserSession` stores:
  - `UserId`
  - `RefreshTokenHash`
  - `DeviceInfo`
  - `IpAddress`
  - `CreatedAt`
  - `ExpiresAt`
- The raw refresh token is never stored in the database.

## Security notes

- Access tokens are signed JWTs and include user identity claims.
- Refresh tokens are generated with cryptographic randomness.
- Refresh token persistence uses SHA-256 hashing before the value is stored.
- Session revocation is scoped to the authenticated user.

## Authenticated user context

Protected endpoints do not accept `userId` in the request body anymore. The API resolves identity from JWT claims through `IUserContext`.

Example:

- `POST /api/v1/chat/stream` requires `Authorization: Bearer {accessToken}`
- The server reads `NameIdentifier`, `Email`, and `tenant` from the token claims

## Endpoints

### `POST /api/v1/auth/register`

Registers a user and creates the first session.

### `POST /api/v1/auth/login`

Authenticates a user and creates a new session row.

### `POST /api/v1/auth/refresh`

Rotates the refresh token for the current session and returns a new token pair.

### `POST /api/v1/auth/logout`

Revokes the session represented by the provided refresh token.

### `GET /api/v1/auth/sessions`

Returns active sessions for the authenticated user.

### `DELETE /api/v1/auth/sessions/{id}`

Revokes one specific session belonging to the authenticated user.

### `DELETE /api/v1/auth/sessions/other`

Revokes all sessions for the authenticated user except the one represented by the current refresh token.

### `GET /api/v1/auth/me`

Returns the authenticated user context extracted from the access token.

## Current limitations

- No device fingerprint mismatch detection yet
- No refresh token reuse detection yet
- No HttpOnly cookie transport for refresh tokens yet
- No rate limiting dedicated to auth endpoints yet
