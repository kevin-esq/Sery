using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public static class RelevantMemoryScorer
{
    public static IReadOnlyList<Message> RankMessages(string query, IEnumerable<Message> candidates, int limit)
    {
        string normalizedQuery = TextHeuristics.Normalize(query);
        HashSet<string> queryTokens = TextHeuristics.Tokenize(normalizedQuery);

        return candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreText(queryTokens, normalizedQuery, candidate.Content, candidate.CreatedAt)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Candidate.CreatedAt)
            .Take(limit)
            .Select(x => x.Candidate)
            .ToList();
    }

    public static IReadOnlyList<UserMemoryFact> RankFacts(string query, IEnumerable<UserMemoryFact> candidates, int limit)
    {
        string normalizedQuery = TextHeuristics.Normalize(query);
        HashSet<string> queryTokens = TextHeuristics.Tokenize(normalizedQuery);

        return candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreText(queryTokens, normalizedQuery, candidate.Content, candidate.LastSeenAt)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Candidate.LastSeenAt)
            .Take(limit)
            .Select(x => x.Candidate)
            .ToList();
    }

    private static double ScoreText(HashSet<string> queryTokens, string normalizedQuery, string candidateText, DateTime recency)
    {
        string normalizedCandidate = TextHeuristics.Normalize(candidateText);
        HashSet<string> candidateTokens = TextHeuristics.Tokenize(normalizedCandidate);
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
        {
            return 0;
        }

        int overlap = queryTokens.Intersect(candidateTokens).Count();
        bool containsPhrase = normalizedCandidate.Contains(normalizedQuery, StringComparison.Ordinal) ||
            normalizedQuery.Contains(normalizedCandidate, StringComparison.Ordinal);
        double tokenScore = (double)overlap / queryTokens.Count;
        double recencyBoost = Math.Max(0, 1 - Math.Min(30, (DateTime.UtcNow - recency).TotalDays) / 60d);
        double phraseBoost = containsPhrase ? 0.4 : 0;

        return tokenScore + phraseBoost + recencyBoost * 0.2;
    }
}
