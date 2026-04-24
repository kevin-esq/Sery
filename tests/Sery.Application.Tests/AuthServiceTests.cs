using Sery.Application.Auth;
using Sery.Application.Common.Interfaces;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndSession_WhenEmailDoesNotExist()
    {
        var persistence = new InMemoryAuthPersistence();
        var jwtService = new StubJwtService();
        var service = new AuthService(persistence, new StubPasswordHasher(), jwtService);

        AuthResult result = await service.RegisterAsync(
            new RegisterUserCommand("USER@Example.com", "Secret1x", "Chrome", "127.0.0.1"));

        Assert.True(result.Succeeded);
        User user = Assert.Single(persistence.Users);
        Assert.Equal("user@example.com", user.Email);

        UserSession session = Assert.Single(persistence.Sessions);
        Assert.Equal(user.Id, session.UserId);
        Assert.Equal("hash-refresh-token-1", session.RefreshTokenHash);
        Assert.Equal("Chrome", session.DeviceInfo);
        Assert.Equal("127.0.0.1", session.IpAddress);
        Assert.Equal("access-user@example.com", result.Tokens!.AccessToken);
        Assert.Equal("refresh-token-1", result.Tokens.RefreshToken);
        Assert.Equal(1, persistence.SaveChangesCalls);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenCredentialsAreInvalid()
    {
        var persistence = new InMemoryAuthPersistence();
        persistence.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed-correct"
        });

        var service = new AuthService(persistence, new StubPasswordHasher(), new StubJwtService());

        AuthResult result = await service.LoginAsync(
            new LoginCommand("user@example.com", "wrong", "Chrome", "127.0.0.1"));

        Assert.False(result.Succeeded);
        Assert.Equal(AuthError.InvalidCredentials, result.Error);
        Assert.Empty(persistence.Sessions);
        Assert.Equal(1, persistence.SaveChangesCalls);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRotateRefreshToken_WhenSessionExists()
    {
        var persistence = new InMemoryAuthPersistence();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed-secret",
            TenantId = Guid.NewGuid()
        };
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            RefreshTokenHash = "hash-old-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        persistence.Users.Add(user);
        persistence.Sessions.Add(session);

        var jwtService = new StubJwtService(["refresh-token-1"]);
        var service = new AuthService(persistence, new StubPasswordHasher(), jwtService);

        AuthResult result = await service.RefreshAsync(
            new RefreshSessionCommand("old-token", "Safari", "10.0.0.1"));

        Assert.True(result.Succeeded);
        Assert.Equal("hash-refresh-token-1", session.RefreshTokenHash);
        Assert.Equal("Safari", session.DeviceInfo);
        Assert.Equal("10.0.0.1", session.IpAddress);
        Assert.Equal("refresh-token-1", result.Tokens!.RefreshToken);
        Assert.Equal("access-user@example.com", result.Tokens.AccessToken);
        Assert.Equal(1, persistence.SaveChangesCalls);
    }

    [Fact]
    public async Task RevokeOtherSessionsAsync_ShouldRemoveAllExceptCurrentSession()
    {
        var persistence = new InMemoryAuthPersistence();
        var userId = Guid.NewGuid();
        persistence.Sessions.AddRange(
        [
            new UserSession { Id = Guid.NewGuid(), UserId = userId, RefreshTokenHash = "hash-current", ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new UserSession { Id = Guid.NewGuid(), UserId = userId, RefreshTokenHash = "hash-other-1", ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new UserSession { Id = Guid.NewGuid(), UserId = userId, RefreshTokenHash = "hash-other-2", ExpiresAt = DateTime.UtcNow.AddDays(1) }
        ]);

        var jwtService = new StubJwtService(["current"]);
        var service = new AuthService(persistence, new StubPasswordHasher(), jwtService);

        await service.RevokeOtherSessionsAsync(new RevokeOtherSessionsCommand(userId, "current"));

        Assert.Single(persistence.Sessions);
        Assert.Equal("hash-current", persistence.Sessions[0].RefreshTokenHash);
        Assert.Equal(1, persistence.SaveChangesCalls);
    }

    private sealed class InMemoryAuthPersistence : IAuthPersistence
    {
        public List<User> Users { get; } = [];
        public List<UserSession> Sessions { get; } = [];
        public int SaveChangesCalls { get; private set; }

        public Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.Any(x => x.Email == email));
        }

        public Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return Task.FromResult(Users.FirstOrDefault(x => x.Email == email));
        }

        public Task<UserSession?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(Sessions.FirstOrDefault(x => x.RefreshTokenHash == refreshTokenHash));
        }

        public Task<UserSession?> GetSessionByUserAndRefreshTokenHashAsync(
            Guid userId,
            string refreshTokenHash,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Sessions.FirstOrDefault(x => x.UserId == userId && x.RefreshTokenHash == refreshTokenHash));
        }

        public Task<UserSession?> GetSessionByIdAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Sessions.FirstOrDefault(x => x.UserId == userId && x.Id == sessionId));
        }

        public Task<IReadOnlyList<AuthSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken)
        {
            IReadOnlyList<AuthSessionDto> sessions = Sessions
                .Where(x => x.UserId == userId)
                .Select(x => new AuthSessionDto(x.Id, x.DeviceInfo, x.IpAddress, x.CreatedAt, x.ExpiresAt))
                .ToList();

            return Task.FromResult(sessions);
        }

        public Task<int> RemoveOtherSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken)
        {
            int removed = Sessions.RemoveAll(x => x.UserId == userId && x.Id != currentSessionId);
            return Task.FromResult(removed);
        }

        public void AddUser(User user)
        {
            Users.Add(user);
        }

        public void AddSession(UserSession session)
        {
            Sessions.Add(session);
        }

        public void RemoveSession(UserSession session)
        {
            _ = Sessions.Remove(session);
        }

        public Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken)
        {
            int count = Sessions.Count(x => x.UserId == userId && x.ExpiresAt > DateTime.UtcNow);
            return Task.FromResult(count);
        }

        public Task RemoveOldestSessionAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserSession? oldest = Sessions
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefault();

            if (oldest is not null)
            {
                Sessions.Remove(oldest);
            }

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return $"hashed-{password}";
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return passwordHash == $"hashed-{password}";
        }
    }

    private sealed class StubJwtService : IJwtService
    {
        private readonly Queue<string> refreshTokens;

        public StubJwtService()
            : this(["refresh-token-1"])
        {
        }

        public StubJwtService(IEnumerable<string> refreshTokens)
        {
            this.refreshTokens = new Queue<string>(refreshTokens);
        }

        public string GenerateAccessToken(User user)
        {
            return $"access-{user.Email}";
        }

        public string GenerateRefreshToken()
        {
            return refreshTokens.Dequeue();
        }

        public string HashToken(string token)
        {
            return $"hash-{token}";
        }
    }
}
