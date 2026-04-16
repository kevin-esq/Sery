# Coding Standards

## Purpose

This document defines coding standards for Sery to keep code maintainable, consistent, and safe as the team grows.

## Core Principles

- Prioritize readability over cleverness.
- Keep domain logic explicit and testable.
- Prefer small, focused classes and methods.
- Fail fast on invalid input and configuration.
- Keep architecture boundaries strict.

## Layering and Dependencies

- `Domain`:
  - Contains business entities, value objects, and invariants.
  - Must not depend on frameworks or infrastructure.
- `Application`:
  - Implements use cases and orchestration.
  - Depends on `Domain` only.
- `Infrastructure`:
  - Contains adapters to external systems (DB, Redis, AI providers).
  - References `Application` and `Domain`.
- `API`:
  - Handles HTTP transport concerns (controllers, middleware, contracts).
  - Must not contain domain business rules.

## C# and .NET Conventions

- Use nullable reference types and avoid null-forgiving operator unless justified.
- Use `async/await` end-to-end for I/O operations.
- Prefer explicit access modifiers for all members.
- Keep methods under roughly 30 lines when practical.
- Prefer `record` for immutable DTOs in `Domain` / `Application`.
- In `src/Sery.API/Contracts`, use `class` or `record` with XML docs and examples as needed for OpenAPI.
- **Primary constructors** (team default for DI types):
  - **Middleware**: always use primary constructors.
  - **Application services** registered in DI: use primary constructors when there is no heavy initialization logic.
  - **Controllers**: use primary constructors when the controller has injected dependencies; use `public sealed class Foo() : ControllerBase` when there are no constructor parameters.
  - **Classic constructors** are allowed when validation or multi-step initialization would make a primary constructor harder to read.
- Use `sealed` when inheritance is not intended.

## API Design Rules

- Controllers should be thin and delegate to `Application`.
- Use versioned RESTful routes: `api/v{version}/[controller]` (see [API versioning](../api/API_VERSIONING.md)).
- Return explicit HTTP status codes.
- Standardize errors with `ProblemDetails`.
- Always include correlation id in request/response flow.
- Follow API contract conventions in [API_STANDARDS.md](API_STANDARDS.md).

## Error Handling and Logging

- Handle unexpected errors in global middleware.
- Do not swallow exceptions silently.
- Log with structured templates, not string concatenation.
- Never log secrets, tokens, or sensitive personal data.
- Centralize API error messages and `ProblemDetails` builders to avoid duplicated string literals.

## Configuration and Secrets

- Use strongly typed `Options` with startup validation.
- Keep environment-specific values in `appsettings.{Environment}.json`.
- Secrets must come from secure secret stores or environment variables.
- Never commit credentials to source control.

## Testing Standards

- Every meaningful behavior change requires tests.
- Test pyramid target:
  - Unit tests in `Domain` and `Application`.
  - Integration tests for API endpoints.
- Test names should follow `MethodName_ShouldExpectedBehavior_WhenCondition`.
- Avoid flaky tests; do not rely on timing-based assertions when possible.

## Naming and Project Structure

- Use clear names that reflect business intent.
- Name classes by responsibility (`ChatService`, `CreateConversationHandler`).
- Group files by feature or concern inside each layer.
- Keep one top-level class per file unless tightly related.

## Pull Request Standards

- Keep PRs small and focused.
- Include technical rationale, not only code diffs.
- Update docs when architecture, behavior, or process changes.
- Add or update ADRs for significant architectural decisions.

## Tooling and Quality Gates

- Repository-wide editor rules: `.editorconfig` at the solution root (IDE + `dotnet format` where applicable).
- Required before pushing:
  - `dotnet build Sery.sln`
  - `dotnet test Sery.sln` (includes `Sery.Architecture.Tests` layer and controller layout checks)
- CI must pass before merge into protected branches.
- Swagger docs must be maintained following [SWAGGER.md](SWAGGER.md).
