using Sery.Domain.Entities;

namespace Sery.API.Contracts.Conversations;

/// <summary>
/// Single stored message within a conversation timeline.
/// </summary>
public sealed class ConversationMessageResponse
{
    /// <summary>
    /// Message identifier.
    /// </summary>
    /// <example>3c1b26d0-84f7-40f3-a3a8-00a517d9bf03</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Message role in the conversation.
    /// </summary>
    /// <example>User</example>
    public MessageRole Role { get; init; }

    /// <summary>
    /// Message content.
    /// </summary>
    /// <example>Hola Sery, hoy me siento nervioso.</example>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the message was created.
    /// </summary>
    /// <example>2026-04-21T20:57:00Z</example>
    public DateTime CreatedAt { get; init; }
}
