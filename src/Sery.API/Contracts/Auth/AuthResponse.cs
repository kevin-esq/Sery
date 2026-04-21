namespace Sery.API.Contracts.Auth;

/// <summary>
/// Returns access and refresh tokens after a successful authentication flow.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// Short-lived JWT access token.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Long-lived refresh token.
    /// </summary>
    /// <example>9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...</example>
    public string RefreshToken { get; init; } = string.Empty;
}
