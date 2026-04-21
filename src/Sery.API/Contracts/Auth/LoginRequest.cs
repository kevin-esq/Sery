namespace Sery.API.Contracts.Auth;

/// <summary>
/// Authenticates an existing user.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Plain text password.
    /// </summary>
    /// <example>P@ssw0rd123!</example>
    public string Password { get; init; } = string.Empty;
}
