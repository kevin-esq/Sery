namespace Sery.API.Contracts.Auth;

/// <summary>
/// Returns the authenticated user context extracted from the access token.
/// </summary>
public sealed class MeResponse
{
    /// <summary>
    /// Authenticated user identifier.
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Authenticated user email.
    /// </summary>
    /// <example>user@example.com</example>
    public string? Email { get; init; }

    /// <summary>
    /// Tenant identifier.
    /// </summary>
    /// <example>22222222-2222-2222-2222-222222222222</example>
    public Guid TenantId { get; init; }
}
