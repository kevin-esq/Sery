using Microsoft.EntityFrameworkCore;
using Sery.Application.AgentCustomization;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfAgentCustomizationPersistence(SeryDbContext dbContext) : IAgentCustomizationPersistence
{
    public Task<UserAgentCustomization?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.UserAgentCustomizations.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public void Add(UserAgentCustomization customization)
    {
        _ = dbContext.UserAgentCustomizations.Add(customization);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        int deleted = await dbContext.UserAgentCustomizations
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            return;
        }
    }
}
