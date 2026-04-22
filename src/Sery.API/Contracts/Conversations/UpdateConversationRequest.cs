namespace Sery.API.Contracts.Conversations;

/// <summary>
/// Partial metadata update for a conversation.
/// </summary>
public sealed class UpdateConversationRequest
{
    /// <summary>
    /// Custom title set by the user.
    /// </summary>
    /// <example>Plan para bajar mi ansiedad esta semana</example>
    public string? Title { get; init; }

    /// <summary>
    /// Archives or unarchives the conversation.
    /// </summary>
    /// <example>true</example>
    public bool? IsArchived { get; init; }

    /// <summary>
    /// Pins or unpins the conversation.
    /// </summary>
    /// <example>true</example>
    public bool? IsPinned { get; init; }
}
