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

    public async IAsyncEnumerable<string> GenerateStreamAsync(List<ChatMessageDto> messages, [EnumeratorCancellation] CancellationToken ct)
    {
        httpClient.BaseAddress = new Uri(_options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            ChatAIRequestHelper.ResolveApiKey(_options, "openai"));

        IReadOnlyList<ChatMessageDto> requestMessages = ChatAIRequestHelper.BuildProviderMessages(messages);
        var request = new ChatCompletionsRequest(
            _options.Model,
            [.. requestMessages.Select(x => new ApiMessage(x.Role, x.Content))]);

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
