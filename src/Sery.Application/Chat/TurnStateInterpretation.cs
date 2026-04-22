namespace Sery.Application.Chat;

public sealed record TurnStateInterpretation(
    bool UserSeemsResolved,
    bool UserIsClosingConversation,
    bool ShouldAvoidReopening,
    bool TopicShiftLikely,
    bool WantsLightClosure,
    string Confidence,
    string Rationale);
