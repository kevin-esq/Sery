namespace Sery.Application.Chat;

public sealed record ChatPromptContext(
    ConversationContext ConversationContext,
    string UserMessage,
    AgentPersonaProfile Persona,
    AgentAdaptationProfile Adaptation,
    TurnAnalysis TurnAnalysis,
    TurnStateInterpretation TurnState,
    MemoryOrchestrationResult MemoryPlan,
    ResponsePolicyDecision ResponsePolicy,
    AdaptivePacingPlan AdaptivePacingPlan,
    RetentionPromptPlan RetentionPromptPlan,
    IReadOnlyList<RetentionHook> Hooks);
