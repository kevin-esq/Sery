namespace Sery.API.Contracts.Health;

/// <summary>
/// Liveness response contract.
/// </summary>
public sealed class HealthResponse
{
    /// <summary>
    /// Liveness status.
    /// </summary>
    /// <example>ok</example>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// API service identifier.
    /// </summary>
    /// <example>Sery.API</example>
    public string Service { get; init; } = string.Empty;
}
