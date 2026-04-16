namespace Sery.API.Contracts.Chat;

/// <summary>
/// Response payload for chat message processing.
/// </summary>
public sealed class SendMessageAcceptedResponse
{
    /// <summary>
    /// Unique identifier of the conversation where the message was stored.
    /// </summary>
    /// <example>31d4f6da-ebf1-4c7e-b8ca-f95cb6de8d53</example>
    public Guid ConversationId { get; init; }

    /// <summary>
    /// Assistant response generated for the conversation.
    /// </summary>
    /// <example>I hear you. Try taking one small step today and notice how it feels.</example>
    public string AssistantMessage { get; init; } = string.Empty;
}
