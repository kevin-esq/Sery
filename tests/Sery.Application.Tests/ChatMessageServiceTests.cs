using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class ChatMessageServiceTests
{
    [Fact]
    public async Task QueueMessageAsync_ShouldReturnQueuedStatus_WhenMessageIsValid()
    {
        var service = new ChatMessageService();
        var command = new QueueMessageCommand(Guid.NewGuid(), "  hello sery  ");

        QueueMessageResult result = await service.QueueMessageAsync(command);

        Assert.Equal("queued", result.Status);
        Assert.Equal(command.UserId, result.UserId);
        Assert.Equal("hello sery", result.Message);
    }
}
