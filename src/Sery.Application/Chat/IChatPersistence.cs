using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public interface IChatPersistence
{
    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<Conversation?> GetLatestConversationAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Message>> GetRecentMessagesAsync(Guid conversationId, int limit, CancellationToken cancellationToken);
    void AddUser(User user);
    void AddConversation(Conversation conversation);
    void AddMessage(Message message);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
