using System.Text.RegularExpressions;

namespace Sery.Application.Chat;

public sealed class HeuristicUserFactExtractor : IUserFactExtractor
{
    public IReadOnlyList<UserFactExtraction> Extract(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis)
    {
        string trimmed = userMessage.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return [];
        }

        var facts = new List<UserFactExtraction>();
        AddIfMatch(facts, ExtractName(trimmed));
        AddIfMatch(facts, ExtractPreference(trimmed));
        AddIfMatch(facts, ExtractWorkIdentity(trimmed));
        AddIfMatch(facts, ExtractLocation(trimmed));
        AddIfMatch(facts, ExtractResponsePreference(trimmed));

        return facts
            .GroupBy(x => x.NormalizedKey, StringComparer.Ordinal)
            .Select(x => x.First())
            .ToList();
    }

    private static void AddIfMatch(List<UserFactExtraction> facts, UserFactExtraction? extraction)
    {
        if (extraction is not null)
        {
            facts.Add(extraction);
        }
    }

    private static UserFactExtraction? ExtractName(string text)
    {
        Match match = Regex.Match(text, @"\b(me llamo|soy)\s+([A-Za-zÁÉÍÓÚáéíóúÑñ][A-Za-zÁÉÍÓÚáéíóúÑñ0-9_-]{1,30})", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        string name = match.Groups[2].Value.Trim();
        return new UserFactExtraction("identity", $"El usuario se llama {name}.", $"identity:name:{TextHeuristics.Normalize(name)}");
    }

    private static UserFactExtraction? ExtractPreference(string text)
    {
        Match match = Regex.Match(
            text,
            @"\b(mi\s+(?:lenguaje|comida|color|música|musica|película|pelicula|serie|juego|hobby|lenguaje favorito)\s+(?:favorito|favorita)?\s*(?:es|son)|me gusta(?:n)?|me encanta(?:n)?)\s+(.+)$",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        string value = SanitizeFactValue(match.Groups[2].Value);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new UserFactExtraction("preference", $"Al usuario le gusta {value}.", $"preference:{TextHeuristics.Normalize(value)}");
    }

    private static UserFactExtraction? ExtractWorkIdentity(string text)
    {
        Match match = Regex.Match(text, @"\b(trabajo como|soy)\s+(desarrollador|developer|ingeniero|ingeniera|diseñador|diseñadora|estudiante|doctor|doctora|psicologo|psicologa)\b", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        string role = match.Groups[2].Value.Trim();
        return new UserFactExtraction("identity", $"El usuario es {role}.", $"identity:role:{TextHeuristics.Normalize(role)}");
    }

    private static UserFactExtraction? ExtractLocation(string text)
    {
        Match match = Regex.Match(text, @"\b(vivo en|soy de)\s+([A-Za-zÁÉÍÓÚáéíóúÑñ\s]{2,40})", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        string location = SanitizeFactValue(match.Groups[2].Value);
        return string.IsNullOrWhiteSpace(location)
            ? null
            : new UserFactExtraction("context", $"El usuario vive en {location}.", $"context:location:{TextHeuristics.Normalize(location)}");
    }

    private static UserFactExtraction? ExtractResponsePreference(string text)
    {
        string normalized = TextHeuristics.Normalize(text);

        if (TextHeuristics.ContainsAny(normalized, ["hablame directo", "se mas directo", "quiero que seas directo", "sin rodeos"]))
        {
            return new UserFactExtraction("response-preference", "El usuario prefiere respuestas directas y sin rodeos.", "response-preference:direct");
        }

        if (TextHeuristics.ContainsAny(normalized, ["responde corto", "mas breve", "quiero respuestas cortas", "no tan largo"]))
        {
            return new UserFactExtraction("response-preference", "El usuario prefiere respuestas breves.", "response-preference:brief");
        }

        if (TextHeuristics.ContainsAny(normalized, ["hazme preguntas", "preguntame", "quiero que me preguntes"]))
        {
            return new UserFactExtraction("response-preference", "El usuario valora preguntas de seguimiento cuando aportan claridad.", "response-preference:follow-up");
        }

        return null;
    }

    private static string SanitizeFactValue(string value)
    {
        string cleaned = value
            .Trim()
            .TrimEnd('.', '!', '?', ',')
            .Trim();

        string[] separators = [" y ", " pero ", " aunque ", " además ", " ademas "];
        foreach (string separator in separators)
        {
            int index = cleaned.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                cleaned = cleaned[..index].Trim();
                break;
            }
        }

        return cleaned;
    }
}
