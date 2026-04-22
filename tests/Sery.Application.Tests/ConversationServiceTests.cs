using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public sealed class ConversationServiceTests
{
    [Fact]
    public async Task GetMessagesAsync_ShouldNormalizePagination()
    {
        var persistence = new SpyConversationPersistence();
        var service = new ConversationService(persistence);

        _ = await service.GetMessagesAsync(Guid.NewGuid(), Guid.NewGuid(), -10, 999);

        Assert.Equal(0, persistence.LastSkip);
        Assert.Equal(100, persistence.LastTake);
    }

    private sealed class SpyConversationPersistence : IConversationPersistence
    {
        public int LastSkip { get; private set; }
        public int LastTake { get; private set; }

        public Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ConversationListItemDto>>([]);
        }

        public Task<ConversationDetailsDto?> GetConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken)
        {
            return Task.FromResult<ConversationDetailsDto?>(new ConversationDetailsDto(
                conversationId,
                "title",
                "summary",
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "preview",
                1,
                false,
                false));
        }

        public Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
            Guid userId,
            Guid conversationId,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            LastSkip = skip;
            LastTake = take;

            IReadOnlyList<ConversationMessageDto> messages =
            [
                new ConversationMessageDto(Guid.NewGuid(), MessageRole.User, "Hola", DateTime.UtcNow)
            ];

            return Task.FromResult(messages);
        }

        public Task<bool> DeleteConversationAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<ConversationDetailsDto?> UpdateMetadataAsync(
            Guid userId,
            Guid conversationId,
            ConversationMetadataUpdateDto update,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ConversationDetailsDto?>(new ConversationDetailsDto(
                conversationId,
                update.Title ?? "title",
                "summary",
                DateTime.UtcNow,
                DateTime.UtcNow,
                DateTime.UtcNow,
                "preview",
                1,
                update.IsArchived ?? false,
                update.IsPinned ?? false));
        }
    }
}
