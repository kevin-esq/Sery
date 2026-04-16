# 0002 - EditorConfig, Architecture Tests, and Versioned Controller Folders

- Status: Accepted
- Date: 2026-04-15
- Deciders: Sery Backend Team

## Context

The team needed consistent formatting, automated enforcement of clean architecture boundaries, and a clear physical structure for API versions to support regression testing and parallel `v1` / `v2` maintenance.

## Decision

1. **`.editorconfig`** at repository root for shared C# / .NET style (indentation, file-scoped namespaces, import order, baseline analyzer severities).
2. **`tests/Sery.Architecture.Tests`** using **NetArchTest.Rules** to assert:
   - `Domain` does not reference `Application`, `Infrastructure`, or `API`.
   - `Application` does not reference `Infrastructure` or `API`.
   - `Infrastructure` does not reference `API`.
   - All classes named `*Controller` in `Sery.API` reside in namespaces matching `Sery.API.Controllers.V{n}` (folder `Controllers/V{n}/`).
3. **Physical API versioning**: controllers are placed under `src/Sery.API/Controllers/V1/`, `V2/`, etc., with matching namespaces, in addition to URL segment versioning and `[ApiVersion]`.

## Consequences

### Positive

- Formatting and trivial style drift are reduced across IDEs.
- Layer violations fail CI immediately.
- Version folders make code review and regression scope obvious (`V1` vs `V2`).

### Negative / Trade-offs

- Slight duplication when shipping parallel major versions (separate controller types per version).
- Architecture tests add compile-time references from the test project to all layers (acceptable for a dedicated test assembly).

## Alternatives Considered

1. **Only URL versioning without folders** - rejected; harder to navigate and review when multiple major versions coexist.
2. **Manual architecture reviews only** - rejected; not scalable as the team grows.
3. **TreatWarningsAsErrors globally** - deferred; may be enabled later once the codebase is clean.
