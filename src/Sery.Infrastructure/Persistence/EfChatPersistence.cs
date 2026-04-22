using Microsoft.EntityFrameworkCore;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfChatPersistence(SeryDbContext dbContext) : IChatPersistence
{
    public async Task<ConversationContext> GetConversationContextAsync(
        Guid userId,
        Guid? conversationId,
        int limit,
        CancellationToken ct)
    {
        UserEmotionalMemory? emotionalMemory = await dbContext.UserEmotionalMemories
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        UserChatEfficacyProfile? efficacyProfile = await dbContext.UserChatEfficacyProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        UserAgentCustomization? agentCustomization = await dbContext.UserAgentCustomizations
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        string language = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.PreferredLanguage)
            .FirstOrDefaultAsync(ct) ?? "es";

        if (conversationId is null)
        {
            return new ConversationContext(null, [], language, emotionalMemory, efficacyProfile, agentCustomization, [], []);
        }

        var data = await dbContext.Conversations
            .Where(x => x.UserId == userId && x.Id == conversationId.Value)
            .Select(c => new
            {
                Conversation = c,
                Messages = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(limit)
            })
            .FirstOrDefaultAsync(ct);

        if (data is null)
        {
            return new ConversationContext(null, [], language, emotionalMemory, efficacyProfile, agentCustomization, [], []);
        }

        var history = data.Messages.OrderBy(m => m.CreatedAt).ToList();

        return new ConversationContext(data.Conversation, history, language, emotionalMemory, efficacyProfile, agentCustomization, [], []);
    }

    public async Task<IReadOnlyList<Message>> GetConversationTimelineAsync(Guid conversationId, int limit, CancellationToken ct)
    {
        return await dbContext.Messages
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Message>> GetRecentUserMessagesAsync(Guid userId, int limit, CancellationToken ct)
    {
        return await dbContext.Messages
            .Where(x => x.Role == MessageRole.User && x.Conversation != null && x.Conversation.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Message>> GetRelevantMessagesAsync(
        Guid userId,
        Guid? currentConversationId,
        string userMessage,
        int limit,
        CancellationToken ct)
    {
        List<Message> candidates = await dbContext.Messages
            .AsNoTracking()
            .Where(x => x.Role == MessageRole.User &&
                x.Conversation != null &&
                x.Conversation.UserId == userId &&
                (currentConversationId == null || x.ConversationId != currentConversationId.Value))
            .OrderByDescending(x => x.CreatedAt)
            .Take(120)
            .ToListAsync(ct);

        return RelevantMemoryScorer.RankMessages(userMessage, candidates, limit);
    }

    public async Task<IReadOnlyList<UserMemoryFact>> GetRelevantFactsAsync(
        Guid userId,
        string userMessage,
        int limit,
        CancellationToken ct)
    {
        List<UserMemoryFact> candidates = await dbContext.UserMemoryFacts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastSeenAt)
            .Take(80)
            .ToListAsync(ct);

        return RelevantMemoryScorer.RankFacts(userMessage, candidates, limit);
    }

    public Task<UserMemoryFact?> FindUserMemoryFactByKeyAsync(Guid userId, string normalizedKey, CancellationToken ct)
    {
        return dbContext.UserMemoryFacts
            .FirstOrDefaultAsync(x => x.UserId == userId && x.NormalizedKey == normalizedKey, ct);
    }

    public void AddConversation(Conversation conversation)
    {
        _ = dbContext.Conversations.Add(conversation);
    }

    public void AddMessage(Message message)
    {
        _ = dbContext.Messages.Add(message);
    }

    public void AddUserEmotionalMemory(UserEmotionalMemory emotionalMemory)
    {
        _ = dbContext.UserEmotionalMemories.Add(emotionalMemory);
    }

    public void AddUserChatEfficacyProfile(UserChatEfficacyProfile efficacyProfile)
    {
        _ = dbContext.UserChatEfficacyProfiles.Add(efficacyProfile);
    }

    public void AddUserMemoryFact(UserMemoryFact fact)
    {
        _ = dbContext.UserMemoryFacts.Add(fact);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await dbContext.SaveChangesAsync(cancellationToken);
    }
}
