namespace Sery.Application.Chat;

public sealed record ResponsePolicyDecision(
    ConversationMode Mode,
    string PreferredResponseLength,
    bool ShouldAskFollowUpQuestion,
    bool PreferBriefClosure,
    bool AvoidReopening,
    string Rationale);
