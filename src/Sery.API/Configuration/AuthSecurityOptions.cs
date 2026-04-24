namespace Sery.API.Configuration;

public sealed class AuthSecurityOptions
{
    public const string SectionName = "AuthSecurity";

    /// <summary>
    /// Cookie name for the refresh token. Uses __Secure- prefix for HTTPS enforcement.
    /// </summary>
    public string RefreshTokenCookieName { get; init; } = "__Secure-refresh_token";

    /// <summary>
    /// Cookie path scope. Only sent to auth endpoints.
    /// </summary>
    public string RefreshTokenCookiePath { get; init; } = "/api/v1/auth";

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; init; } = 7;
}
