namespace Sery.Application.Chat;

public sealed record CompanionSafetyProfile(
    bool MustStayExplicitlyAI,
    bool BlockExclusiveBondingLanguage,
    bool EncourageOfflineSupport,
    bool DeescalateDuringCrisis,
    bool DisallowSexualContent,
    bool DisallowManipulativeDependencyLanguage);
