namespace Sery.Application.Chat;

public interface IChatAIService
{
    Task<string> GenerateResponseAsync(List<ChatMessageDto> messages, CancellationToken ct);
}
