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

    /// <summary>
    /// Existing conversation identifier. Omit to start a new conversation.
    /// </summary>
    /// <example>1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38</example>
    public Guid? ConversationId { get; init; }
}
