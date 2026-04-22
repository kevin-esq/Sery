namespace Sery.Application.Chat;

public sealed class HeuristicTurnAnalyzer : ITurnAnalyzer
{
    public TurnAnalysis Analyze(ConversationContext context, string userMessage)
    {
        string message = userMessage.Trim();
        string lower = TextHeuristics.Normalize(message);
        string emotion = DetectEmotion(lower, context);
        EmotionalIntensity intensity = DetectIntensity(lower);
        EnergyLevel energy = DetectEnergy(lower);
        FormalityLevel formality = DetectFormality(lower);
        SelfDisclosureDepth disclosureDepth = DetectDisclosureDepth(message, lower);
        bool wantsDirectAdvice = ContainsAny(lower, ["qué hago", "que hago", "dime", "al grano", "directo", "just tell me", "what should i do"]);
        bool wantsReflection = ContainsAny(lower, ["por qué", "porque me pasa", "no entiendo", "what does it mean", "pattern", "patrón"]);
        bool questionHeavy = message.Count(x => x == '?') > 0;
        bool vulnerability = ContainsAny(lower, ["me siento", "tengo miedo", "ansioso", "ansiosa", "triste", "solo", "sola", "abrumado", "abrumada"]);
        bool humor = ContainsAny(lower, ["jaja", "ajaj", "xd", "lol", "haha"]);
        bool urgency = intensity is EmotionalIntensity.High or EmotionalIntensity.Crisis ||
            ContainsAny(lower, ["ahora", "ya", "urgente", "panic", "ataque", "no puedo más", "no puedo mas"]);

        UserNeed need = ResolveNeed(wantsDirectAdvice, wantsReflection, vulnerability, urgency, questionHeavy);

        return new TurnAnalysis(
            emotion,
            intensity,
            energy,
            formality,
            disclosureDepth,
            need,
            wantsDirectAdvice,
            wantsReflection,
            questionHeavy,
            vulnerability,
            humor,
            urgency);
    }

    private static string DetectEmotion(string lower, ConversationContext context)
    {
        if (ContainsAny(lower, ["suicid", "hacerme daño", "autoles", "kill myself", "hurt myself"]))
        {
            return "crisis";
        }

        if (ContainsAny(lower, ["ansioso", "ansiosa", "nervioso", "nerviosa", "ansiedad", "panic"]))
        {
            return "anxious";
        }

        if (ContainsAny(lower, ["triste", "vacío", "vacio", "deprimido", "deprimida", "llorar"]))
        {
            return "sad";
        }

        if (ContainsAny(lower, ["frustrado", "frustrada", "enojado", "enojada", "rabia"]))
        {
            return "frustrated";
        }

        if (ContainsAny(lower, ["confundido", "confundida", "no entiendo", "perdido", "perdida"]))
        {
            return "confused";
        }

        return context.EmotionalMemory?.DominantEmotion is { Length: > 0 }
            ? context.EmotionalMemory.DominantEmotion
            : "mixed";
    }

    private static EmotionalIntensity DetectIntensity(string lower)
    {
        if (ContainsAny(lower, ["suicid", "hacerme daño", "autoles", "kill myself", "hurt myself"]))
        {
            return EmotionalIntensity.Crisis;
        }

        int score = 0;
        if (ContainsAny(lower, ["muy", "demasiado", "no puedo", "me estoy ahogando", "me supera", "panic", "ataque"])) score += 2;
        if (ContainsAny(lower, ["ansioso", "ansiosa", "triste", "abrumado", "abrumada", "miedo", "rabia"])) score += 1;

        return score switch
        {
            <= 0 => EmotionalIntensity.Low,
            1 => EmotionalIntensity.Moderate,
            2 => EmotionalIntensity.High,
            _ => EmotionalIntensity.High
        };
    }

    private static EnergyLevel DetectEnergy(string lower)
    {
        if (ContainsAny(lower, ["!!!", "ya", "urgente", "rápido", "rapido", "ahora"]))
        {
            return EnergyLevel.High;
        }

        if (ContainsAny(lower, ["cansado", "cansada", "agotado", "agotada", "vacío", "vacio"]))
        {
            return EnergyLevel.Low;
        }

        return EnergyLevel.Medium;
    }

    private static FormalityLevel DetectFormality(string lower)
    {
        if (ContainsAny(lower, ["estimado", "quisiera", "considero", "agradecería", "agradeceria"]))
        {
            return FormalityLevel.Formal;
        }

        if (ContainsAny(lower, ["jaja", "wey", "bro", "xd", "lol", "tipo"]))
        {
            return FormalityLevel.Casual;
        }

        return FormalityLevel.Neutral;
    }

    private static SelfDisclosureDepth DetectDisclosureDepth(string message, string lower)
    {
        int words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        bool introspective = ContainsAny(lower, ["me siento", "pienso", "me pasa", "me cuesta", "siento que", "hay una parte de mí", "hay una parte de mi"]);

        if (words >= 35 || (words >= 18 && introspective))
        {
            return SelfDisclosureDepth.High;
        }

        if (words >= 10 || introspective)
        {
            return SelfDisclosureDepth.Medium;
        }

        return SelfDisclosureDepth.Low;
    }

    private static UserNeed ResolveNeed(
        bool wantsDirectAdvice,
        bool wantsReflection,
        bool vulnerability,
        bool urgency,
        bool questionHeavy)
    {
        if (urgency)
        {
            return UserNeed.Regulation;
        }

        if (wantsDirectAdvice)
        {
            return UserNeed.Action;
        }

        if (wantsReflection || questionHeavy)
        {
            return UserNeed.Reflection;
        }

        if (vulnerability)
        {
            return UserNeed.Validation;
        }

        return UserNeed.Clarity;
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> probes)
    {
        return TextHeuristics.ContainsAny(text, probes);
    }
}
