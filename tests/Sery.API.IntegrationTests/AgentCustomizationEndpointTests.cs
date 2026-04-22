using System.Net;
using System.Net.Http.Json;
using Sery.API.Contracts.AgentCustomization;

namespace Sery.API.IntegrationTests;

public sealed class AgentCustomizationEndpointTests : IClassFixture<ChatWebApplicationFactory>
{
    private readonly HttpClient client;

    public AgentCustomizationEndpointTests(ChatWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ShouldReturnEffectiveAgentProfile_WhenAuthenticated()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/v1/agent-profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AgentCustomizationResponse? payload = await response.Content.ReadFromJsonAsync<AgentCustomizationResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Sery", payload.AgentName);
    }

    [Fact]
    public async Task Patch_ShouldUpdateAgentProfile_WhenPayloadIsValid()
    {
        using HttpRequestMessage request = new(HttpMethod.Patch, "/api/v1/agent-profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
        request.Content = JsonContent.Create(new UpdateAgentCustomizationRequest
        {
            RelationshipMode = "romantic-companion",
            Warmth = 85,
            UsesAffectionateLanguage = true
        });

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AgentCustomizationResponse? payload = await response.Content.ReadFromJsonAsync<AgentCustomizationResponse>();
        Assert.NotNull(payload);
        Assert.Equal("romantic-companion", payload.RelationshipMode, ignoreCase: true);
        Assert.True(payload.UsesAffectionateLanguage);
        Assert.True(payload.IsCustomized);
    }

    [Fact]
    public async Task Delete_ShouldResetAgentProfile_WhenAuthenticated()
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, "/api/v1/agent-profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
