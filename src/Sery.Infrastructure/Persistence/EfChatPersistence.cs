using Microsoft.EntityFrameworkCore;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfChatPersistence(SeryDbContext dbContext) : IChatPersistence
{
    public async Task<IReadOnlyList<Message>> GetRecentMessagesAsync(
        Guid conversationId,
        int limit,
        CancellationToken cancellationToken)
    {
        return await dbContext.Messages
                .Where(x => x.ConversationId == conversationId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
    }

    public void AddUser(User user)
    {
        _ = dbContext.Users.Add(user);
    }

    public async Task<ConversationContext> GetConversationContextAsync(Guid userId, int limit, CancellationToken ct)
    {
        var data = await dbContext.Conversations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(c => new
            {
                Conversation = c,
                Messages = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(limit)
            })
            .FirstOrDefaultAsync(ct);

        string language = await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.PreferredLanguage)
            .FirstOrDefaultAsync(ct) ?? "es";

        if (data is null)
        {
            return new ConversationContext(null, [], language);
        }

        var history = data.Messages.OrderBy(m => m.CreatedAt).ToList();

        return new ConversationContext(data.Conversation, history, language);
    }

    public void AddConversation(Conversation conversation)
    {
        _ = dbContext.Conversations.Add(conversation);
    }

    public void AddMessage(Message message)
    {
        _ = dbContext.Messages.Add(message);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await dbContext.SaveChangesAsync(cancellationToken);
    }
}
