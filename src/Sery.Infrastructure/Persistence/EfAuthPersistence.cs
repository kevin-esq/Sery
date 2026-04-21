using Microsoft.EntityFrameworkCore;
using Sery.Application.Auth;
using Sery.Domain.Entities;

namespace Sery.Infrastructure.Persistence;

public sealed class EfAuthPersistence(SeryDbContext dbContext) : IAuthPersistence
{
    public Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<UserSession?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        return dbContext.UserSessions
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);
    }

    public Task<UserSession?> GetSessionByUserAndRefreshTokenHashAsync(
        Guid userId,
        string refreshTokenHash,
        CancellationToken cancellationToken)
    {
        return dbContext.UserSessions
            .FirstOrDefaultAsync(
                x => x.UserId == userId && x.RefreshTokenHash == refreshTokenHash,
                cancellationToken);
    }

    public Task<UserSession?> GetSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        return dbContext.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AuthSessionDto(
                x.Id,
                x.DeviceInfo,
                x.IpAddress,
                x.CreatedAt,
                x.ExpiresAt))
            .ToListAsync(cancellationToken);
    }

    public Task<int> RemoveOtherSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken)
    {
        return dbContext.UserSessions
            .Where(x => x.UserId == userId && x.Id != currentSessionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public void AddUser(User user)
    {
        _ = dbContext.Users.Add(user);
    }

    public void AddSession(UserSession session)
    {
        _ = dbContext.UserSessions.Add(session);
    }

    public void RemoveSession(UserSession session)
    {
        _ = dbContext.UserSessions.Remove(session);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await dbContext.SaveChangesAsync(cancellationToken);
    }
}
