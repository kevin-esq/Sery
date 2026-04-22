namespace Sery.Infrastructure.AI.Gemini;

internal sealed record GeminiGenerateContentRequest(
    GeminiSystemInstruction? SystemInstruction,
    IReadOnlyList<GeminiContent> Contents);

internal sealed record GeminiSystemInstruction(
    IReadOnlyList<GeminiPart> Parts);

internal sealed record GeminiContent(
    string Role,
    IReadOnlyList<GeminiPart> Parts);

internal sealed record GeminiPart(string Text);
