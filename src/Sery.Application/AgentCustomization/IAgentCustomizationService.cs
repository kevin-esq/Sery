namespace Sery.Application.AgentCustomization;

public interface IAgentCustomizationService
{
    Task<AgentCustomizationDto> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<AgentCustomizationDto> UpdateAsync(Guid userId, UpdateAgentCustomizationCommand command, CancellationToken cancellationToken);
    Task ResetAsync(Guid userId, CancellationToken cancellationToken);
}
