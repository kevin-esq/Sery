namespace Sery.Application.Chat;

public sealed record RelationalPersonaProfile(
    RelationshipMode RelationshipMode,
    int Closeness,
    int Tenderness,
    int Protectiveness,
    int Flirtiness,
    bool UsesAffectionateLanguage,
    bool AllowsRomanticFraming,
    bool PrioritizeSupportOverRoleplay);
