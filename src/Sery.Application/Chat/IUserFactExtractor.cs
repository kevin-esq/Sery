namespace Sery.Application.Chat;

public interface IUserFactExtractor
{
    IReadOnlyList<UserFactExtraction> Extract(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis);
}
