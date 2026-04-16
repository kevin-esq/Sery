using Microsoft.EntityFrameworkCore;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfChatPersistence(SeryDbContext dbContext) : IChatPersistence
{
    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<Conversation?> GetLatestConversationAsync(Guid userId, CancellationToken cancellationToken)
        => dbContext.Conversations
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Message>> GetRecentMessagesAsync(
        Guid conversationId,
        int limit,
        CancellationToken cancellationToken)
        => await dbContext.Messages
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public void AddUser(User user) => dbContext.Users.Add(user);

    public void AddConversation(Conversation conversation) => dbContext.Conversations.Add(conversation);

    public void AddMessage(Message message) => dbContext.Messages.Add(message);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
