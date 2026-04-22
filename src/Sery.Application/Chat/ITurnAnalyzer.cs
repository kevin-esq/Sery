namespace Sery.Application.Chat;

public interface ITurnAnalyzer
{
    TurnAnalysis Analyze(ConversationContext context, string userMessage);
}
