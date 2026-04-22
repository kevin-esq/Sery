namespace Sery.Application.Chat;

public interface IAgentPersonaProvider
{
    AgentPersonaProfile GetPersona();

    AgentAdaptationProfile GetAdaptation();
}
