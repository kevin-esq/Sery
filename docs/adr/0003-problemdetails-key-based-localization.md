# 0003 - ProblemDetails key-based localization

- Status: Accepted
- Date: 2026-04-15
- Deciders: Sery Backend Team

## Context

Clients need stable `code` / `messageKey` for logic while humans need readable text in their language. The team wanted key-based resources without changing the JSON contract.

## Decision

- Store user-facing strings in **`ApiMessages.resx`** / **`ApiMessages.{culture}.resx`** with keys equal to **`messageKey`** values from `ErrorCatalog`.
- Resolve text with **`ResourceManager`** and **`CultureInfo.CurrentUICulture`**, configured via **`RequestLocalization`** (`Accept-Language` prioritized).
- Implement **`IApiProblemDetailsFactory`** as the single place that builds `ProblemDetails` with localized `title` / `detail`.

## Consequences

### Positive

- Contract remains stable for automation; UX improves per locale.
- Adding a language is adding a satellite `.resx` file.

### Negative / Trade-offs

- Strings are maintained in `.resx` (merge conflicts possible); consider tooling or CI checks for missing keys later.

## Alternatives Considered

1. **Only English `detail` + client-side translation from `messageKey`** - rejected for MVP API clarity and support tooling.
2. **IStringLocalizer-only without ResourceManager** - rejected after manifest resolution issues with the current folder layout; `ResourceManager` with explicit base name is deterministic.
