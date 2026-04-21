using Microsoft.Extensions.Logging.Abstractions;
using Sery.Application.Chat;
using Sery.Application.Common.Interfaces;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public class ChatMessageServiceTests
{
    private sealed class MockUserContext(Guid userId) : IUserContext
    {
        public Guid? UserId => userId;
        public Guid TenantId => Guid.Empty;
        public string? Email => "test@example.com";
    }

    [Fact]
    public async Task QueueMessageStreamAsyncShouldPersistAssistantMessageWhenAIResponseSucceeds()
    {
        var userId = Guid.NewGuid();
        var persistence = new InMemoryChatPersistence();
        var aiService = new StubChatAIService(["Take ", "one small ", "step."]);
        var userContext = new MockUserContext(userId);
        var service = new ChatMessageService(persistence, aiService, userContext, NullLogger<ChatMessageService>.Instance);
        var command = new QueueMessageCommand("  hello sery  ");

        var results = new List<StreamChunkDto>();
        await foreach (StreamChunkDto chunk in service.QueueMessageStreamAsync(command))
        {
            results.Add(chunk);
        }

        Assert.Equal(4, results.Count); // 3 chunk dtos + 1 final empty chunk
        Assert.True(results.Last().f);
        _ = Assert.Single(persistence.Conversations);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal("hello sery", persistence.Messages[0].Content);
        Assert.Equal(MessageRole.User, persistence.Messages[0].Role);
        Assert.Equal("Take one small step.", persistence.Messages[1].Content);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
    }

    [Fact]
    public async Task QueueMessageStreamAsyncShouldReturnFallbackAndPersistWhenAIFails()
    {
        var userId = Guid.NewGuid();
        var persistence = new InMemoryChatPersistence();
        var aiService = new ThrowingChatAIService();
        var userContext = new MockUserContext(userId);
        var service = new ChatMessageService(persistence, aiService, userContext, NullLogger<ChatMessageService>.Instance);
        var command = new QueueMessageCommand("need help");

        var results = new List<StreamChunkDto>();
        await foreach (StreamChunkDto chunk in service.QueueMessageStreamAsync(command))
        {
            results.Add(chunk);
        }

        Assert.Equal(2, results.Count); // 1 fallback text + 1 final blank
        Assert.Equal("I'm having trouble responding right now, but I'm here with you. Want to try again?", results[0].t);
        Assert.True(results[1].f);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
        Assert.Equal("I'm having trouble responding right now, but I'm here with you. Want to try again?", persistence.Messages[1].Content);
    }

    private sealed class InMemoryChatPersistence : IChatPersistence
    {
        public List<User> Users { get; } = [];
        public List<Conversation> Conversations { get; } = [];
        public List<Message> Messages { get; } = [];

        public Task<ConversationContext> GetConversationContextAsync(Guid userId, int limit, CancellationToken ct)
        {
            Conversation? conversation = Conversations
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            List<Message> history = conversation != null
                ? [.. Messages.Where(x => x.ConversationId == conversation.Id).TakeLast(limit)]
                : [];

            return Task.FromResult(new ConversationContext(conversation, history, "es"));
        }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void AddConversation(Conversation conversation)
        {
            Conversations.Add(conversation);
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubChatAIService(string[] chunks) : IChatAIService
    {
        public async IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            foreach (string chunk in chunks)
            {
                await Task.Yield();
                yield return chunk;
            }
        }
    }

    private sealed class ThrowingChatAIService : IChatAIService
    {
        public async IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            await Task.Yield();
            throw new InvalidOperationException("AI provider unavailable");
#pragma warning disable CS0162 // Unreachable code detected
            yield break;
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
