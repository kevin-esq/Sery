namespace Sery.Application.Chat;

public interface IResponseCritic
{
    Task<ResponseCritiqueResult> CritiqueAsync(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState,
        string draftResponse,
        CancellationToken cancellationToken);
}
