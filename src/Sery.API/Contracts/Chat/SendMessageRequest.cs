namespace Sery.API.Contracts.Chat;

/// <summary>
/// Request payload to queue a chat message.
/// </summary>
public sealed class SendMessageRequest
{
    /// <summary>
    /// Unique identifier of the user sending the message.
    /// </summary>
    /// <example>7d06b9f5-2fdb-44d8-88e8-9b2c49a23c5a</example>
    public Guid UserId { get; init; }

    /// <summary>
    /// Raw message content.
    /// </summary>
    /// <example>Hola Sery, hoy me siento nervioso.</example>
    public string Message { get; init; } = string.Empty;
}
