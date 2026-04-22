namespace Sery.Application.Chat;

public interface IResponsePolicyEngine
{
    ResponsePolicyDecision BuildPolicy(
        TurnAnalysis turnAnalysis,
        TurnStateInterpretation turnState,
        AdaptivePacingPlan currentPacingPlan,
        RetentionPromptPlan currentRetentionPlan);
}
