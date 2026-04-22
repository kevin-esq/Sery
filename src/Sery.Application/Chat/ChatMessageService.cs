using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Sery.Application.Common.Interfaces;
using Sery.Application.Conversations;
using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public sealed class ChatMessageService(
    IChatPersistence chatPersistence,
    IChatAIService chatAIService,
    IChatPromptComposer chatPromptComposer,
    IAgentPersonaProvider agentPersonaProvider,
    ITurnAnalyzer turnAnalyzer,
    ITurnStateInterpreter turnStateInterpreter,
    IMemoryOrchestrator memoryOrchestrator,
    IUserFactExtractor userFactExtractor,
    IAdaptivePacingPolicy adaptivePacingPolicy,
    IConversationRetentionPlanner retentionPlanner,
    IResponsePolicyEngine responsePolicyEngine,
    IHookEngine hookEngine,
    IResponseCritic responseCritic,
    IChatEfficacyProfiler chatEfficacyProfiler,
    IUserContext userContext,
    IConversationInsightsService conversationInsightsService,
    ILogger<ChatMessageService> logger) : IChatMessageService
{
    private const int ConversationContextLimit = 30;
    private const int ConversationTimelineCompressionLimit = 40;
    private const int EmotionalMemoryWindow = 24;
    private const string FallbackAssistantMessage =
        "I'm having trouble responding right now, but I'm here with you. Want to try again?";

    public async IAsyncEnumerable<StreamChunkDto> QueueMessageStreamAsync(
        QueueMessageCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Guid userId = userContext.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");

        ConversationContext context = await chatPersistence.GetConversationContextAsync(
            userId,
            command.ConversationId,
            ConversationContextLimit,
            cancellationToken);

        Conversation conversation = context.Conversation ?? new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (context.Conversation is null)
        {
            chatPersistence.AddConversation(conversation);
        }
        else if (conversation.IsArchived)
        {
            conversation.IsArchived = false;
        }

        var userMsg = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = command.Message.Trim()
        };
        chatPersistence.AddMessage(userMsg);
        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.LastMessageAt = userMsg.CreatedAt;
        await chatPersistence.SaveChangesAsync(cancellationToken);

        AgentPersonaProfile persona = AgentPersonaCustomizationMerger.Merge(
            agentPersonaProvider.GetPersona(),
            context.AgentCustomization);
        AgentAdaptationProfile adaptation = agentPersonaProvider.GetAdaptation();
        TurnAnalysis turnAnalysis = turnAnalyzer.Analyze(context, userMsg.Content);
        TurnStateInterpretation turnState = await turnStateInterpreter.InterpretAsync(
            context,
            userMsg.Content,
            cancellationToken);
        MemoryOrchestrationResult memoryPlan = memoryOrchestrator.CreatePlan(
            context,
            userMsg.Content,
            turnAnalysis,
            turnState);
        IReadOnlyList<Message> relevantHistory = memoryPlan.ReadPlan.IncludeRelevantMessages
            ? await chatPersistence.GetRelevantMessagesAsync(
                userId,
                conversation.Id,
                userMsg.Content,
                memoryPlan.ReadPlan.RelevantMessageLimit,
                cancellationToken)
            : [];
        IReadOnlyList<UserMemoryFact> relevantFacts = memoryPlan.ReadPlan.IncludeRelevantFacts
            ? await chatPersistence.GetRelevantFactsAsync(
                userId,
                userMsg.Content,
                memoryPlan.ReadPlan.RelevantFactLimit,
                cancellationToken)
            : [];
        context = context with
        {
            RelevantHistory = relevantHistory,
            RelevantFacts = relevantFacts
        };
        AdaptivePacingPlan adaptivePacingPlan =
            adaptivePacingPolicy.CreatePlan(context, userMsg.Content, turnAnalysis, persona, adaptation);
        RetentionPromptPlan retentionPromptPlan =
            retentionPlanner.CreatePlan(context, userMsg.Content, turnAnalysis, adaptivePacingPlan, persona, adaptation);
        ResponsePolicyDecision responsePolicy = responsePolicyEngine.BuildPolicy(
            turnAnalysis,
            turnState,
            adaptivePacingPlan,
            retentionPromptPlan);
        adaptivePacingPlan = ApplyResponsePolicy(adaptivePacingPlan, responsePolicy);
        retentionPromptPlan = ApplyResponsePolicy(retentionPromptPlan, responsePolicy);
        IReadOnlyList<RetentionHook> hooks =
            hookEngine.BuildHooks(context, userMsg.Content, turnAnalysis, retentionPromptPlan.Mode, adaptivePacingPlan, persona, adaptation);

        var promptContext = new ChatPromptContext(
            context,
            userMsg.Content,
            persona,
            adaptation,
            turnAnalysis,
            turnState,
            memoryPlan,
            responsePolicy,
            adaptivePacingPlan,
            retentionPromptPlan,
            hooks);

        string systemPrompt = chatPromptComposer.ComposePrompt(promptContext);

        var aiMessages = new List<ChatMessageDto>
        {
            new("system", systemPrompt)
        };
        aiMessages.AddRange(context.History
            .TakeLast(memoryPlan.ReadPlan.HistoryWindow)
            .Select(m => new ChatMessageDto(MapRole(m.Role), m.Content)));
        aiMessages.Add(new("user", userMsg.Content));

        string? responseToEmit = null;
        try
        {
            string draftResponse = await GenerateDraftResponseAsync(aiMessages, cancellationToken);
            ResponseCritiqueResult critique = await responseCritic.CritiqueAsync(
                context,
                userMsg.Content,
                turnAnalysis,
                turnState,
                draftResponse,
                cancellationToken);

            responseToEmit = string.IsNullOrWhiteSpace(critique.FinalResponse)
                ? draftResponse
                : critique.FinalResponse.Trim();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Response generation canceled by the user.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error when contacting the AI service.");
            responseToEmit = FallbackAssistantMessage;
        }

        if (!string.IsNullOrWhiteSpace(responseToEmit))
        {
            foreach (string chunk in ChunkResponse(responseToEmit))
            {
                yield return new StreamChunkDto(chunk, false, conversation.Id);
            }

            await PersistAssistantResponseAsync(
                userId,
                context,
                conversation,
                responseToEmit,
                userMsg.Content,
                turnAnalysis,
                memoryPlan,
                retentionPromptPlan,
                adaptivePacingPlan,
                userFactExtractor,
                cancellationToken);
        }

        yield return new StreamChunkDto(string.Empty, true, conversation.Id);
    }

    private static string MapRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "assistant",
            _ => "user"
        };
    }

    private async Task<string> GenerateDraftResponseAsync(
        List<ChatMessageDto> aiMessages,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        await foreach (string token in chatAIService.GenerateStreamAsync(aiMessages, cancellationToken))
        {
            builder.Append(token);
        }

        return builder.ToString();
    }

    private static IEnumerable<string> ChunkResponse(string response)
    {
        const int targetSize = 80;
        if (string.IsNullOrWhiteSpace(response))
        {
            yield break;
        }

        var builder = new StringBuilder();
        foreach (string word in response.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (builder.Length + word.Length + 1 > targetSize && builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(word);
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }

    private async Task PersistAssistantResponseAsync(
        Guid userId,
        ConversationContext context,
        Conversation conversation,
        string finalResponse,
        string userMessage,
        TurnAnalysis turnAnalysis,
        MemoryOrchestrationResult memoryPlan,
        RetentionPromptPlan retentionPromptPlan,
        AdaptivePacingPlan adaptivePacingPlan,
        IUserFactExtractor userFactExtractor,
        CancellationToken cancellationToken)
    {
        var assistantMsg = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = finalResponse
        };
        chatPersistence.AddMessage(assistantMsg);

        conversation.UpdatedAt = DateTime.UtcNow;
        conversation.LastMessageAt = assistantMsg.CreatedAt;

        IReadOnlyList<Message> timeline = await chatPersistence.GetConversationTimelineAsync(
            conversation.Id,
            ConversationTimelineCompressionLimit,
            cancellationToken);

        ConversationInsightsDto conversationInsights = memoryPlan.WritePlan.UpdateConversationSummary
            ? conversationInsightsService.BuildConversationInsights(conversation, timeline)
            : new ConversationInsightsDto(
                conversation.Title,
                conversation.IsTitleGenerated,
                conversation.Summary,
                conversation.UpdatedAt,
                conversation.LastMessageAt);

        if (string.IsNullOrWhiteSpace(conversation.Title) || conversation.IsTitleGenerated)
        {
            conversation.Title = conversationInsights.Title;
            conversation.IsTitleGenerated = conversationInsights.IsTitleGenerated;
        }

        conversation.Summary = conversationInsights.Summary;
        conversation.UpdatedAt = conversationInsights.UpdatedAt;
        conversation.LastMessageAt = conversationInsights.LastMessageAt;

        IReadOnlyList<Message> recentUserMessages = await chatPersistence.GetRecentUserMessagesAsync(
            userId,
            EmotionalMemoryWindow,
            cancellationToken);

        if (memoryPlan.WritePlan.UpdateEmotionalMemory)
        {
            EmotionalMemoryProfileDto emotionalMemoryProfile =
                conversationInsightsService.BuildEmotionalMemoryProfile(context.EmotionalMemory, recentUserMessages);

            UserEmotionalMemory emotionalMemory = context.EmotionalMemory ?? new UserEmotionalMemory
            {
                UserId = userId
            };

            emotionalMemory.Summary = emotionalMemoryProfile.Summary;
            emotionalMemory.DominantEmotion = emotionalMemoryProfile.DominantEmotion;
            emotionalMemory.UpdatedAt = emotionalMemoryProfile.UpdatedAt;

            if (context.EmotionalMemory is null)
            {
                chatPersistence.AddUserEmotionalMemory(emotionalMemory);
            }
        }

        ChatEfficacyProfileSnapshot? currentEfficacyProfile = context.EfficacyProfile is null
            ? null
            : new ChatEfficacyProfileSnapshot(
                context.EfficacyProfile.PreferredConversationMode,
                context.EfficacyProfile.PreferredResponseLength,
                context.EfficacyProfile.PreferredQuestionStyle,
                context.EfficacyProfile.PreferredActionStyle,
                context.EfficacyProfile.PreferredPacing,
                context.EfficacyProfile.SuccessfulStrategiesSummary,
                context.EfficacyProfile.UpdatedAt);

        if (memoryPlan.WritePlan.UpdateEfficacyProfile)
        {
            ChatEfficacyProfileSnapshot efficacySnapshot = chatEfficacyProfiler.BuildProfile(
                recentUserMessages.Select(x => x.Content).ToList(),
                turnAnalysis,
                retentionPromptPlan.Mode,
                adaptivePacingPlan,
                currentEfficacyProfile);

            UserChatEfficacyProfile efficacyProfile = context.EfficacyProfile ?? new UserChatEfficacyProfile
            {
                UserId = userId
            };

            efficacyProfile.PreferredConversationMode = efficacySnapshot.PreferredConversationMode;
            efficacyProfile.PreferredResponseLength = efficacySnapshot.PreferredResponseLength;
            efficacyProfile.PreferredQuestionStyle = efficacySnapshot.PreferredQuestionStyle;
            efficacyProfile.PreferredActionStyle = efficacySnapshot.PreferredActionStyle;
            efficacyProfile.PreferredPacing = efficacySnapshot.PreferredPacing;
            efficacyProfile.SuccessfulStrategiesSummary = efficacySnapshot.SuccessfulStrategiesSummary;
            efficacyProfile.UpdatedAt = efficacySnapshot.UpdatedAt;

            if (context.EfficacyProfile is null)
            {
                chatPersistence.AddUserChatEfficacyProfile(efficacyProfile);
            }
        }

        IReadOnlyList<UserFactExtraction> extractedFacts = userFactExtractor.Extract(context, userMessage, turnAnalysis);
        foreach (UserFactExtraction extractedFact in extractedFacts)
        {
            UserMemoryFact? existingFact = await chatPersistence.FindUserMemoryFactByKeyAsync(
                userId,
                extractedFact.NormalizedKey,
                cancellationToken);

            if (existingFact is null)
            {
                chatPersistence.AddUserMemoryFact(new UserMemoryFact
                {
                    UserId = userId,
                    SourceConversationId = conversation.Id,
                    Category = extractedFact.Category,
                    Content = extractedFact.Content,
                    NormalizedKey = extractedFact.NormalizedKey,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                });
            }
            else
            {
                existingFact.Category = extractedFact.Category;
                existingFact.Content = extractedFact.Content;
                existingFact.SourceConversationId = conversation.Id;
                existingFact.UpdatedAt = DateTime.UtcNow;
                existingFact.LastSeenAt = DateTime.UtcNow;
            }
        }

        await chatPersistence.SaveChangesAsync(cancellationToken);
    }

    private static AdaptivePacingPlan ApplyResponsePolicy(
        AdaptivePacingPlan current,
        ResponsePolicyDecision policy)
    {
        return current with
        {
            PreferredResponseLength = policy.PreferredResponseLength,
            QuestionBudget = policy.ShouldAskFollowUpQuestion ? Math.Max(1, current.QuestionBudget) : 0,
            ClosingCadence = policy.PreferBriefClosure ? "close briefly and naturally" : current.ClosingCadence
        };
    }

    private static RetentionPromptPlan ApplyResponsePolicy(
        RetentionPromptPlan current,
        ResponsePolicyDecision policy)
    {
        return current with
        {
            Mode = policy.Mode,
            ClosingInstruction = policy.PreferBriefClosure
                ? "Close naturally in a brief way and do not reopen the topic."
                : policy.AvoidReopening
                    ? $"{current.ClosingInstruction} Do not reopen already-resolved concerns."
                    : current.ClosingInstruction
        };
    }
}
