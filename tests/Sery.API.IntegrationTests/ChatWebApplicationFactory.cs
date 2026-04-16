using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sery.Application.Chat;

namespace Sery.API.IntegrationTests;

/// <summary>
/// Registers a stub chat service so chat endpoints can be exercised without external services.
/// </summary>
public sealed class ChatWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatMessageService>();
            services.AddScoped<IChatMessageService, StubChatMessageService>();
        });
    }
}
