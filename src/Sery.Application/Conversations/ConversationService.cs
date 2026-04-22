namespace Sery.Application.Conversations;

public sealed class ConversationService(IConversationPersistence conversationPersistence) : IConversationService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    public Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return conversationPersistence.GetConversationsAsync(userId, cancellationToken);
    }

    public Task<ConversationDetailsDto?> GetConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return conversationPersistence.GetConversationAsync(userId, conversationId, cancellationToken);
    }

    public Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        int normalizedSkip = Math.Max(0, skip);
        int normalizedTake = take <= 0 ? DefaultPageSize : Math.Min(take, MaxPageSize);

        return conversationPersistence.GetMessagesAsync(
            userId,
            conversationId,
            normalizedSkip,
            normalizedTake,
            cancellationToken);
    }

    public Task<ConversationDetailsDto?> UpdateMetadataAsync(
        Guid userId,
        Guid conversationId,
        ConversationMetadataUpdateDto update,
        CancellationToken cancellationToken = default)
    {
        return conversationPersistence.UpdateMetadataAsync(userId, conversationId, update, cancellationToken);
    }

    public Task<bool> DeleteConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return conversationPersistence.DeleteConversationAsync(userId, conversationId, cancellationToken);
    }
}
