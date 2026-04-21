using Sery.Application.Chat;

namespace Sery.API.IntegrationTests;

/// <summary>
/// Avoids real PostgreSQL and OpenAI during integration tests (CI has no DB/API keys).
/// </summary>
internal sealed class StubChatMessageService : IChatMessageService
{
    public async IAsyncEnumerable<StreamChunkDto> QueueMessageStreamAsync(
        QueueMessageCommand command,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        await Task.Yield();
        yield return new StreamChunkDto("Hello ", false, id);

        await Task.Yield();
        yield return new StreamChunkDto("from integration stub", false, id);

        await Task.Yield();
        yield return new StreamChunkDto(string.Empty, true, id);
    }

}
