using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.AI;

public sealed class AIBackedTurnStateInterpreter(
    IChatAIService chatAIService,
    ILogger<AIBackedTurnStateInterpreter> logger) : ITurnStateInterpreter
{
    public async Task<TurnStateInterpretation> InterpretAsync(
        ConversationContext context,
        string userMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ChatMessageDto> messages = BuildMessages(context, userMessage);
            var builder = new StringBuilder();

            await foreach (string chunk in chatAIService.GenerateStreamAsync(messages, cancellationToken))
            {
                builder.Append(chunk);
            }

            TurnStateInterpretation? parsed = TryParse(builder.ToString());
            if (parsed is not null)
            {
                return parsed;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI turn-state interpretation failed. Falling back to heuristics.");
        }

        return BuildFallback(userMessage);
    }

    private static List<ChatMessageDto> BuildMessages(ConversationContext context, string userMessage)
    {
        string recentHistory = string.Join(
            "\n",
            context.History.TakeLast(12).Select(m => $"{MapRole(m.Role)}: {m.Content}"));

        string prompt =
            """
            You are a classifier for chat continuity.
            Read the recent conversation and the latest user message.
            Return JSON only with this shape:
            {
              "userSeemsResolved": true|false,
              "userIsClosingConversation": true|false,
              "shouldAvoidReopening": true|false,
              "topicShiftLikely": true|false,
              "wantsLightClosure": true|false,
              "confidence": "low|medium|high",
              "rationale": "short explanation"
            }

            Rules:
            - Mark userSeemsResolved true if the user indicates they already calmed down, feel better, moved on, or no longer need help.
            - Mark userIsClosingConversation true if the user is giving thanks, wrapping up, or ending the exchange.
            - Mark shouldAvoidReopening true if the assistant should NOT ask them to explain the same problem again.
            - Mark topicShiftLikely true if the latest user message seems to move away from the previous unresolved topic.
            - Mark wantsLightClosure true if a short natural wrap-up is better than a deeper follow-up.
            - Be tolerant of typos, slang, missing accents, and informal wording.
            - Focus on the latest user message first, but use recent history for context.
            - Output valid JSON only. No markdown.
            """;

        string userPayload =
            $"""
            Recent history:
            {recentHistory}

            Latest user message:
            {userMessage}
            """;

        return
        [
            new ChatMessageDto("system", prompt),
            new ChatMessageDto("user", userPayload)
        ];
    }

    private static TurnStateInterpretation? TryParse(string raw)
    {
        string json = ExtractJson(raw);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        TurnStatePayload? payload = JsonSerializer.Deserialize<TurnStatePayload>(json);
        if (payload is null)
        {
            return null;
        }

        return new TurnStateInterpretation(
            payload.UserSeemsResolved,
            payload.UserIsClosingConversation,
            payload.ShouldAvoidReopening,
            payload.TopicShiftLikely,
            payload.WantsLightClosure,
            string.IsNullOrWhiteSpace(payload.Confidence) ? "low" : payload.Confidence.Trim().ToLowerInvariant(),
            payload.Rationale?.Trim() ?? string.Empty);
    }

    private static string ExtractJson(string raw)
    {
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw[start..(end + 1)] : string.Empty;
    }

    private static TurnStateInterpretation BuildFallback(string userMessage)
    {
        string normalized = TextHeuristics.Normalize(userMessage);
        string[] resolvedSignals =
        [
            "ya se me tranquil",
            "ya me calme",
            "ya se me calmo",
            "ya estoy mejor",
            "ya se me paso",
            "todo bien",
            "estoy bien",
            "gracias"
        ];

        bool resolved = resolvedSignals.Any(x => normalized.Contains(TextHeuristics.Normalize(x), StringComparison.Ordinal));
        bool closing = resolved || normalized.EndsWith("gracias", StringComparison.Ordinal);
        bool lightClosure = closing || TextHeuristics.ContainsAny(normalized, ["todo bien", "ok gracias", "vale gracias", "ya estoy bien"]);

        return new TurnStateInterpretation(
            resolved,
            closing,
            resolved || closing,
            false,
            lightClosure,
            "low",
            "heuristic fallback");
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

    private sealed class TurnStatePayload
    {
        public bool UserSeemsResolved { get; init; }
        public bool UserIsClosingConversation { get; init; }
        public bool ShouldAvoidReopening { get; init; }
        public bool TopicShiftLikely { get; init; }
        public bool WantsLightClosure { get; init; }
        public string? Confidence { get; init; }
        public string? Rationale { get; init; }
    }
}
