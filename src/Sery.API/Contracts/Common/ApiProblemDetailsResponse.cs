namespace Sery.API.Contracts.Common;

/// <summary>
/// Standard API error contract returned as ProblemDetails with internal metadata.
/// </summary>
public sealed class ApiProblemDetailsResponse
{
    /// <summary>
    /// Problem type URI.
    /// </summary>
    /// <example>https://httpstatuses.com/401</example>
    public string? Type { get; init; }

    /// <summary>
    /// Short, human-readable summary of the problem.
    /// </summary>
    /// <example>Invalid credentials.</example>
    public string? Title { get; init; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    /// <example>401</example>
    public int? Status { get; init; }

    /// <summary>
    /// Human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    /// <example>Invalid credentials.</example>
    public string? Detail { get; init; }

    /// <summary>
    /// Request path where the error occurred.
    /// </summary>
    /// <example>/api/v1/auth/login</example>
    public string? Instance { get; init; }

    /// <summary>
    /// Stable internal error code for client-side handling.
    /// </summary>
    /// <example>SERY-API-401-001</example>
    public string? Code { get; init; }

    /// <summary>
    /// Localization key associated with the error.
    /// </summary>
    /// <example>error.auth.invalid_credentials</example>
    public string? MessageKey { get; init; }
}
