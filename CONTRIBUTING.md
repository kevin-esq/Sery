# Contributing to Sery

## Workflow

1. Create branch from `develop`:

- `feature/<short-name>`
- `fix/<short-name>`

2. Keep commits focused and small.
3. Open Pull Request to `develop`.
4. Ensure CI passes and at least one review is completed.

## Local Development

```bash
dotnet restore
dotnet build Sery.sln
dotnet test Sery.sln
```

Run API:

```bash
dotnet run --project src/Sery.API/Sery.API.csproj
```

## Coding Guidelines

- Follow [docs/standards/CODING_STANDARDS.md](docs/standards/CODING_STANDARDS.md) as the primary coding convention reference.
- Follow API conventions in [docs/standards/API_STANDARDS.md](docs/standards/API_STANDARDS.md).
- Keep endpoint documentation aligned with [docs/standards/SWAGGER.md](docs/standards/SWAGGER.md).
- Keep per-controller docs updated under [docs/api/controllers/](docs/api/controllers/).
- Keep error contracts aligned with [docs/standards/ERROR_HANDLING.md](docs/standards/ERROR_HANDLING.md).
- Add or update `.resx` entries when introducing new `messageKey` values ([docs/standards/LOCALIZATION.md](docs/standards/LOCALIZATION.md)).
- Use the [documentation index](docs/README.md) to find the rest.
- Respect [.editorconfig](.editorconfig); architecture rules are enforced by `tests/Sery.Architecture.Tests`.
- Respect layered architecture boundaries:
    - `Domain` has no external dependencies.
    - `Application` depends on `Domain`.
    - `Infrastructure` implements adapters/integrations.
    - `API` composes application + infrastructure.
- Use dependency injection via `DependencyInjection` classes.
- Add/update tests for each meaningful change.
- Prefer explicit naming and single-responsibility classes.

## Pull Request Checklist

- Branch is up-to-date with target branch.
- Tests added/updated.
- `dotnet test Sery.sln` passes locally.
- Docs updated when behavior or architecture changed.
- ADR added when making architectural decisions.
