using Microsoft.Extensions.DependencyInjection;
using Sery.Application.AgentCustomization;
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

    [Fact]
    public void AddApplication_ShouldRegisterRetentionPlanner_WhenCalled()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        ServiceDescriptor? descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IConversationRetentionPlanner));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddApplication_ShouldRegisterAgentCustomizationService_WhenCalled()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        ServiceDescriptor? descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAgentCustomizationService));

        Assert.NotNull(descriptor);
    }
}
