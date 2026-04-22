using System.Net;
using System.Net.Http.Json;

using Sery.API.Contracts.Conversations;

namespace Sery.API.IntegrationTests;

public sealed class ConversationEndpointTests : IClassFixture<ChatWebApplicationFactory>
{
    private readonly HttpClient client;

    public ConversationEndpointTests(ChatWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetConversations_ShouldReturnOk_WhenAuthenticated()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/v1/conversations");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetConversationMessages_ShouldReturnNotFound_WhenConversationDoesNotExist()
    {
        Guid id = Guid.NewGuid();
        using HttpRequestMessage request = new(HttpMethod.Get, $"/api/v1/conversations/{id}/messages");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateConversation_ShouldReturnOk_WhenConversationExists()
    {
        Guid id = Guid.Parse("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38");
        using HttpRequestMessage request = new(HttpMethod.Patch, $"/api/v1/conversations/{id}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
        request.Content = JsonContent.Create(new UpdateConversationRequest
        {
            Title = "Plan para bajar mi ansiedad esta semana",
            IsPinned = true
        });

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
