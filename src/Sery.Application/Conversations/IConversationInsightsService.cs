using Sery.Domain.Entities;

namespace Sery.Application.Conversations;

public interface IConversationInsightsService
{
    ConversationInsightsDto BuildConversationInsights(
        Conversation conversation,
        IReadOnlyList<Message> timeline);

    EmotionalMemoryProfileDto BuildEmotionalMemoryProfile(
        UserEmotionalMemory? currentMemory,
        IReadOnlyList<Message> recentUserMessages);
}
