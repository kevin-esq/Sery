namespace Sery.Application.Chat;

public interface IHookEngine
{
    IReadOnlyList<RetentionHook> BuildHooks(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        ConversationMode mode,
        AdaptivePacingPlan adaptivePacingPlan,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation);
}
