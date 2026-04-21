namespace Sery.API.Contracts.Chat;

/// <summary>
/// Request payload to queue a chat message.
/// </summary>
public sealed class SendMessageRequest
{
    /// <summary>
    /// Raw message content.
    /// </summary>
    /// <example>Hola Sery, hoy me siento nervioso.</example>
    public string Message { get; init; } = string.Empty;
}
