# API Controllers Convention

## Purpose

Define standards for ASP.NET Core controllers in `src/Sery.API/Controllers`.

## Rules

- Route prefix: `api/[controller]`.
- Controllers only handle transport concerns (HTTP input/output).
- Business logic must live in `Application` layer.
- Return explicit status codes and contracts.
- Keep actions small and focused.

## Initial Controllers

- `HealthController`: liveness/health endpoint.
- `ChatController`: initial message endpoint contract for chat flow.

## Next Controllers (planned)

- `AuthController`
- `UserProfileController`
- `ConversationController`
- `SubscriptionController` (Phase 2)
