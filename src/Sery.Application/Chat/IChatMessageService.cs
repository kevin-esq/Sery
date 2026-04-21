namespace Sery.Application.Chat;

public interface IChatMessageService
{
    IAsyncEnumerable<StreamChunkDto> QueueMessageStreamAsync(
        QueueMessageCommand command,
        CancellationToken cancellationToken = default);
}
