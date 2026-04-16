namespace Sery.Application.Chat;

public sealed class ChatMessageService() : IChatMessageService
{
    public Task<QueueMessageResult> QueueMessageAsync(
        QueueMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var sanitizedMessage = command.Message.Trim();
        var result = new QueueMessageResult(command.UserId, sanitizedMessage, "queued");

        return Task.FromResult(result);
    }
}
