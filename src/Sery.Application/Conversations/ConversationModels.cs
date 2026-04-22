using Sery.Domain.Entities;

namespace Sery.Application.Conversations;

public sealed record ConversationListItemDto(
    Guid Id,
    string Title,
    string Summary,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastMessageAt,
    string Preview,
    int MessageCount,
    bool IsArchived,
    bool IsPinned);

public sealed record ConversationMessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    DateTime CreatedAt);

public sealed record ConversationDetailsDto(
    Guid Id,
    string Title,
    string Summary,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastMessageAt,
    string Preview,
    int MessageCount,
    bool IsArchived,
    bool IsPinned);

public sealed record ConversationMetadataUpdateDto(
    string? Title,
    bool? IsArchived,
    bool? IsPinned);

public sealed record ConversationInsightsDto(
    string Title,
    bool IsTitleGenerated,
    string Summary,
    DateTime UpdatedAt,
    DateTime? LastMessageAt);

public sealed record EmotionalMemoryProfileDto(
    string Summary,
    string DominantEmotion,
    DateTime UpdatedAt);
