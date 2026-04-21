namespace Sery.Infrastructure.AI.Ollama;

internal sealed record OllamaChatRequest(string Model, bool Stream, List<OllamaApiMessage> Messages);
