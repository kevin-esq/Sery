using Microsoft.EntityFrameworkCore;
using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfConversationPersistence(SeryDbContext dbContext) : IConversationPersistence
{
    public async Task<IReadOnlyList<ConversationListItemDto>> GetConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsPinned)
            .ThenBy(x => x.IsArchived)
            .ThenByDescending(x => x.LastMessageAt ?? x.UpdatedAt)
            .Select(x => new ConversationListItemDto(
                x.Id,
                x.Title,
                x.Summary,
                x.CreatedAt,
                x.UpdatedAt,
                x.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault(),
                x.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? string.Empty,
                x.Messages.Count,
                x.IsArchived,
                x.IsPinned))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationDetailsDto?> GetConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Id == conversationId)
            .Select(x => new ConversationDetailsDto(
                x.Id,
                x.Title,
                x.Summary,
                x.CreatedAt,
                x.UpdatedAt,
                x.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => (DateTime?)m.CreatedAt)
                    .FirstOrDefault(),
                x.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? string.Empty,
                x.Messages.Count,
                x.IsArchived,
                x.IsPinned))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationMessageDto>> GetMessagesAsync(
        Guid userId,
        Guid conversationId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId && x.Conversation != null && x.Conversation.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new ConversationMessageDto(
                x.Id,
                x.Role,
                x.Content,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationDetailsDto?> UpdateMetadataAsync(
        Guid userId,
        Guid conversationId,
        ConversationMetadataUpdateDto update,
        CancellationToken cancellationToken)
    {
        Conversation? conversation = await dbContext.Conversations
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        if (update.Title is not null)
        {
            conversation.Title = update.Title.Trim();
            conversation.IsTitleGenerated = false;
        }

        if (update.IsArchived.HasValue)
        {
            conversation.IsArchived = update.IsArchived.Value;
        }

        if (update.IsPinned.HasValue)
        {
            conversation.IsPinned = update.IsPinned.Value;
        }

        conversation.UpdatedAt = DateTime.UtcNow;

        _ = await dbContext.SaveChangesAsync(cancellationToken);

        return await GetConversationAsync(userId, conversationId, cancellationToken);
    }

    public async Task<bool> DeleteConversationAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        int deleted = await dbContext.Conversations
            .Where(x => x.UserId == userId && x.Id == conversationId)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}
