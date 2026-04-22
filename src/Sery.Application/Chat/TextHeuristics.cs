using System.Globalization;
using System.Text;

namespace Sery.Application.Chat;

public static class TextHeuristics
{
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string lowered = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(lowered.Length);

        foreach (char c in lowered)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ');
        }

        string cleaned = CollapseRepeatedCharacters(builder.ToString());
        return CollapseWhitespace(cleaned);
    }

    public static bool ContainsAny(string text, IReadOnlyList<string> probes)
    {
        string normalizedText = Normalize(text);
        return probes.Any(probe => normalizedText.Contains(Normalize(probe), StringComparison.Ordinal));
    }

    public static HashSet<string> Tokenize(string text)
    {
        string normalized = Normalize(text);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        string[] stopWords =
        [
            "de", "la", "el", "los", "las", "y", "o", "que", "me", "mi", "mis", "tu", "tus", "un", "una",
            "por", "para", "con", "sin", "del", "al", "en", "es", "ya", "pero", "muy", "the", "and", "or",
            "for", "with", "this", "that", "was", "are"
        ];

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length > 2 && !stopWords.Contains(token, StringComparer.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string CollapseRepeatedCharacters(string text)
    {
        if (text.Length <= 2)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        char previous = '\0';
        int runLength = 0;

        foreach (char c in text)
        {
            if (c == previous)
            {
                runLength++;
                if (char.IsLetter(c) && runLength >= 2)
                {
                    continue;
                }
            }
            else
            {
                previous = c;
                runLength = 0;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    private static string CollapseWhitespace(string text)
    {
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
