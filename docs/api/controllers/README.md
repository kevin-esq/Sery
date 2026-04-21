# Controllers

Per-controller reference pages live in this folder. Global API rules are in [../API_VERSIONING.md](../API_VERSIONING.md) and [../../standards/API_STANDARDS.md](../../standards/API_STANDARDS.md).

## Index

- [AuthController](AuthController.md)
- [ChatController](ChatController.md)
- [HealthController](HealthController.md)

## Conventions (`src/Sery.API/Controllers`)

- Place controllers in **`Controllers/V{n}/`** with namespace **`Sery.API.Controllers.V{n}`** (see [API versioning](../API_VERSIONING.md)).
- Business routes use versioned prefix: `api/v{version}/[controller]`.
- Operational endpoints may be version-neutral when documented explicitly.
- Controllers only handle HTTP transport; business logic belongs in `Application`.
- Return explicit status codes and typed contracts from `src/Sery.API/Contracts`.
- Keep actions small; one use case per action when possible.

## Planned controllers

- `UserProfileController`
- `ConversationController`
- `SubscriptionController` (Phase 2)
