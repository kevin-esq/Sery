using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI;

public sealed class OpenAIChatAIService(
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
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("ChatAI ApiKey is required.");
        }

        httpClient.BaseAddress = new Uri(_options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        string toneModifier = ToneModifiers[Random.Shared.Next(ToneModifiers.Length)];
        string requestSystemPrompt =
            $"{_options.SystemPrompt}{Environment.NewLine}{Environment.NewLine}" +
            $"Additional style guidance for this response only: {toneModifier}";

        var request = new ChatCompletionsRequest(
            _options.Model,
            [new ApiMessage("system", requestSystemPrompt), .. messages.Select(x => new ApiMessage(x.Role, x.Content))]);

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/chat/completions", request, ct);
        response.EnsureSuccessStatusCode();

        ChatCompletionsResponse? body = await response.Content.ReadFromJsonAsync<ChatCompletionsResponse>(cancellationToken: ct);
        string? content = body?.Choices?.FirstOrDefault()?.Message?.Content;
        return content?.Trim() ?? string.Empty;
    }

    private sealed record ChatCompletionsRequest(string Model, List<ApiMessage> Messages);
    private sealed record ApiMessage(string Role, string Content);

    private sealed class ChatCompletionsResponse
    {
        public List<Choice> Choices { get; init; } = [];
    }

    private sealed class Choice
    {
        public ResponseMessage Message { get; init; } = new();
    }

    private sealed class ResponseMessage
    {
        public string Content { get; init; } = string.Empty;
    }
}
