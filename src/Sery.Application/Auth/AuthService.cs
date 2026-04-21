using Sery.Application.Common.Interfaces;
using Sery.Domain.Entities;

namespace Sery.Application.Auth;

public sealed class AuthService(
    IAuthPersistence authPersistence,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IAuthService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public async Task<AuthResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = NormalizeEmail(command.Email);
        bool userExists = await authPersistence.UserExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (userExists)
        {
            return AuthResult.Failure(AuthError.UserAlreadyExists);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(command.Password),
            TenantId = Guid.NewGuid()
        };

        authPersistence.AddUser(user);

        AuthTokens tokens = CreateSession(user, command.DeviceInfo, command.IpAddress);
        await authPersistence.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(tokens);
    }

    public async Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = NormalizeEmail(command.Email);
        User? user = await authPersistence.GetUserByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            return AuthResult.Failure(AuthError.InvalidCredentials);
        }

        AuthTokens tokens = CreateSession(user, command.DeviceInfo, command.IpAddress);
        await authPersistence.SaveChangesAsync(cancellationToken);

        return AuthResult.Success(tokens);
    }

    public async Task<AuthResult> RefreshAsync(RefreshSessionCommand command, CancellationToken cancellationToken = default)
    {
        string tokenHash = jwtService.HashToken(command.RefreshToken);
        UserSession? session = await authPersistence.GetSessionByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (session is null || session.ExpiresAt <= DateTime.UtcNow)
        {
            return AuthResult.Failure(AuthError.InvalidRefreshToken);
        }

        string newRefreshToken = jwtService.GenerateRefreshToken();
        session.RefreshTokenHash = jwtService.HashToken(newRefreshToken);
        session.ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime);
        session.IpAddress = command.IpAddress;
        session.DeviceInfo = command.DeviceInfo;

        await authPersistence.SaveChangesAsync(cancellationToken);

        string accessToken = jwtService.GenerateAccessToken(session.User);
        return AuthResult.Success(new AuthTokens(accessToken, newRefreshToken));
    }

    public async Task LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        string tokenHash = jwtService.HashToken(command.RefreshToken);
        UserSession? session = await authPersistence.GetSessionByUserAndRefreshTokenHashAsync(
            command.UserId,
            tokenHash,
            cancellationToken);

        if (session is null)
        {
            return;
        }

        authPersistence.RemoveSession(session);
        await authPersistence.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<AuthSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return authPersistence.GetSessionsAsync(userId, cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        UserSession? session = await authPersistence.GetSessionByIdAsync(userId, sessionId, cancellationToken);
        if (session is null)
        {
            return;
        }

        authPersistence.RemoveSession(session);
        await authPersistence.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeOtherSessionsAsync(
        RevokeOtherSessionsCommand command,
        CancellationToken cancellationToken = default)
    {
        string currentTokenHash = jwtService.HashToken(command.CurrentRefreshToken);
        UserSession? currentSession = await authPersistence.GetSessionByUserAndRefreshTokenHashAsync(
            command.UserId,
            currentTokenHash,
            cancellationToken);

        if (currentSession is null)
        {
            return;
        }

        _ = await authPersistence.RemoveOtherSessionsAsync(command.UserId, currentSession.Id, cancellationToken);
        await authPersistence.SaveChangesAsync(cancellationToken);
    }

    private AuthTokens CreateSession(User user, string? deviceInfo, string? ipAddress)
    {
        string refreshToken = jwtService.GenerateRefreshToken();
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = jwtService.HashToken(refreshToken),
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime)
        };

        authPersistence.AddSession(session);

        string accessToken = jwtService.GenerateAccessToken(user);
        return new AuthTokens(accessToken, refreshToken);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
