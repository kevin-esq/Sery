using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    public async IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, [EnumeratorCancellation] CancellationToken ct)
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

        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/chat/completions", request, JsonSerializerOptions.Default, ct);
        _ = response.EnsureSuccessStatusCode();

        using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
            {
                continue;
            }

            string dataLine = line[6..].Trim();
            if (dataLine == "[DONE]")
            {
                break;
            }

            ChatCompletionsChunk? chunk = JsonSerializer.Deserialize<ChatCompletionsChunk>(dataLine, JsonSerializerOptions.Default);
            string? content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            if (content != null)
            {
                yield return content;
            }
        }
    }

    private sealed record ChatCompletionsRequest(string Model, List<ApiMessage> Messages, bool Stream = true);
    private sealed record ApiMessage(string Role, string Content);

    private sealed class ChatCompletionsChunk { public List<ChunkChoice> Choices { get; init; } = []; }
    private sealed class ChunkChoice { public ChunkDelta Delta { get; init; } = new(); }
    private sealed class ChunkDelta { public string? Content { get; init; } }
}
