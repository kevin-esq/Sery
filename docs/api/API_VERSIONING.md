# API Versioning Strategy

## How to verify versioning

Versioning is **active in code**, not only in documentation:

- Every HTTP route is under **`/api/v1/...`** today (segment `v1` comes from `[ApiVersion("1.0")]` and `UrlSegmentApiVersionReader`).
- Examples:
  - `GET /api/v1/health`
  - `POST /api/v1/chat/message`
- In **Development**, Swagger UI lists one OpenAPI document per version (e.g. **Sery API V1**) and serves JSON at `/swagger/v1/swagger.json`. Implementation: `ConfigureSwaggerOptions` + `IApiVersionDescriptionProvider`.

## Physical layout (regression and reviews)

- Controllers live under **`src/Sery.API/Controllers/V{n}/`** (e.g. `V1/`, `V2/`).
- Namespace matches folder: **`Sery.API.Controllers.V1`**, **`Sery.API.Controllers.V2`**, etc.
- Adding `V2` means a new folder, new namespace, and new `[ApiVersion("2.0")]` types; keep `V1` until clients migrate. Automated check: `Sery.Architecture.Tests.ApiControllerOrganizationTests`.

## Versioning Model

- Style: URL segment versioning.
- Current stable version: `v1`.
- Route pattern: `api/v{version}/[controller]`.
- Operational endpoints may be version-neutral when they are not part of the business contract.
  - Current example: `GET /api/health`
  - Compatibility route kept: `GET /api/v1/health`

## Policy

- New breaking changes require a new major API version (`v2`, `v3`, ...).
- Non-breaking additive changes can be released within current version.
- Controllers must declare supported version with `[ApiVersion("1.0")]`.
- Do not rely on implicit defaulting for URL-segment versioning; clients must request the version explicitly on versioned routes.

## Swagger

- Swagger endpoint for current version:
  - `/swagger/v1/swagger.json`
- Swagger UI label:
  - `Sery API v1`

## Migration Guidance

- Keep at least one previous major version during migration windows.
- Publish deprecation notice and end-of-support date before removing versions.
