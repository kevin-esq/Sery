namespace Sery.Application.Chat;

public sealed record ResponseCritiqueResult(
    bool ShouldRewrite,
    string FinalResponse,
    string Confidence,
    string Rationale);
