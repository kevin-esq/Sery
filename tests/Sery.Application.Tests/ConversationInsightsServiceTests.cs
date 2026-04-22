using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public sealed class ConversationInsightsServiceTests
{
    [Fact]
    public void BuildConversationInsights_ShouldGenerateTitleAndSummary()
    {
        var service = new ConversationInsightsService();
        var conversation = new Conversation { UserId = Guid.NewGuid() };
        IReadOnlyList<Message> timeline =
        [
            new Message { ConversationId = conversation.Id, Role = MessageRole.User, Content = "Me siento muy nervioso por el trabajo y mañana tengo una reunión importante." },
            new Message { ConversationId = conversation.Id, Role = MessageRole.Assistant, Content = "Respira conmigo y vamos paso a paso." }
        ];

        ConversationInsightsDto result = service.BuildConversationInsights(conversation, timeline);

        Assert.False(string.IsNullOrWhiteSpace(result.Title));
        Assert.Contains("work pressure", result.Summary);
    }

    [Fact]
    public void BuildEmotionalMemoryProfile_ShouldDetectDominantEmotion()
    {
        var service = new ConversationInsightsService();
        IReadOnlyList<Message> timeline =
        [
            new Message { ConversationId = Guid.NewGuid(), Role = MessageRole.User, Content = "Estoy ansioso y muy nervioso por el trabajo." },
            new Message { ConversationId = Guid.NewGuid(), Role = MessageRole.User, Content = "Me siento abrumado y con ansiedad." }
        ];

        EmotionalMemoryProfileDto result = service.BuildEmotionalMemoryProfile(null, timeline);

        Assert.Equal("anxious", result.DominantEmotion);
        Assert.Contains("practical guidance", result.Summary);
    }
}
