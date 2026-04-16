using Microsoft.Extensions.Logging.Abstractions;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public class ChatMessageServiceTests
{
    [Fact]
    public async Task QueueMessageAsync_ShouldPersistAssistantMessage_WhenAIResponseSucceeds()
    {
        var persistence = new InMemoryChatPersistence();
        var aiService = new StubChatAIService("Take one small real-world step today.");
        var service = new ChatMessageService(persistence, aiService, NullLogger<ChatMessageService>.Instance);
        var userId = Guid.NewGuid();
        var command = new QueueMessageCommand(userId, "  hello sery  ");

        QueueMessageResult result = await service.QueueMessageAsync(command);

        Assert.NotEqual(Guid.Empty, result.ConversationId);
        Assert.Single(persistence.Users);
        Assert.Single(persistence.Conversations);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal(userId, persistence.Users.Single().Id);
        Assert.Equal(result.ConversationId, persistence.Messages[0].ConversationId);
        Assert.Equal("hello sery", persistence.Messages[0].Content);
        Assert.Equal(MessageRole.User, persistence.Messages[0].Role);
        Assert.Equal(result.ConversationId, persistence.Messages[1].ConversationId);
        Assert.Equal("Take one small real-world step today.", persistence.Messages[1].Content);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
        Assert.Equal("Take one small real-world step today.", result.AssistantMessage);
    }

    [Fact]
    public async Task QueueMessageAsync_ShouldReturnFallback_WhenAIFails()
    {
        var persistence = new InMemoryChatPersistence();
        var aiService = new ThrowingChatAIService();
        var service = new ChatMessageService(persistence, aiService, NullLogger<ChatMessageService>.Instance);
        var command = new QueueMessageCommand(Guid.NewGuid(), "need help");

        QueueMessageResult result = await service.QueueMessageAsync(command);

        Assert.Equal("I'm having trouble responding right now, but I'm here with you. Want to try again?", result.AssistantMessage);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
        Assert.Equal("I'm having trouble responding right now, but I'm here with you. Want to try again?", persistence.Messages[1].Content);
    }

    private sealed class InMemoryChatPersistence : IChatPersistence
    {
        public List<User> Users { get; } = [];
        public List<Conversation> Conversations { get; } = [];
        public List<Message> Messages { get; } = [];

        public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(Users.SingleOrDefault(x => x.Id == userId));

        public Task<Conversation?> GetLatestConversationAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(Conversations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault());

        public Task<IReadOnlyList<Message>> GetRecentMessagesAsync(Guid conversationId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Message>>(Messages
                .Where(x => x.ConversationId == conversationId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToList());

        public void AddUser(User user) => Users.Add(user);
        public void AddConversation(Conversation conversation) => Conversations.Add(conversation);
        public void AddMessage(Message message) => Messages.Add(message);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubChatAIService(string response) : IChatAIService
    {
        public Task<string> GenerateResponseAsync(List<ChatMessageDto> messages, CancellationToken ct)
            => Task.FromResult(response);
    }

    private sealed class ThrowingChatAIService : IChatAIService
    {
        public Task<string> GenerateResponseAsync(List<ChatMessageDto> messages, CancellationToken ct)
            => throw new InvalidOperationException("AI provider unavailable");
    }
}
