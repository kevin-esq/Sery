using Sery.Application.Chat;

namespace Sery.API.IntegrationTests;

/// <summary>
/// Avoids real PostgreSQL and OpenAI during integration tests (CI has no DB/API keys).
/// </summary>
internal sealed class StubChatMessageService : IChatMessageService
{
    public Task<QueueMessageResult> QueueMessageAsync(
        QueueMessageCommand command,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new QueueMessageResult(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Hello from integration stub"));

}
