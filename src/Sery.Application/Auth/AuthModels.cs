namespace Sery.Application.Auth;

public sealed record AuthTokens(string AccessToken, string RefreshToken);

public sealed record AuthSessionDto(
    Guid Id,
    string? DeviceInfo,
    string? IpAddress,
    DateTime CreatedAt,
    DateTime ExpiresAt);

public enum AuthError
{
    None = 0,
    UserAlreadyExists = 1,
    InvalidCredentials = 2,
    InvalidRefreshToken = 3
}

public sealed record AuthResult(bool Succeeded, AuthError Error, AuthTokens? Tokens)
{
    public static AuthResult Success(AuthTokens tokens) => new(true, AuthError.None, tokens);

    public static AuthResult Failure(AuthError error) => new(false, error, null);
}
