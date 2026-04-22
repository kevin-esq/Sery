using Sery.Domain.Entities;

namespace Sery.API.Contracts.Conversations;

/// <summary>
/// Summary of a conversation owned by the authenticated user.
/// </summary>
public sealed class ConversationListItemResponse
{
    /// <summary>
    /// Conversation identifier.
    /// </summary>
    /// <example>1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38</example>
    public Guid Id { get; init; }

    /// <summary>
    /// Human-friendly conversation title.
    /// </summary>
    /// <example>Anxiety before tomorrow's meeting</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Compressed summary used to preserve continuity.
    /// </summary>
    /// <example>The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues.</example>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the conversation was created.
    /// </summary>
    /// <example>2026-04-21T20:56:52Z</example>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// UTC timestamp when the conversation metadata was last updated.
    /// </summary>
    /// <example>2026-04-21T21:01:18Z</example>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// UTC timestamp of the last message in the conversation.
    /// </summary>
    /// <example>2026-04-21T21:01:18Z</example>
    public DateTime? LastMessageAt { get; init; }

    /// <summary>
    /// Last message preview.
    /// </summary>
    /// <example>Respira conmigo. Vamos paso a paso.</example>
    public string Preview { get; init; } = string.Empty;

    /// <summary>
    /// Total messages stored for the conversation.
    /// </summary>
    /// <example>14</example>
    public int MessageCount { get; init; }

    /// <summary>
    /// Indicates whether the conversation is archived.
    /// </summary>
    /// <example>false</example>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Indicates whether the conversation is pinned.
    /// </summary>
    /// <example>true</example>
    public bool IsPinned { get; init; }
}
