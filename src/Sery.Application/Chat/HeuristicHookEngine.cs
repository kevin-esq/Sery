namespace Sery.Application.Chat;

public sealed class HeuristicHookEngine : IHookEngine
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
        var hooks = new List<RetentionHook>
        {
            new("precise-observation", "Include one concise observation that proves you understood what matters most in the user's message.")
        };

        if (!string.IsNullOrWhiteSpace(context.Conversation?.Summary))
        {
            hooks.Add(new RetentionHook(
                "continuity-callback",
                "If naturally relevant, connect the current turn with the ongoing thread so the user feels continuity rather than reset."));
        }

        switch (mode)
        {
            case ConversationMode.Deep or ConversationMode.Reflective:
                hooks.Add(new RetentionHook(
                    "inner-contrast",
                    "Surface one tension, contradiction, or dual pull in the user's experience without sounding clinical."));
                break;
            case ConversationMode.Direct or ConversationMode.Grounding:
                hooks.Add(new RetentionHook(
                    "micro-next-step",
                    "Offer one very small next step that feels doable right now."));
                break;
        }

        if (persona.AskFollowUpQuestions && mode is not ConversationMode.CrisisSafe && adaptivePacingPlan.QuestionBudget > 0)
        {
            hooks.Add(new RetentionHook(
                "calibrated-question",
                "End with at most one question, and only if it deepens the conversation instead of slowing it down."));
        }

        if (persona.Charisma >= 60 && mode is ConversationMode.Warm or ConversationMode.Playful)
        {
            hooks.Add(new RetentionHook(
                "magnetic-phrasing",
                "Use a memorable but natural phrase once, so the response feels distinctive without sounding scripted."));
        }

        if (adaptation.Enabled && adaptation.MirrorUserVerbosity)
        {
            hooks.Add(new RetentionHook(
                "pacing-match",
                "Match the user's conversational density: compact if they are brief, fuller if they bring rich context."));
        }

        if (turnAnalysis.ShowsVulnerability)
        {
            hooks.Add(new RetentionHook(
                "soft-label",
                "Use one gentle emotional label such as 'it sounds like' or 'there is a lot of' to help the user feel understood."));
        }

        if (persona.RelationalStyle.RelationshipMode is RelationshipMode.CloseCompanion or RelationshipMode.RomanticCompanion &&
            !turnAnalysis.ContainsUrgencySignals &&
            persona.RelationalStyle.UsesAffectionateLanguage)
        {
            hooks.Add(new RetentionHook(
                "affectionate-presence",
                "If it fits naturally, use one affectionate but grounded phrase that makes the user feel accompanied without sounding possessive."));
        }

        if (persona.SafetyProfile.EncourageOfflineSupport &&
            turnAnalysis.ContainsUrgencySignals)
        {
            hooks.Add(new RetentionHook(
                "reality-anchor",
                "When the user seems overwhelmed, gently anchor them toward immediate real-world support, grounding, or a safer next step."));
        }

        return hooks;
    }
}
