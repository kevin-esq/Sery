namespace Sery.Application.Chat;

public sealed record MemoryReadPlan(
    int HistoryWindow,
    bool IncludeConversationSummary,
    bool IncludeEmotionalMemory,
    bool IncludeEfficacyMemory,
    bool IncludeRelevantMessages,
    bool IncludeRelevantFacts,
    int RelevantMessageLimit,
    int RelevantFactLimit,
    string FocusInstruction);
