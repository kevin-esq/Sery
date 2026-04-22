namespace Sery.Application.Conversations;

public interface IConversationService
{
    Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ConversationDetailsDto?> GetConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<ConversationDetailsDto?> UpdateMetadataAsync(
        Guid userId,
        Guid conversationId,
        ConversationMetadataUpdateDto update,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}
