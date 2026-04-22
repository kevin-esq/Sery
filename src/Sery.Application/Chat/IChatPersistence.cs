using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public record ConversationContext(
    Conversation? Conversation,
    IReadOnlyList<Message> History,
    string Language,
    UserEmotionalMemory? EmotionalMemory,
    UserChatEfficacyProfile? EfficacyProfile,
    UserAgentCustomization? AgentCustomization,
    IReadOnlyList<Message> RelevantHistory,
    IReadOnlyList<UserMemoryFact> RelevantFacts);

public interface IChatPersistence
{
    Task<ConversationContext> GetConversationContextAsync(
        Guid userId,
        Guid? conversationId,
        int limit,
        CancellationToken ct);

    Task<IReadOnlyList<Message>> GetConversationTimelineAsync(Guid conversationId, int limit, CancellationToken ct);

    Task<IReadOnlyList<Message>> GetRecentUserMessagesAsync(Guid userId, int limit, CancellationToken ct);

    Task<IReadOnlyList<Message>> GetRelevantMessagesAsync(
        Guid userId,
        Guid? currentConversationId,
        string userMessage,
        int limit,
        CancellationToken ct);

    Task<IReadOnlyList<UserMemoryFact>> GetRelevantFactsAsync(
        Guid userId,
        string userMessage,
        int limit,
        CancellationToken ct);

    Task<UserMemoryFact?> FindUserMemoryFactByKeyAsync(Guid userId, string normalizedKey, CancellationToken ct);

    void AddConversation(Conversation conversation);
    void AddMessage(Message message);
    void AddUserEmotionalMemory(UserEmotionalMemory emotionalMemory);
    void AddUserChatEfficacyProfile(UserChatEfficacyProfile efficacyProfile);
    void AddUserMemoryFact(UserMemoryFact fact);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
