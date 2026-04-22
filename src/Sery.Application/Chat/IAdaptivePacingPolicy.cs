namespace Sery.Application.Chat;

public interface IAdaptivePacingPolicy
{
    AdaptivePacingPlan CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation);
}
