using System.ComponentModel.DataAnnotations;

namespace Sery.API.Contracts.Auth;

/// <summary>
/// Registers a new user account.
/// </summary>
public sealed class RegisterRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    /// <example>user@example.com</example>
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Plain text password. Must be 8-128 characters with at least one uppercase letter, one lowercase letter, and one digit.
    /// </summary>
    /// <example>P@ssw0rd123!</example>
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
