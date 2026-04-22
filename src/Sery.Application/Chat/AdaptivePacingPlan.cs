namespace Sery.Application.Chat;

public sealed record AdaptivePacingPlan(
    string PreferredResponseLength,
    string Tempo,
    string BackchannelLevel,
    int QuestionBudget,
    bool UseParagraphBreaks,
    bool PreferBullets,
    string SentenceStyle,
    string OpeningCadence,
    string ClosingCadence);
