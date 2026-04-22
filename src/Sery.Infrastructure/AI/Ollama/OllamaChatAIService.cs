using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI.Ollama;

public sealed class OllamaChatAIService : IChatAIService
{
    private readonly HttpClient _httpClient;
    private readonly ChatAIOptions _options;
    private readonly ILogger<OllamaChatAIService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public OllamaChatAIService(
        HttpClient httpClient,
        IOptions<ChatAIOptions> options,
        ILogger<OllamaChatAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("ChatAI BaseUrl is required for Ollama.");
        }

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, [EnumeratorCancellation] CancellationToken ct)
    {
        IReadOnlyList<ChatMessageDto> requestMessages = ChatAIRequestHelper.BuildProviderMessages(messages);
        var apiMessages = requestMessages.Select(static m => new OllamaApiMessage(m.Role, m.Content)).ToList();

        var request = new OllamaChatRequest(_options.Model, true, apiMessages);

        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/chat", request, JsonOptions, ct);
        _ = response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            OllamaChatResponse? chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, JsonOptions);
            if (chunk?.Message?.Content != null)
            {
                yield return chunk.Message.Content;
            }
        }
    }
}
