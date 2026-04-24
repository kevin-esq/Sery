using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Sery.API.Common;
using Sery.API.Configuration;
using Sery.API.Contracts.Auth;
using Sery.API.Contracts.Common;
using Sery.Application.Auth;
using Sery.Application.Common.Interfaces;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Exposes authentication and session management endpoints.
/// </summary>
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(
    IAuthService authService,
    IUserContext userContext,
    IApiProblemDetailsFactory problemDetailsFactory,
    IOptions<AuthSecurityOptions> authSecurityOptions) : ControllerBase
{
    private readonly AuthSecurityOptions _authSecurity = authSecurityOptions.Value;

    /// <summary>
    /// Registers a user and creates the first session.
    /// </summary>
    /// <param name="request">Registration payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Access token for the newly created account. Refresh token is set as an HttpOnly cookie.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: user registered and initial session created.
    /// - 409: email already exists.
    /// - 429: rate limit exceeded.
    /// </remarks>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EnableRateLimiting("auth-register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        AuthResult result = await authService.RegisterAsync(
            new RegisterUserCommand(
                request.Email,
                request.Password,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.Error == AuthError.WeakPassword)
            {
                ProblemDetails problem = problemDetailsFactory.CreateValidationProblem(HttpContext, ErrorCatalog.WeakPassword);
                return BadRequest(problem);
            }

            ProblemDetails conflictProblem = problemDetailsFactory.CreateConflictProblem(HttpContext, ErrorCatalog.UserAlreadyExists);
            return Conflict(conflictProblem);
        }

        SetRefreshTokenCookie(result.Tokens!.RefreshToken);
        return Ok(ToResponse(result.Tokens));
    }

    /// <summary>
    /// Authenticates a user and creates a new device session.
    /// </summary>
    /// <param name="request">Login payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Access token for the authenticated session. Refresh token is set as an HttpOnly cookie.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: credentials valid and session created.
    /// - 401: invalid credentials.
    /// - 429: rate limit exceeded.
    /// </remarks>
    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthResult result = await authService.LoginAsync(
            new LoginCommand(
                request.Email,
                request.Password,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken);

        if (!result.Succeeded)
        {
            ProblemDetails problem = problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.InvalidCredentials);
            return Unauthorized(problem);
        }

        SetRefreshTokenCookie(result.Tokens!.RefreshToken);
        return Ok(ToResponse(result.Tokens));
    }

    /// <summary>
    /// Rotates the refresh token for an existing session.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A newly rotated access token. The new refresh token replaces the HttpOnly cookie.</returns>
    /// <remarks>
    /// The current refresh token is read from the HttpOnly cookie automatically.
    ///
    /// Outcomes:
    /// - 200: refresh token valid and rotated.
    /// - 401: refresh token invalid or expired.
    /// - 429: rate limit exceeded.
    /// </remarks>
    [HttpPost("refresh")]
    [Produces("application/json")]
    [EnableRateLimiting("auth-refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        string? refreshToken = ReadRefreshTokenCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            ProblemDetails problem = problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.InvalidRefreshToken);
            return Unauthorized(problem);
        }

        AuthResult result = await authService.RefreshAsync(
            new RefreshSessionCommand(
                refreshToken,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken);

        if (!result.Succeeded)
        {
            ClearRefreshTokenCookie();
            ProblemDetails problem = problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.InvalidRefreshToken);
            return Unauthorized(problem);
        }

        SetRefreshTokenCookie(result.Tokens!.RefreshToken);
        return Ok(ToResponse(result.Tokens));
    }

    /// <summary>
    /// Revokes the current session.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the session is revoked or already absent.</returns>
    /// <remarks>
    /// Requires bearer authentication. The refresh token is read from the HttpOnly cookie.
    /// </remarks>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        string? refreshToken = ReadRefreshTokenCookie();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await authService.LogoutAsync(new LogoutCommand(userId.Value, refreshToken), cancellationToken);
        }

        ClearRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>
    /// Lists all active sessions for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The active sessions owned by the authenticated user.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    /// </remarks>
    [Authorize]
    [HttpGet("sessions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> GetSessions(CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        IReadOnlyList<AuthSessionDto> sessions = await authService.GetSessionsAsync(userId.Value, cancellationToken);
        return Ok(sessions.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Revokes a specific session owned by the authenticated user.
    /// </summary>
    /// <param name="id">Session identifier to revoke.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the target session is revoked or already absent.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    /// </remarks>
    [Authorize]
    [HttpDelete("sessions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        await authService.RevokeSessionAsync(userId.Value, id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Revokes every active session except the current one.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the operation completes.</returns>
    /// <remarks>
    /// Requires bearer authentication. The current refresh token is read from the HttpOnly cookie.
    /// </remarks>
    [Authorize]
    [HttpDelete("sessions/other")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeOtherSessions(CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        string? refreshToken = ReadRefreshTokenCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.InvalidRefreshToken));
        }

        await authService.RevokeOtherSessionsAsync(
            new RevokeOtherSessionsCommand(userId.Value, refreshToken),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns the authenticated user context.
    /// </summary>
    /// <returns>The user identity resolved from JWT claims.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    /// </remarks>
    [Authorize]
    [HttpGet("me")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public ActionResult<MeResponse> Me()
    {
        return Ok(new MeResponse
        {
            UserId = userContext.UserId,
            Email = userContext.Email,
            TenantId = userContext.TenantId
        });
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(_authSecurity.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = _authSecurity.RefreshTokenCookiePath,
            MaxAge = TimeSpan.FromDays(_authSecurity.RefreshTokenLifetimeDays),
            IsEssential = true
        });
    }

    private string? ReadRefreshTokenCookie()
    {
        return Request.Cookies[_authSecurity.RefreshTokenCookieName];
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(_authSecurity.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = _authSecurity.RefreshTokenCookiePath,
            IsEssential = true
        });
    }

    private static AuthResponse ToResponse(AuthTokens tokens)
    {
        return new AuthResponse
        {
            AccessToken = tokens.AccessToken
        };
    }

    private static SessionResponse ToResponse(AuthSessionDto session)
    {
        return new SessionResponse
        {
            Id = session.Id,
            DeviceInfo = session.DeviceInfo,
            IpAddress = session.IpAddress,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt
        };
    }
}
