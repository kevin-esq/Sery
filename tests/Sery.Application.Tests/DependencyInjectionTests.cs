using Microsoft.Extensions.DependencyInjection;
using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterChatService_WhenCalled()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        ServiceDescriptor? descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IChatMessageService));

        Assert.NotNull(descriptor);
    }
}
