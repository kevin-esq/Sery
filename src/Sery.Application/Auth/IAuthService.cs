namespace Sery.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(RefreshSessionCommand command, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSessionDto>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

    Task RevokeOtherSessionsAsync(
        RevokeOtherSessionsCommand command,
        CancellationToken cancellationToken = default);
}
