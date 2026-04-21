using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public record ConversationContext(Conversation? Conversation, IReadOnlyList<Message> History, string Language);

public interface IChatPersistence
{
    Task<ConversationContext> GetConversationContextAsync(Guid userId, int limit, CancellationToken ct);

    void AddConversation(Conversation conversation);
    void AddMessage(Message message);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
