using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sery.API.IntegrationTests;

public class HealthEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenEndpointIsAvailable()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SendMessage_ShouldReturnAccepted_WhenPayloadIsValid()
    {
        HttpClient client = factory.CreateClient();
        var request = new
        {
            userId = Guid.NewGuid(),
            message = "Hello from integration test"
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/message", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("queued", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task SendMessage_ShouldReturnInternalErrorCode_WhenPayloadIsInvalid()
    {
        HttpClient client = factory.CreateClient();
        var request = new
        {
            userId = Guid.Empty,
            message = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/message", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("SERY-API-400-002", body.GetProperty("code").GetString());
        Assert.Equal("error.chat.required_userid_message", body.GetProperty("messageKey").GetString());
        Assert.Equal("Validation failed.", body.GetProperty("title").GetString());
        Assert.Equal("UserId and Message are required.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task SendMessage_ShouldReturnLocalizedProblemDetails_WhenAcceptLanguageIsSpanish()
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es");

        var request = new
        {
            userId = Guid.Empty,
            message = " "
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/chat/message", request);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("SERY-API-400-002", body.GetProperty("code").GetString());
        Assert.Equal("error.chat.required_userid_message", body.GetProperty("messageKey").GetString());
        Assert.Equal("La validación falló.", body.GetProperty("title").GetString());
        Assert.Equal("Se requieren UserId y Message.", body.GetProperty("detail").GetString());
    }
}
