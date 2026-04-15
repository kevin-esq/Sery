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

- Respect layered architecture boundaries:
  - `Domain` has no external dependencies.
  - `Application` depends on `Domain`.
  - `Infrastructure` implements adapters/integrations.
  - `API` composes application + infrastructure.
- Use dependency injection via `DependencyInjection` classes.
- Add/update tests for each meaningful change.
- Prefer explicit naming and single-responsibility classes.

## Pull Request Checklist

- [ ] Branch is up-to-date with target branch.
- [ ] Tests added/updated.
- [ ] `dotnet test Sery.sln` passes locally.
- [ ] Docs updated when behavior or architecture changed.
- [ ] ADR added when making architectural decisions.
