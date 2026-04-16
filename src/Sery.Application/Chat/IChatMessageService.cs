namespace Sery.Application.Chat;

public interface IChatMessageService
{
    Task<QueueMessageResult> QueueMessageAsync(
        QueueMessageCommand command,
        CancellationToken cancellationToken = default);
}
