using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI;

public sealed class OllamaChatAIService(
    HttpClient httpClient,
    IOptions<ChatAIOptions> options) : IChatAIService
{
    private readonly ChatAIOptions _options = options.Value;
    private static readonly string[] ToneModifiers =
    [
        "Be slightly more direct and practical.",
        "Be more warm and empathetic.",
        "Be a bit more concise and focused.",
        "Be slightly more curious and ask questions."
    ];

    public async Task<string> GenerateResponseAsync(List<ChatMessageDto> messages, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("ChatAI BaseUrl is required for Ollama.");
        }

        httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");

        string toneModifier = ToneModifiers[Random.Shared.Next(ToneModifiers.Length)];
        string requestSystemPrompt =
            $"{_options.SystemPrompt}{Environment.NewLine}{Environment.NewLine}" +
            $"Additional style guidance for this response only: {toneModifier}";

        var request = new OllamaChatRequest(
            _options.Model,
            false,
            [new OllamaApiMessage("system", requestSystemPrompt), .. messages.Select(x => new OllamaApiMessage(x.Role, x.Content))]);

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/chat", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            string err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Ollama request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {err}");
        }

        OllamaChatResponse? body = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: ct);
        string? content = body?.Message?.Content;
        return content?.Trim() ?? string.Empty;
    }

    private sealed record OllamaChatRequest(string Model, bool Stream, List<OllamaApiMessage> Messages);
    private sealed record OllamaApiMessage(string Role, string Content);

    private sealed class OllamaChatResponse
    {
        public OllamaMessagePayload? Message { get; init; }
    }

    private sealed class OllamaMessagePayload
    {
        public string Content { get; init; } = string.Empty;
    }
}
