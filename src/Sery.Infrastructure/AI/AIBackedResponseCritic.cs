using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.AI;

public sealed class AIBackedResponseCritic(
    IChatAIService chatAIService,
    ILogger<AIBackedResponseCritic> logger) : IResponseCritic
{
    public async Task<ResponseCritiqueResult> CritiqueAsync(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState,
        string draftResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            List<ChatMessageDto> messages = BuildMessages(context, userMessage, turnState, draftResponse);
            var builder = new StringBuilder();

            await foreach (string chunk in chatAIService.GenerateStreamAsync(messages, cancellationToken))
            {
                builder.Append(chunk);
            }

            ResponseCritiqueResult? parsed = TryParse(context, userMessage, builder.ToString(), draftResponse);
            if (parsed is not null)
            {
                return parsed;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI response critic failed. Falling back to heuristic critic.");
        }

        return BuildFallback(context, userMessage, turnState, draftResponse);
    }

    private static List<ChatMessageDto> BuildMessages(
        ConversationContext context,
        string userMessage,
        TurnStateInterpretation turnState,
        string draftResponse)
    {
        string recentHistory = string.Join(
            "\n",
            context.History.TakeLast(12).Select(m => $"{MapRole(m.Role)}: {m.Content}"));

        string prompt =
            """
            You are a response critic for conversational continuity.
            Evaluate whether the assistant draft properly answers the latest user message.
            Return JSON only with this shape:
            {
              "shouldRewrite": true|false,
              "confidence": "low|medium|high",
              "rationale": "short explanation",
              "rewrittenResponse": "final assistant response"
            }

            Rewrite when the draft:
            - reopens a problem the user already resolved,
            - ignores the latest user message,
            - repeats the previous support script,
            - sounds melodramatic, inflated, or generic,
            - asks a follow-up that no longer fits,
            - includes internal meta-output, adaptation notes, or parenthetical strategy comments,
            - invents unsupported time references or events such as saying "ayer" when the visible conversation does not say that.

            Keep rewritten responses natural, short when closure is better, and aligned to the latest user turn.
            Output valid JSON only.
            """;

        string userPayload =
            $"""
            Recent history:
            {recentHistory}

            Latest user message:
            {userMessage}

            Turn state:
            resolved={turnState.UserSeemsResolved}, closing={turnState.UserIsClosingConversation}, avoidReopening={turnState.ShouldAvoidReopening}, topicShift={turnState.TopicShiftLikely}, wantsLightClosure={turnState.WantsLightClosure}

            Assistant draft:
            {draftResponse}
            """;

        return
        [
            new ChatMessageDto("system", prompt),
            new ChatMessageDto("user", userPayload)
        ];
    }

    private static ResponseCritiqueResult? TryParse(
        ConversationContext context,
        string userMessage,
        string raw,
        string draftResponse)
    {
        string json = ExtractJson(raw);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        CriticPayload? payload = JsonSerializer.Deserialize<CriticPayload>(json);
        if (payload is null)
        {
            return null;
        }

        string finalResponse = string.IsNullOrWhiteSpace(payload.RewrittenResponse)
            ? draftResponse
            : payload.RewrittenResponse.Trim();
        finalResponse = SanitizeFinalResponse(context, userMessage, finalResponse, raw);

        bool shouldRewrite = payload.ShouldRewrite;
        string confidence = string.IsNullOrWhiteSpace(payload.Confidence)
            ? "low"
            : payload.Confidence.Trim().ToLowerInvariant();

        // Soft mode: low-confidence rewrites are not trustworthy enough to override the draft.
        if (shouldRewrite && confidence == "low")
        {
            shouldRewrite = false;
            finalResponse = SanitizeFinalResponse(context, userMessage, draftResponse, raw);
        }

        return new ResponseCritiqueResult(
            shouldRewrite,
            finalResponse,
            confidence,
            payload.Rationale?.Trim() ?? string.Empty);
    }

    private static string ExtractJson(string raw)
    {
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        return start >= 0 && end > start ? raw[start..(end + 1)] : string.Empty;
    }

    private static ResponseCritiqueResult BuildFallback(
        ConversationContext context,
        string userMessage,
        TurnStateInterpretation turnState,
        string draftResponse)
    {
        string sanitized = SanitizeFinalResponse(context, userMessage, draftResponse, draftResponse);
        string normalized = TextHeuristics.Normalize(sanitized);
        bool seemsReopening = turnState.ShouldAvoidReopening &&
            TextHeuristics.ContainsAny(normalized,
            [
                "que te preocupa",
                "que pasa",
                "que esta pasando",
                "que te inquieta",
                "cuentame mas",
                "puedes contarme mas",
                "hay algo en concreto",
                "hay alguna cosa en concreto",
                "quieres contarme",
                "si quieres hablar sobre eso"
            ]);

        if (!seemsReopening)
        {
            return new ResponseCritiqueResult(false, sanitized, "low", "heuristic fallback accepted draft");
        }

        string rewritten = turnState.UserSeemsResolved || turnState.WantsLightClosure
            ? "Me alegra que ya se te haya pasado. Gracias por contármelo; si luego quieres seguir hablando, aquí estoy."
            : sanitized;

        return new ResponseCritiqueResult(true, rewritten, "low", "heuristic fallback rewrote reopening draft");
    }

    private static string SanitizeFinalResponse(
        ConversationContext context,
        string userMessage,
        string response,
        string rawCriticOutput)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return string.Empty;
        }

        string sanitized = response.Trim();
        string lower = sanitized.ToLowerInvariant();

        int markerIndex = lower.IndexOf("(respuesta adaptada", StringComparison.Ordinal);
        if (markerIndex >= 0)
        {
            sanitized = sanitized[..markerIndex].TrimEnd();
        }

        markerIndex = lower.IndexOf("(adapted response", StringComparison.Ordinal);
        if (markerIndex >= 0)
        {
            sanitized = sanitized[..markerIndex].TrimEnd();
        }

        sanitized = string.Join(
            Environment.NewLine,
            sanitized
                .Split(Environment.NewLine)
                .Where(line =>
                {
                    string normalized = TextHeuristics.Normalize(line);
                    return !TextHeuristics.ContainsAny(normalized,
                    [
                        "respuesta adaptada",
                        "adapted response",
                        "curiosidad aumentada",
                        "pregunta",
                        "strategy note",
                        "internal note"
                    ]);
                }))
            .Trim();

        if (!ConversationSupportsExplicitPastReference(context, userMessage))
        {
            sanitized = RemoveUnsupportedPastReferences(sanitized);
        }

        if (rawCriticOutput.IndexOf("\"rewrittenResponse\"", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return sanitized;
        }

        return sanitized;
    }

    private static bool ConversationSupportsExplicitPastReference(ConversationContext context, string userMessage)
    {
        string combined = string.Join(
            " ",
            context.History.Select(x => x.Content).Append(userMessage));

        string normalized = TextHeuristics.Normalize(combined);
        return TextHeuristics.ContainsAny(normalized,
        [
            "ayer",
            "anoche",
            "hace rato",
            "hace unas horas",
            "antes",
            "earlier",
            "yesterday",
            "last night"
        ]);
    }

    private static string RemoveUnsupportedPastReferences(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string sanitized = text
            .Replace("ayer pasó algo y ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("ayer paso algo y ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("entiendo que ayer pasó algo y ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("entiendo que ayer paso algo y ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("ayer", "antes", StringComparison.OrdinalIgnoreCase);

        return sanitized.Trim();
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

    private sealed class CriticPayload
    {
        public bool ShouldRewrite { get; init; }
        public string? Confidence { get; init; }
        public string? Rationale { get; init; }
        public string? RewrittenResponse { get; init; }
    }
}
