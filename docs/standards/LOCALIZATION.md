# API localization (ProblemDetails)

## Goals

- Keep a **stable machine contract**: `code` and `messageKey` never change per locale.
- Localize **human-readable** fields (`title`, `detail`) using the same keys as `messageKey` where applicable.
- Fail safe: if a key is missing in a satellite assembly, fall back to `ErrorCatalog` defaults (neutral English).

## Resource files

| File                                                | Role                                   |
| --------------------------------------------------- | -------------------------------------- |
| `src/Sery.API/Resources/ApiMessages.resx`           | Default (English) strings              |
| `src/Sery.API/Resources/ApiMessages.{culture}.resx` | Satellite (e.g. `ApiMessages.es.resx`) |
| `src/Sery.API/Resources/ApiMessages.cs`             | Marker type for resource manifest name |

**Key names** must match `messageKey` values in `ErrorCatalog` (e.g. `error.chat.required_userid_message`).

## Runtime

- Culture comes from **ASP.NET Core request localization** (`Accept-Language` first, then query string and cookie providers).
- Supported cultures are configured in `AddApiLocalization` (`en`, `en-US`, `es`, `es-MX` today).
- `IApiProblemDetailsFactory` uses `ResourceManager` + `CultureInfo.CurrentUICulture` (set by `UseRequestLocalization` after `UseRouting`).

## Client usage

Send a standard header, for example:

```http
Accept-Language: es
```

The response still includes:

- `messageKey`: `error.chat.required_userid_message`
- `title` / `detail`: strings in the requested language when available.

## Adding a new string

1. Add the key to `ApiMessages.resx` (and satellites).
2. Add or reuse an `ApiError` in `ErrorCatalog` with the same `messageKey`.
3. Document the key in [ERROR_HANDLING.md](ERROR_HANDLING.md) if it is part of the public error catalog.
