namespace Sery.API.Contracts.Chat;

/// <summary>
/// Accepted response for queued chat messages.
/// </summary>
public sealed class SendMessageAcceptedResponse
{
    /// <summary>
    /// Unique identifier of the user who submitted the message.
    /// </summary>
    /// <example>7d06b9f5-2fdb-44d8-88e8-9b2c49a23c5a</example>
    public Guid UserId { get; init; }

    /// <summary>
    /// Sanitized message content.
    /// </summary>
    /// <example>Hola Sery, hoy me siento nervioso.</example>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Current processing state for the message.
    /// </summary>
    /// <example>queued</example>
    public string Status { get; init; } = string.Empty;
}
