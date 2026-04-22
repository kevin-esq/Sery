namespace Sery.Application.Chat;

public sealed record AgentAdaptationProfile(
    bool Enabled,
    bool MirrorUserEnergy,
    bool MirrorUserFormality,
    bool MirrorUserVerbosity,
    bool AllowModeShifts,
    int AdaptationStrength,
    int MaxToneShiftPerTurn);
