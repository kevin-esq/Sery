namespace Sery.Application.Chat;

public sealed record MemoryWritePlan(
    bool UpdateConversationSummary,
    bool UpdateEmotionalMemory,
    bool UpdateEfficacyProfile,
    bool CapturePreferenceSignal,
    string Rationale);
