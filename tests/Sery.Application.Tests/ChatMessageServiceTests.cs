using Microsoft.Extensions.Logging.Abstractions;
using Sery.Application.Chat;
using Sery.Application.Common.Interfaces;
using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public class ChatMessageServiceTests
{
    [Fact]
    public async Task QueueMessageStreamAsyncShouldPersistAssistantMessageWhenAIResponseSucceeds()
    {
        var userId = Guid.NewGuid();
        var persistence = new InMemoryChatPersistence();
        var aiService = new StubChatAIService(["Take ", "one small ", "step."]);
        ChatMessageService service = CreateService(persistence, aiService, userId);
        var command = new QueueMessageCommand("  hello sery  ");

        var results = new List<StreamChunkDto>();
        await foreach (StreamChunkDto chunk in service.QueueMessageStreamAsync(command))
        {
            results.Add(chunk);
        }

        Assert.True(results.Count >= 2);
        Assert.True(results.Last().f);
        _ = Assert.Single(persistence.Conversations);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal("hello sery", persistence.Messages[0].Content);
        Assert.Equal(MessageRole.User, persistence.Messages[0].Role);
        Assert.Equal("Take one small step.", persistence.Messages[1].Content);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
        Assert.False(string.IsNullOrWhiteSpace(persistence.Conversations[0].Title));
        Assert.False(string.IsNullOrWhiteSpace(persistence.Conversations[0].Summary));
        Assert.NotNull(persistence.EmotionalMemory);
        Assert.NotNull(persistence.EfficacyProfile);
        Assert.Equal("Warm", persistence.EfficacyProfile!.PreferredConversationMode);
    }

    [Fact]
    public async Task QueueMessageStreamAsyncShouldReturnFallbackAndPersistWhenAIFails()
    {
        var userId = Guid.NewGuid();
        var persistence = new InMemoryChatPersistence();
        var aiService = new ThrowingChatAIService();
        ChatMessageService service = CreateService(persistence, aiService, userId);
        var command = new QueueMessageCommand("need help");

        var results = new List<StreamChunkDto>();
        await foreach (StreamChunkDto chunk in service.QueueMessageStreamAsync(command))
        {
            results.Add(chunk);
        }

        Assert.True(results.Count >= 2);
        Assert.NotEmpty(results.Where(x => !x.f));
        Assert.True(results.Last().f);
        Assert.Equal(2, persistence.Messages.Count);
        Assert.Equal(MessageRole.Assistant, persistence.Messages[1].Role);
        Assert.Equal("I'm having trouble responding right now, but I'm here with you. Want to try again?", persistence.Messages[1].Content);
        Assert.NotNull(persistence.EfficacyProfile);
    }

    private static ChatMessageService CreateService(InMemoryChatPersistence persistence, IChatAIService aiService, Guid userId)
    {
        return new ChatMessageService(
            persistence,
            aiService,
            new StubChatPromptComposer(),
            new StubAgentPersonaProvider(),
            new StubTurnAnalyzer(),
            new StubTurnStateInterpreter(),
            new HeuristicMemoryOrchestrator(),
            new HeuristicUserFactExtractor(),
            new StubAdaptivePacingPolicy(),
            new StubConversationRetentionPlanner(),
            new StubResponsePolicyEngine(),
            new StubHookEngine(),
            new StubResponseCritic(),
            new StubChatEfficacyProfiler(),
            new MockUserContext(userId),
            new ConversationInsightsService(),
            NullLogger<ChatMessageService>.Instance);
    }

    private sealed class MockUserContext(Guid userId) : IUserContext
    {
        public Guid? UserId => userId;
        public Guid TenantId => Guid.Empty;
        public string Email => "test@example.com";
    }

    private sealed class InMemoryChatPersistence : IChatPersistence
    {
        public List<Conversation> Conversations { get; } = [];
        public List<Message> Messages { get; } = [];
        public UserEmotionalMemory? EmotionalMemory { get; private set; }
        public UserChatEfficacyProfile? EfficacyProfile { get; private set; }
        public List<UserMemoryFact> MemoryFacts { get; } = [];

        public Task<ConversationContext> GetConversationContextAsync(
            Guid userId,
            Guid? conversationId,
            int limit,
            CancellationToken ct)
        {
            Conversation? conversation = conversationId is null
                ? null
                : Conversations.FirstOrDefault(x => x.UserId == userId && x.Id == conversationId.Value);

            List<Message> history = conversation is null
                ? []
                : [.. Messages.Where(x => x.ConversationId == conversation.Id).TakeLast(limit)];

            return Task.FromResult(new ConversationContext(conversation, history, "es", EmotionalMemory, EfficacyProfile, null, [], []));
        }

        public Task<IReadOnlyList<Message>> GetConversationTimelineAsync(Guid conversationId, int limit, CancellationToken ct)
        {
            IReadOnlyList<Message> timeline = [.. Messages.Where(x => x.ConversationId == conversationId).TakeLast(limit)];
            return Task.FromResult(timeline);
        }

        public Task<IReadOnlyList<Message>> GetRecentUserMessagesAsync(Guid userId, int limit, CancellationToken ct)
        {
            IReadOnlyList<Message> userMessages = [.. Messages.Where(x => x.Role == MessageRole.User).TakeLast(limit)];
            return Task.FromResult(userMessages);
        }

        public Task<IReadOnlyList<Message>> GetRelevantMessagesAsync(
            Guid userId,
            Guid? currentConversationId,
            string userMessage,
            int limit,
            CancellationToken ct)
        {
            IReadOnlyList<Message> relevant = [];
            return Task.FromResult(relevant);
        }

        public Task<IReadOnlyList<UserMemoryFact>> GetRelevantFactsAsync(
            Guid userId,
            string userMessage,
            int limit,
            CancellationToken ct)
        {
            IReadOnlyList<UserMemoryFact> relevant = [.. MemoryFacts.Take(limit)];
            return Task.FromResult(relevant);
        }

        public Task<UserMemoryFact?> FindUserMemoryFactByKeyAsync(Guid userId, string normalizedKey, CancellationToken ct)
        {
            return Task.FromResult(MemoryFacts.FirstOrDefault(x => x.UserId == userId && x.NormalizedKey == normalizedKey));
        }

        public void AddConversation(Conversation conversation)
        {
            Conversations.Add(conversation);
        }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public void AddUserEmotionalMemory(UserEmotionalMemory emotionalMemory)
        {
            EmotionalMemory = emotionalMemory;
        }

        public void AddUserChatEfficacyProfile(UserChatEfficacyProfile efficacyProfile)
        {
            EfficacyProfile = efficacyProfile;
        }

        public void AddUserMemoryFact(UserMemoryFact fact)
        {
            MemoryFacts.Add(fact);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubChatAIService(string[] chunks) : IChatAIService
    {
        public async IAsyncEnumerable<string> GenerateStreamAsync(
            List<ChatMessageDto> messages,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
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
        public async IAsyncEnumerable<string> GenerateStreamAsync(
            List<ChatMessageDto> messages,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            await Task.Yield();
            throw new InvalidOperationException("AI provider unavailable");
#pragma warning disable CS0162
            yield break;
#pragma warning restore CS0162
        }
    }

    private sealed class StubChatPromptComposer : IChatPromptComposer
    {
        public string ComposePrompt(ChatPromptContext context)
        {
            return $"You are Sery. Always respond in {context.ConversationContext.Language}.";
        }
    }

    private sealed class StubAgentPersonaProvider : IAgentPersonaProvider
    {
        public AgentPersonaProfile GetPersona()
        {
            return new AgentPersonaProfile(
                "Sery",
                "neutral",
                "calm and warm",
                70,
                50,
                80,
                60,
                30,
                70,
                60,
                55,
                "medium",
                true,
                true,
                ["warm", "clear"],
                new RelationalPersonaProfile(
                    RelationshipMode.Friend,
                    70,
                    60,
                    55,
                    0,
                    false,
                    false,
                    true),
                new CompanionSafetyProfile(
                    true,
                    true,
                    true,
                    true,
                    true,
                    true));
        }

        public AgentAdaptationProfile GetAdaptation()
        {
            return new AgentAdaptationProfile(true, true, true, true, true, 60, 30);
        }
    }

    private sealed class StubTurnAnalyzer : ITurnAnalyzer
    {
        public TurnAnalysis Analyze(ConversationContext context, string userMessage)
        {
            return new TurnAnalysis(
                "calm",
                EmotionalIntensity.Low,
                EnergyLevel.Medium,
                FormalityLevel.Neutral,
                SelfDisclosureDepth.Low,
                UserNeed.Connection,
                false,
                false,
                false,
                false,
                false,
                false);
        }
    }

    private sealed class StubAdaptivePacingPolicy : IAdaptivePacingPolicy
    {
        public AdaptivePacingPlan CreatePlan(
            ConversationContext context,
            string userMessage,
            TurnAnalysis turnAnalysis,
            AgentPersonaProfile persona,
            AgentAdaptationProfile adaptation)
        {
            return new AdaptivePacingPlan("medium", "steady", "light", 1, true, false, "natural", "soft", "gentle");
        }
    }

    private sealed class StubTurnStateInterpreter : ITurnStateInterpreter
    {
        public Task<TurnStateInterpretation> InterpretAsync(
            ConversationContext context,
            string userMessage,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new TurnStateInterpretation(false, false, false, false, false, "low", "stub"));
        }
    }

    private sealed class StubConversationRetentionPlanner : IConversationRetentionPlanner
    {
        public RetentionPromptPlan CreatePlan(
            ConversationContext context,
            string userMessage,
            TurnAnalysis turnAnalysis,
            AdaptivePacingPlan adaptivePacingPlan,
            AgentPersonaProfile persona,
            AgentAdaptationProfile adaptation)
        {
            return new RetentionPromptPlan(
                ConversationMode.Warm,
                "Validate briefly.",
                "Increase trust.",
                "Keep it steady.",
                "Close with one gentle question.");
        }
    }

    private sealed class StubHookEngine : IHookEngine
    {
        public IReadOnlyList<RetentionHook> BuildHooks(
            ConversationContext context,
            string userMessage,
            TurnAnalysis turnAnalysis,
            ConversationMode mode,
            AdaptivePacingPlan adaptivePacingPlan,
            AgentPersonaProfile persona,
            AgentAdaptationProfile adaptation)
        {
            return [new RetentionHook("precise-observation", "Show understanding.")];
        }
    }

    private sealed class StubResponsePolicyEngine : IResponsePolicyEngine
    {
        public ResponsePolicyDecision BuildPolicy(
            TurnAnalysis turnAnalysis,
            TurnStateInterpretation turnState,
            AdaptivePacingPlan currentPacingPlan,
            RetentionPromptPlan currentRetentionPlan)
        {
            return new ResponsePolicyDecision(
                currentRetentionPlan.Mode,
                currentPacingPlan.PreferredResponseLength,
                true,
                false,
                false,
                "stub");
        }
    }

    private sealed class StubResponseCritic : IResponseCritic
    {
        public Task<ResponseCritiqueResult> CritiqueAsync(
            ConversationContext context,
            string userMessage,
            TurnAnalysis turnAnalysis,
            TurnStateInterpretation turnState,
            string draftResponse,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResponseCritiqueResult(false, draftResponse, "high", "stub"));
        }
    }

    private sealed class StubChatEfficacyProfiler : IChatEfficacyProfiler
    {
        public ChatEfficacyProfileSnapshot BuildProfile(
            IReadOnlyList<string> recentUserMessages,
            TurnAnalysis latestTurnAnalysis,
            ConversationMode latestMode,
            AdaptivePacingPlan latestPacingPlan,
            ChatEfficacyProfileSnapshot? currentProfile = null)
        {
            return new ChatEfficacyProfileSnapshot(
                latestMode.ToString(),
                latestPacingPlan.PreferredResponseLength,
                "likes one calibrated reflective question",
                "prefer validation before action",
                latestPacingPlan.Tempo,
                "The user currently responds best to warm conversations.",
                DateTime.UtcNow);
        }
    }
}
