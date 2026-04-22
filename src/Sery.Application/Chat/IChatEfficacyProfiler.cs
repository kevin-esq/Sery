namespace Sery.Application.Chat;

public interface IChatEfficacyProfiler
{
    ChatEfficacyProfileSnapshot BuildProfile(
        IReadOnlyList<string> recentUserMessages,
        TurnAnalysis latestTurnAnalysis,
        ConversationMode latestMode,
        AdaptivePacingPlan latestPacingPlan,
        ChatEfficacyProfileSnapshot? currentProfile = null);
}
