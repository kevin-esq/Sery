using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI.Gemini;

public sealed class GeminiChatAIService : IChatAIService
{
    private readonly HttpClient _httpClient;
    private readonly ChatAIOptions _options;

    public GeminiChatAIService(HttpClient httpClient, IOptions<ChatAIOptions> options)
    {
        _options = options.Value;
        _httpClient = httpClient;

        string apiKey = ChatAIRequestHelper.ResolveApiKey(_options, "gemini");
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(
        List<ChatMessageDto> messages,
        [EnumeratorCancellation] CancellationToken ct)
    {
        IReadOnlyList<ChatMessageDto> preparedMessages = ChatAIRequestHelper.BuildProviderMessages(messages);
        string? systemInstruction = ChatAIRequestHelper.ExtractSystemInstruction(preparedMessages);
        IReadOnlyList<ChatMessageDto> requestMessages = ChatAIRequestHelper.BuildConversationMessagesWithoutSystem(preparedMessages);
        var payload = new GeminiGenerateContentRequest(
            string.IsNullOrWhiteSpace(systemInstruction)
                ? null
                : new GeminiSystemInstruction([new GeminiPart(systemInstruction)]),
            [.. requestMessages.Select(MapContent)]);

        string modelPath = NormalizeModel(_options.Model);
        string requestUri = $"/v1beta/{modelPath}:streamGenerateContent?alt=sse";

        using HttpRequestMessage request = new(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(payload)
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);
        _ = response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            string payloadLine = line[6..].Trim();
            if (payloadLine == "[DONE]")
            {
                break;
            }

            GeminiGenerateContentResponse? chunk =
                JsonSerializer.Deserialize<GeminiGenerateContentResponse>(payloadLine);

            IEnumerable<string> parts = chunk?.Candidates?
                .SelectMany(static x => x.Content?.Parts ?? [])
                .Select(static x => x.Text)
                .Where(static x => !string.IsNullOrWhiteSpace(x)) ?? [];

            foreach (string part in parts)
            {
                yield return part;
            }
        }
    }

    private static GeminiContent MapContent(ChatMessageDto message)
    {
        return new GeminiContent(
            MapRole(message.Role),
            [new GeminiPart(message.Content)]);
    }

    private static string MapRole(string role)
    {
        return role switch
        {
            "assistant" => "model",
            _ => "user"
        };
    }

    private static string NormalizeModel(string model)
    {
        string normalized = model.Trim();
        return normalized.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"models/{normalized}";
    }

    private sealed class GeminiGenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; init; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiResponseContent? Content { get; init; }
    }

    private sealed class GeminiResponseContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiResponsePart>? Parts { get; init; }
    }

    private sealed class GeminiResponsePart
    {
        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;
    }
}
