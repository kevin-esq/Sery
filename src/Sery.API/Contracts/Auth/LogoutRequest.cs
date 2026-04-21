namespace Sery.API.Contracts.Auth;

/// <summary>
/// Revokes the current session represented by the provided refresh token.
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// Refresh token to revoke.
    /// </summary>
    /// <example>9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...</example>
    public string RefreshToken { get; init; } = string.Empty;
}
