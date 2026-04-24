namespace Sery.API.Contracts.Auth;

/// <summary>
/// Returns the access token after a successful authentication flow.
/// The refresh token is set as an HttpOnly cookie and is not exposed in the response body.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// Short-lived JWT access token.
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string AccessToken { get; init; } = string.Empty;
}
