namespace Sery.Application.Chat;

public sealed record QueueMessageCommand(Guid UserId, string Message);
