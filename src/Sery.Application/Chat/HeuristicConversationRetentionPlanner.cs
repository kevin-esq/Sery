namespace Sery.Application.Chat;

public sealed class HeuristicConversationRetentionPlanner : IConversationRetentionPlanner
{
    public RetentionPromptPlan CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        AdaptivePacingPlan adaptivePacingPlan,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation)
    {
        ConversationMode mode = ResolveMode(turnAnalysis, persona, adaptation);

        return mode switch
        {
            ConversationMode.CrisisSafe => new RetentionPromptPlan(
                mode,
                "Start by stabilizing the emotional situation and naming the urgency with calm precision.",
                "Create safety, reduce overwhelm, and avoid intensity escalation.",
                "Use short, grounded sentences with no unnecessary flourish.",
                "Close with one immediate, safe next step or one grounding question."),
            ConversationMode.Grounding => new RetentionPromptPlan(
                mode,
                "Open with a concise validation and a grounding-oriented observation.",
                "Lower emotional activation while preserving the feeling of being understood.",
                "Keep rhythm slow, simple, and easy to absorb.",
                "Close with one very small practical step or one centering question."),
            ConversationMode.Reflective => new RetentionPromptPlan(
                mode,
                "Open by reflecting the user's inner conflict or tension with nuance.",
                "Help the user feel deeply understood and invite self-discovery.",
                "Use medium-length responses with thoughtful structure.",
                "Close with one calibrated question that opens reflection."),
            ConversationMode.Direct => new RetentionPromptPlan(
                mode,
                "Open with direct recognition of the core issue without sounding cold.",
                "Create traction and clarity quickly.",
                "Use concise and practical phrasing.",
                "Close with one concrete next move or decision frame."),
            ConversationMode.Playful => new RetentionPromptPlan(
                mode,
                "Open with warmth and lightness while respecting the user's emotional state.",
                "Keep the exchange alive and engaging without becoming frivolous.",
                "Use agile, natural wording and a lightly playful cadence.",
                "Close with an inviting question or tiny challenge."),
            ConversationMode.Deep => new RetentionPromptPlan(
                mode,
                "Open with a precise emotional or psychological observation.",
                "Generate depth, insight, and a sense of meaningful progress.",
                "Use richer structure and layered reasoning, but stay readable.",
                "Close with a reflective question or distilled insight."),
            _ => new RetentionPromptPlan(
                ConversationMode.Warm,
                "Open with genuine validation and a precise sign that you understood the user's message.",
                "Increase trust, emotional safety, and continuity.",
                "Keep a warm, natural, balanced cadence.",
                "Close with a gentle question or a small practical suggestion.")
        };
    }

    private static ConversationMode ResolveMode(
        TurnAnalysis turnAnalysis,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation)
    {
        if (turnAnalysis.EmotionalIntensity is EmotionalIntensity.Crisis)
        {
            return ConversationMode.CrisisSafe;
        }

        switch (turnAnalysis.UserNeed)
        {
            case UserNeed.Regulation:
                return ConversationMode.Grounding;
            case UserNeed.Reflection when turnAnalysis.SelfDisclosureDepth is SelfDisclosureDepth.High:
                return ConversationMode.Deep;
            case UserNeed.Action when persona.RelationalStyle.RelationshipMode is RelationshipMode.SupportCoach:
            case UserNeed.Action:
                return ConversationMode.Direct;
            case UserNeed.Validation:
            case UserNeed.Clarity:
            case UserNeed.Connection:
                break;
            default:
                var exception = new ArgumentOutOfRangeException();
                exception.HelpLink = null;
                exception.HResult = 0;
                exception.Source = null;
                throw exception;
        }

        switch (persona.RelationalStyle.RelationshipMode)
        {
            case RelationshipMode.Listener:
                return turnAnalysis.IsQuestionHeavy ? ConversationMode.Reflective : ConversationMode.Warm;
            case RelationshipMode.CloseCompanion or RelationshipMode.RomanticCompanion when
                turnAnalysis.ShowsHumor &&
                persona.Playfulness >= 25:
                return ConversationMode.Playful;
        }

        if (turnAnalysis.ShowsHumor && persona.Playfulness >= 35)
        {
            return ConversationMode.Playful;
        }

        if (adaptation.AllowModeShifts && turnAnalysis.IsQuestionHeavy)
        {
            return ConversationMode.Reflective;
        }

        return ConversationMode.Warm;
    }
}
