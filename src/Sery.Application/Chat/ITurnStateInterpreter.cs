namespace Sery.Application.Chat;

public interface ITurnStateInterpreter
{
    Task<TurnStateInterpretation> InterpretAsync(
        ConversationContext context,
        string userMessage,
        CancellationToken cancellationToken);
}
