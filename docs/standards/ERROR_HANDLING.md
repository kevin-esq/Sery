# Error Handling Standards

## Goal

Provide consistent and predictable error responses across all Sery API endpoints.

## Contract

- Error responses must use `ProblemDetails` (`application/problem+json`).
- A correlation id must be present in `X-Correlation-ID` response header.
- Sensitive data must never be returned in error payloads.
- Every error payload must include:
  - `code` (internal error code)
  - `messageKey` (localization key; see [LOCALIZATION.md](LOCALIZATION.md))
- `title` and `detail` are **localized** when a matching entry exists in `Resources/ApiMessages*.resx` and the client sends a supported `Accept-Language`.

## Centralization

- Canonical error metadata lives in `src/Sery.API/Common/ErrorCatalog.cs` (`code`, `messageKey`, default message).
- `ProblemDetails` construction is centralized in `src/Sery.API/Common/ApiProblemDetailsFactory.cs`.
- Unhandled exceptions are mapped by `GlobalExceptionMiddleware`.

## Error Catalog

- `SERY-API-400-001` / `error.validation.failed`
- `SERY-API-400-002` / `error.chat.required_userid_message`
- `SERY-API-500-001` / `error.unexpected`

## Current Outcomes

- `400 Bad Request`
  - Title: `Validation failed.`
  - Includes `code` and `messageKey`.
- `500 Internal Server Error`
  - Title: `An unexpected error occurred.`
  - Includes `code` and `messageKey`.
  - Detail is included only in development (controlled by `ApiBehavior:IncludeExceptionDetails`).
