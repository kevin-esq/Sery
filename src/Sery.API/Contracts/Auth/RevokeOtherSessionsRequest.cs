namespace Sery.API.Contracts.Auth;

/// <summary>
/// Revokes every other session owned by the authenticated user.
/// </summary>
public sealed class RevokeOtherSessionsRequest
{
    /// <summary>
    /// Refresh token of the session that must remain active.
    /// </summary>
    /// <example>9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...</example>
    public string CurrentRefreshToken { get; init; } = string.Empty;
}
