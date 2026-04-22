namespace Sery.Application.Chat;

public sealed record RetentionPromptPlan(
    ConversationMode Mode,
    string OpeningInstruction,
    string RelationalGoal,
    string TempoInstruction,
    string ClosingInstruction);
