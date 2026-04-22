namespace Sery.Application.Chat;

public sealed class HeuristicMemoryOrchestrator : IMemoryOrchestrator
{
    public MemoryOrchestrationResult CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState)
    {
        int historyWindow = ResolveHistoryWindow(turnAnalysis, turnState);
        bool includeConversationSummary = context.Conversation is not null;
        bool includeEmotionalMemory = turnAnalysis.UserNeed is not UserNeed.Action || turnAnalysis.ShowsVulnerability;
        bool includeEfficacyMemory = context.EfficacyProfile is not null;

        string focus = turnState.UserSeemsResolved || turnState.UserIsClosingConversation
            ? "Favor continuity and closure over exploration."
            : turnState.TopicShiftLikely
                ? "Treat the turn as a likely topic shift and avoid overfitting to old emotional framing."
                : "Use prior context only when it improves relevance to the latest user message.";

        var readPlan = new MemoryReadPlan(
            historyWindow,
            includeConversationSummary,
            includeEmotionalMemory,
            includeEfficacyMemory,
            true,
            true,
            turnState.UserIsClosingConversation || turnState.WantsLightClosure ? 2 : 5,
            turnState.TopicShiftLikely ? 2 : 5,
            focus);

        bool updateConversationSummary = true;
        bool updateEmotionalMemory = !turnState.UserIsClosingConversation || turnAnalysis.ShowsVulnerability;
        bool updateEfficacyProfile = true;
        bool capturePreferenceSignal = turnState.UserIsClosingConversation || turnAnalysis.WantsDirectAdvice || turnAnalysis.WantsReflection;

        var writePlan = new MemoryWritePlan(
            updateConversationSummary,
            updateEmotionalMemory,
            updateEfficacyProfile,
            capturePreferenceSignal,
            turnState.UserIsClosingConversation
                ? "Preserve closure signal and avoid overweighting it as a new unresolved issue."
                : "Update conversational memory because the turn contains live interaction signal.");

        return new MemoryOrchestrationResult(readPlan, writePlan);
    }

    private static int ResolveHistoryWindow(TurnAnalysis turnAnalysis, TurnStateInterpretation turnState)
    {
        if (turnState.UserIsClosingConversation || turnState.WantsLightClosure)
        {
            return 10;
        }

        if (turnAnalysis.SelfDisclosureDepth is SelfDisclosureDepth.High || turnAnalysis.UserNeed is UserNeed.Reflection)
        {
            return 20;
        }

        if (turnAnalysis.UserNeed is UserNeed.Action)
        {
            return 12;
        }

        return 16;
    }
}
