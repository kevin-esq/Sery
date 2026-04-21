namespace Sery.API.Contracts.Auth;

/// <summary>
/// Describes one active user session.
/// </summary>
public sealed class SessionResponse
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    /// <example>1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Captured device information from the request.
    /// </summary>
    /// <example>Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36</example>
    public string? DeviceInfo { get; init; }

    /// <summary>
    /// Last known IP address.
    /// </summary>
    /// <example>203.0.113.10</example>
    public string? IpAddress { get; init; }

    /// <summary>
    /// UTC creation time.
    /// </summary>
    /// <example>2026-04-21T20:56:52Z</example>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// UTC expiration time.
    /// </summary>
    /// <example>2026-04-28T20:56:52Z</example>
    public DateTime ExpiresAt { get; init; }
}
