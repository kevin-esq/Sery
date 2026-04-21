namespace Sery.Application.Auth;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? DeviceInfo,
    string? IpAddress);

public sealed record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo,
    string? IpAddress);

public sealed record RefreshSessionCommand(
    string RefreshToken,
    string? DeviceInfo,
    string? IpAddress);

public sealed record LogoutCommand(Guid UserId, string RefreshToken);

public sealed record RevokeOtherSessionsCommand(Guid UserId, string CurrentRefreshToken);
