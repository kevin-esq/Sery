namespace Sery.Application.Conversations;

public interface IConversationPersistence
{
    Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken);

    Task<ConversationDetailsDto?> GetConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<ConversationDetailsDto?> UpdateMetadataAsync(
        Guid userId,
        Guid conversationId,
        ConversationMetadataUpdateDto update,
        CancellationToken cancellationToken);

    Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken);
}
