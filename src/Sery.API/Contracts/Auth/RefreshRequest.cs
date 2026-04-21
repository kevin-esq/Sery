namespace Sery.API.Contracts.Auth;

/// <summary>
/// Rotates the current refresh token and returns a new token pair.
/// </summary>
public sealed class RefreshRequest
{
    /// <summary>
    /// Current refresh token.
    /// </summary>
    /// <example>9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...</example>
    public string RefreshToken { get; init; } = string.Empty;
}
