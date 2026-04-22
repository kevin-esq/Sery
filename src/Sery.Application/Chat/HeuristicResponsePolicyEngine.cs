namespace Sery.Application.Chat;

public sealed class HeuristicResponsePolicyEngine : IResponsePolicyEngine
{
    public ResponsePolicyDecision BuildPolicy(
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState,
        AdaptivePacingPlan currentPacingPlan,
        RetentionPromptPlan currentRetentionPlan)
    {
        if (turnState.UserSeemsResolved || turnState.UserIsClosingConversation || turnState.WantsLightClosure)
        {
            return new ResponsePolicyDecision(
                ConversationMode.Warm,
                "short",
                false,
                true,
                true,
                "The user appears resolved or is closing, so prefer brief continuity and avoid reopening.");
        }

        if (turnState.TopicShiftLikely)
        {
            return new ResponsePolicyDecision(
                currentRetentionPlan.Mode,
                currentPacingPlan.PreferredResponseLength,
                currentPacingPlan.QuestionBudget > 0,
                false,
                false,
                "The user likely shifted topic, so retain continuity without dragging forward stale framing.");
        }

        return new ResponsePolicyDecision(
            currentRetentionPlan.Mode,
            currentPacingPlan.PreferredResponseLength,
            currentPacingPlan.QuestionBudget > 0,
            false,
            turnState.ShouldAvoidReopening,
            "Default response policy follows turn analysis and pacing.");
    }
}
