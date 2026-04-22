namespace Sery.Application.Chat;

public sealed class HeuristicAdaptivePacingPolicy : IAdaptivePacingPolicy
{
    public AdaptivePacingPlan CreatePlan(
        ConversationContext context,
        string userMessage,
        TurnAnalysis turnAnalysis,
        AgentPersonaProfile persona,
        AgentAdaptationProfile adaptation)
    {
        string responseLength = ResolveLength(turnAnalysis, persona);
        string tempo = ResolveTempo(turnAnalysis);
        string backchannelLevel = ResolveBackchannel(turnAnalysis, adaptation);
        int questionBudget = ResolveQuestionBudget(turnAnalysis, persona);
        bool useParagraphBreaks = turnAnalysis.SelfDisclosureDepth is SelfDisclosureDepth.High || responseLength == "long";
        bool preferBullets = turnAnalysis.UserNeed is UserNeed.Action && turnAnalysis.WantsDirectAdvice;
        string sentenceStyle = turnAnalysis.UserNeed is UserNeed.Regulation
            ? "short and regulating"
            : turnAnalysis.UserNeed is UserNeed.Reflection
                ? "layered but readable"
                : "clear and natural";
        string openingCadence = turnAnalysis.ShowsVulnerability
            ? "begin soft and precise"
            : turnAnalysis.WantsDirectAdvice
                ? "begin quickly with clarity"
                : "begin with balanced warmth";
        string closingCadence = turnAnalysis.UserNeed is UserNeed.Action
            ? "close with momentum"
            : turnAnalysis.UserNeed is UserNeed.Reflection
                ? "close with one reflective opening"
                : "close with gentle continuity";

        return new AdaptivePacingPlan(
            responseLength,
            tempo,
            backchannelLevel,
            questionBudget,
            useParagraphBreaks,
            preferBullets,
            sentenceStyle,
            openingCadence,
            closingCadence);
    }

    private static string ResolveLength(TurnAnalysis analysis, AgentPersonaProfile persona)
    {
        if (analysis.UserNeed is UserNeed.Regulation)
        {
            return "short";
        }

        if (analysis.SelfDisclosureDepth is SelfDisclosureDepth.High || analysis.UserNeed is UserNeed.Reflection)
        {
            return "medium-long";
        }

        return persona.PreferredResponseLength;
    }

    private static string ResolveTempo(TurnAnalysis analysis)
    {
        return analysis.UserNeed switch
        {
            UserNeed.Regulation => "slow and steady",
            UserNeed.Action => "brisk and practical",
            UserNeed.Reflection => "measured and spacious",
            _ => "balanced"
        };
    }

    private static string ResolveBackchannel(TurnAnalysis analysis, AgentAdaptationProfile adaptation)
    {
        if (!adaptation.Enabled)
        {
            return "minimal";
        }

        return analysis.ShowsVulnerability || analysis.SelfDisclosureDepth is SelfDisclosureDepth.High
            ? "visible but restrained"
            : analysis.EnergyLevel is EnergyLevel.High
                ? "light"
                : "minimal";
    }

    private static int ResolveQuestionBudget(TurnAnalysis analysis, AgentPersonaProfile persona)
    {
        if (!persona.AskFollowUpQuestions)
        {
            return 0;
        }

        if (analysis.UserNeed is UserNeed.Regulation or UserNeed.Action)
        {
            return 1;
        }

        return analysis.SelfDisclosureDepth is SelfDisclosureDepth.High ? 1 : 0;
    }
}
