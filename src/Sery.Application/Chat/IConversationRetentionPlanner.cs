namespace Sery.Application.Chat;

public interface IConversationRetentionPlanner
{
    RetentionPromptPlan CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        AdaptivePacingPlan adaptivePacingPlan,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation);
}
