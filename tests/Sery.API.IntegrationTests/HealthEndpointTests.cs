using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sery.API.IntegrationTests;

public class HealthEndpointTests(ChatWebApplicationFactory factory) : IClassFixture<ChatWebApplicationFactory>
{
    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenEndpointIsAvailable()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenUsingCompatibilityVersionedRoute()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SendMessage_ShouldReturnOk_WhenPayloadIsValid()
    {
        HttpClient client = factory.CreateClient();
        var request = new
        {
            message = "Hello from integration test"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/stream", request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("11111111-1111-1111-1111-111111111111", body);
        Assert.Contains("from integration stub", body);
    }

    [Fact]
    public async Task SendMessage_ShouldReturnInternalErrorCode_WhenPayloadIsInvalid()
    {
        HttpClient client = factory.CreateClient();
        var request = new
        {
            message = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/stream", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("SERY-API-400-002", body.GetProperty("code").GetString());
        Assert.Equal("error.chat.required_message", body.GetProperty("messageKey").GetString());
        Assert.Equal("Validation failed.", body.GetProperty("title").GetString());
        Assert.Equal("Message is required.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task SendMessage_ShouldReturnLocalizedProblemDetails_WhenAcceptLanguageIsSpanish()
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es");

        var request = new
        {
            message = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/stream", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("SERY-API-400-002", body.GetProperty("code").GetString());
        Assert.Equal("error.chat.required_message", body.GetProperty("messageKey").GetString());
        Assert.Equal("La validación falló.", body.GetProperty("title").GetString());
        Assert.Equal("El mensaje es obligatorio.", body.GetProperty("detail").GetString());
    }
}
