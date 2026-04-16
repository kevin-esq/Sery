# Swagger and OpenAPI Guidelines

## Purpose

Define how API contracts are documented in Swagger/OpenAPI for Sery.

## Rules

- Every public controller action must include XML comments (`summary`, `param`, `returns`).
- Every endpoint must declare response codes with `ProducesResponseType`.
- Request and response DTOs should be explicit and stable.
- Do not expose internal exceptions or stack traces in API docs.
- Keep route naming versioned and RESTful: `api/v{version}/[controller]`.
- Document endpoint outcomes in `<remarks>` and in [api/controllers/](../api/controllers/) (one markdown file per controller where practical).
- Prefer typed responses over anonymous objects so schemas are explicit.

## Local Verification

Run the API and inspect Swagger UI:

```bash
dotnet run --project src/Sery.API/Sery.API.csproj
```

Then open:

- `https://localhost:5001/swagger` (or the URL shown by the API logs)

## Done Criteria for New Endpoints

- Endpoint appears in Swagger with description.
- Request model is documented.
- Success and failure HTTP codes are documented.
- Integration test exists for the endpoint behavior.
- At least one concrete example is provided in schema/XML docs and controller markdown docs.
