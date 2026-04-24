using Sery.Domain.Entities;

namespace Sery.Application.Auth;

public interface IAuthPersistence
{
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserSession?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);

    Task<UserSession?> GetSessionByUserAndRefreshTokenHashAsync(
        Guid userId,
        string refreshTokenHash,
        CancellationToken cancellationToken);

    Task<UserSession?> GetSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuthSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> RemoveOtherSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken);

    void AddUser(User user);

    void AddSession(UserSession session);

    void RemoveSession(UserSession session);

    Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken);

    Task RemoveOldestSessionAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
