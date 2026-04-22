namespace Sery.Application.Chat;

public interface IMemoryOrchestrator
{
    MemoryOrchestrationResult CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState);
}
