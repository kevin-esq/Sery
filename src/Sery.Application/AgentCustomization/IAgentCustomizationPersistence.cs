using Sery.Domain.Entities;

namespace Sery.Application.AgentCustomization;

public interface IAgentCustomizationPersistence
{
    Task<UserAgentCustomization?> GetAsync(Guid userId, CancellationToken cancellationToken);
    void Add(UserAgentCustomization customization);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}
