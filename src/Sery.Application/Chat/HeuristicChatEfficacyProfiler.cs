namespace Sery.Application.Chat;

public sealed class HeuristicChatEfficacyProfiler : IChatEfficacyProfiler
{
    public ChatEfficacyProfileSnapshot BuildProfile(
        IReadOnlyList<string> recentUserMessages,
        TurnAnalysis latestTurnAnalysis,
        ConversationMode latestMode,
        AdaptivePacingPlan latestPacingPlan,
        ChatEfficacyProfileSnapshot? currentProfile = null)
    {
        string preferredMode = ResolveMode(recentUserMessages, latestTurnAnalysis, latestMode);
        string preferredLength = ResolveLength(recentUserMessages, latestPacingPlan, currentProfile);
        string questionStyle = latestTurnAnalysis.WantsReflection || latestTurnAnalysis.IsQuestionHeavy
            ? "likes one calibrated reflective question"
            : "prefers minimal questioning";
        string actionStyle = latestTurnAnalysis.WantsDirectAdvice || latestTurnAnalysis.UserNeed is UserNeed.Action
            ? "respond well to clear next steps"
            : "prefer validation before action";
        string pacing = latestPacingPlan.Tempo;
        string summary = BuildSummary(preferredMode, preferredLength, questionStyle, actionStyle, pacing, currentProfile);

        return new ChatEfficacyProfileSnapshot(
            preferredMode,
            preferredLength,
            questionStyle,
            actionStyle,
            pacing,
            summary,
            DateTime.UtcNow);
    }

    private static string ResolveMode(
        IReadOnlyList<string> recentUserMessages,
        TurnAnalysis analysis,
        ConversationMode latestMode)
    {
        if (analysis.UserNeed is UserNeed.Regulation)
        {
            return ConversationMode.Grounding.ToString();
        }

        if (analysis.UserNeed is UserNeed.Action)
        {
            return ConversationMode.Direct.ToString();
        }

        int avgWords = recentUserMessages.Count == 0
            ? 0
            : (int)recentUserMessages.Average(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

        if (avgWords >= 25 || analysis.SelfDisclosureDepth is SelfDisclosureDepth.High)
        {
            return ConversationMode.Deep.ToString();
        }

        return latestMode.ToString();
    }

    private static string ResolveLength(
        IReadOnlyList<string> recentUserMessages,
        AdaptivePacingPlan latestPacingPlan,
        ChatEfficacyProfileSnapshot? currentProfile)
    {
        int avgWords = recentUserMessages.Count == 0
            ? 0
            : (int)recentUserMessages.Average(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

        if (avgWords >= 30)
        {
            return "medium-long";
        }

        if (avgWords <= 8)
        {
            return "short";
        }

        return currentProfile?.PreferredResponseLength ?? latestPacingPlan.PreferredResponseLength;
    }

    private static string BuildSummary(
        string mode,
        string preferredLength,
        string questionStyle,
        string actionStyle,
        string pacing,
        ChatEfficacyProfileSnapshot? currentProfile)
    {
        string current = string.Join(
            " ",
            new[]
            {
                $"The user currently responds best to {mode} conversations.",
                $"They appear to prefer {preferredLength} answers.",
                $"Question style: {questionStyle}.",
                $"Action style: {actionStyle}.",
                $"Pacing that seems to fit: {pacing}."
            });

        if (string.IsNullOrWhiteSpace(currentProfile?.SuccessfulStrategiesSummary))
        {
            return current;
        }

        return $"{currentProfile.SuccessfulStrategiesSummary} {current}".Trim();
    }
}
