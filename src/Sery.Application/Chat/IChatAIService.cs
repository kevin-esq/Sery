namespace Sery.Application.Chat;

public interface IChatAIService
{
    IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, CancellationToken ct);
}
