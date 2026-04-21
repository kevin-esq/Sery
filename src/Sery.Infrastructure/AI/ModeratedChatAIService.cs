using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI;

public sealed class ModeratedChatAIService(
    IChatAIService innerService,
    IOptions<ChatAIOptions> options,
    ILogger<ModeratedChatAIService> logger) : IChatAIService
{
    private readonly string[] _bannedWords = options.Value.BannedWords ?? [];

    public IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, CancellationToken ct)
    {
        return innerService.GenerateStreamAsync(messages, ct);
    }
}
