using System.Text.RegularExpressions;
using Sery.Domain.Entities;

namespace Sery.Application.Conversations;

public sealed class ConversationInsightsService : IConversationInsightsService
{
    private static readonly (string Emotion, string[] Keywords)[] EmotionSignals =
    [
        ("anxious", ["nervioso", "nerviosa", "ansioso", "ansiosa", "ansiedad", "anxious", "panic", "panic attack"]),
        ("sad", ["triste", "vacío", "vacia", "deprimido", "deprimida", "llorar", "sad", "hopeless"]),
        ("angry", ["enojado", "enojada", "frustrado", "frustrada", "rabia", "angry", "furious"]),
        ("overwhelmed", ["agotado", "agotada", "abrumado", "abrumada", "burnout", "overwhelmed", "estresado", "estresada"]),
        ("calm", ["mejor", "tranquilo", "tranquila", "calm", "peaceful", "aliviado", "aliviada"]),
        ("hopeful", ["esperanza", "hopeful", "motivado", "motivada", "ilusión", "ilusion"])
    ];

    private static readonly (string Topic, string[] Keywords)[] TopicSignals =
    [
        ("work pressure", ["trabajo", "work", "jefe", "deadline", "oficina", "reunión", "meeting"]),
        ("relationships", ["pareja", "novio", "novia", "relationship", "familia", "family", "amigo", "amiga"]),
        ("sleep and rest", ["dormir", "sueño", "sleep", "insomnio", "cansado", "cansada"]),
        ("self-esteem", ["inseguro", "insegura", "culpa", "culpable", "self-esteem", "confianza", "confidence"]),
        ("health anxiety", ["salud", "health", "síntoma", "symptom", "doctor", "dolor"]),
        ("life direction", ["futuro", "career", "propósito", "purpose", "rumbo", "direction"])
    ];

    public ConversationInsightsDto BuildConversationInsights(
        Conversation conversation,
        IReadOnlyList<Message> timeline)
    {
        Message? firstUserMessage = timeline.FirstOrDefault(x => x.Role == MessageRole.User);
        string generatedTitle = string.IsNullOrWhiteSpace(conversation.Title) || conversation.IsTitleGenerated
            ? CreateTitle(firstUserMessage?.Content ?? string.Empty)
            : conversation.Title.Trim();

        string dominantEmotion = DetectDominantEmotion(timeline.Where(x => x.Role == MessageRole.User).Select(x => x.Content));
        string topics = BuildTopicSummary(timeline.Where(x => x.Role == MessageRole.User).Select(x => x.Content));
        string supportStyle = BuildSupportStyleSummary(timeline.Where(x => x.Role == MessageRole.Assistant).Select(x => x.Content));

        string summary = BuildConversationSummary(dominantEmotion, topics, supportStyle);
        DateTime? lastMessageAt = timeline.LastOrDefault()?.CreatedAt;

        return new ConversationInsightsDto(
            generatedTitle,
            true,
            summary,
            DateTime.UtcNow,
            lastMessageAt);
    }

    public EmotionalMemoryProfileDto BuildEmotionalMemoryProfile(
        UserEmotionalMemory? currentMemory,
        IReadOnlyList<Message> recentUserMessages)
    {
        IReadOnlyList<string> contents = recentUserMessages
            .Where(x => x.Role == MessageRole.User)
            .Select(x => x.Content)
            .ToList();

        string dominantEmotion = DetectDominantEmotion(contents);
        string topicSummary = BuildTopicSummary(contents);
        string previousSummary = string.IsNullOrWhiteSpace(currentMemory?.Summary)
            ? string.Empty
            : currentMemory!.Summary.Trim();

        string summary = string.Join(
            " ",
            new[]
            {
                topicSummary == "general emotional support"
                    ? "The user tends to seek grounded emotional support."
                    : $"The user often brings up {topicSummary}.",
                dominantEmotion == "mixed"
                    ? "Their emotional tone varies between sessions."
                    : $"Their recent emotional tone is mostly {dominantEmotion}.",
                "Respond with calm, validating, and practical guidance."
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (!string.IsNullOrWhiteSpace(previousSummary) &&
            !summary.Contains(previousSummary, StringComparison.OrdinalIgnoreCase))
        {
            summary = $"{previousSummary} {summary}".Trim();
        }

        return new EmotionalMemoryProfileDto(summary, dominantEmotion, DateTime.UtcNow);
    }

    private static string CreateTitle(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "New conversation";
        }

        string cleaned = Regex.Replace(content.Trim(), @"\s+", " ");
        cleaned = Regex.Replace(cleaned, @"[^\p{L}\p{N}\s]", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return "New conversation";
        }

        string[] words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Take(6));
    }

    private static string DetectDominantEmotion(IEnumerable<string> contents)
    {
        Dictionary<string, int> scores = EmotionSignals.ToDictionary(x => x.Emotion, _ => 0);

        foreach (string content in contents)
        {
            string lower = content.ToLowerInvariant();
            foreach ((string emotion, string[] keywords) in EmotionSignals)
            {
                if (keywords.Any(lower.Contains))
                {
                    scores[emotion]++;
                }
            }
        }

        KeyValuePair<string, int> best = scores.OrderByDescending(x => x.Value).FirstOrDefault();
        return best.Value == 0 ? "mixed" : best.Key;
    }

    private static string BuildTopicSummary(IEnumerable<string> contents)
    {
        Dictionary<string, int> scores = TopicSignals.ToDictionary(x => x.Topic, _ => 0);

        foreach (string content in contents)
        {
            string lower = content.ToLowerInvariant();
            foreach ((string topic, string[] keywords) in TopicSignals)
            {
                if (keywords.Any(lower.Contains))
                {
                    scores[topic]++;
                }
            }
        }

        string[] topTopics = scores
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Take(2)
            .Select(x => x.Key)
            .ToArray();

        return topTopics.Length == 0 ? "general emotional support" : string.Join(" and ", topTopics);
    }

    private static string BuildSupportStyleSummary(IEnumerable<string> assistantMessages)
    {
        string joined = string.Join(" ", assistantMessages).ToLowerInvariant();

        if (joined.Contains("respira") || joined.Contains("breathe") || joined.Contains("ground"))
        {
            return "grounding and breathing cues";
        }

        if (joined.Contains("paso") || joined.Contains("step"))
        {
            return "step-by-step guidance";
        }

        if (joined.Contains("plan") || joined.Contains("next action"))
        {
            return "small action planning";
        }

        return "empathetic reflection";
    }

    private static string BuildConversationSummary(string dominantEmotion, string topics, string supportStyle)
    {
        return string.Join(
            " ",
            new[]
            {
                topics == "general emotional support"
                    ? "The conversation centers on emotional support and self-regulation."
                    : $"The conversation focuses on {topics}.",
                dominantEmotion == "mixed"
                    ? "The emotional tone has been varied."
                    : $"The user's recent tone has been mostly {dominantEmotion}.",
                $"Helpful responses so far use {supportStyle}."
            });
    }
}
