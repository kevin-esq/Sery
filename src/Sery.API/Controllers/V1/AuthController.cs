using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
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
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    /// Registers a user and creates the first session.
    /// </summary>
    /// <param name="request">Registration payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Access and refresh tokens for the newly created account.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: user registered and initial session created.
    /// - 409: email already exists.
    /// </remarks>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status409Conflict)]
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
            ProblemDetails problem = problemDetailsFactory.CreateConflictProblem(HttpContext, ErrorCatalog.UserAlreadyExists);
            return Conflict(problem);
        }

        return Ok(ToResponse(result.Tokens!));
    }

    /// <summary>
    /// Authenticates a user and creates a new device session.
    /// </summary>
    /// <param name="request">Login payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Access and refresh tokens for the authenticated session.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: credentials valid and session created.
    /// - 401: invalid credentials.
    /// </remarks>
    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
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

        return Ok(ToResponse(result.Tokens!));
    }

    /// <summary>
    /// Rotates the refresh token for an existing session.
    /// </summary>
    /// <param name="request">Refresh payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>A newly rotated access and refresh token pair.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: refresh token valid and rotated.
    /// - 401: refresh token invalid or expired.
    /// </remarks>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        AuthResult result = await authService.RefreshAsync(
            new RefreshSessionCommand(
                request.RefreshToken,
                Request.Headers.UserAgent.ToString(),
                HttpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken);

        if (!result.Succeeded)
        {
            ProblemDetails problem = problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.InvalidRefreshToken);
            return Unauthorized(problem);
        }

        return Ok(ToResponse(result.Tokens!));
    }

    /// <summary>
    /// Revokes the current session.
    /// </summary>
    /// <param name="request">Logout payload containing the current refresh token.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the session is revoked or already absent.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    /// </remarks>
    [Authorize]
    [HttpPost("logout")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        await authService.LogoutAsync(new LogoutCommand(userId.Value, request.RefreshToken), cancellationToken);
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
    /// <param name="request">Payload containing the refresh token of the session that must remain active.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the operation completes.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    /// </remarks>
    [Authorize]
    [HttpDelete("sessions/other")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeOtherSessions(
        [FromBody] RevokeOtherSessionsRequest request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        await authService.RevokeOtherSessionsAsync(
            new RevokeOtherSessionsCommand(userId.Value, request.CurrentRefreshToken),
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

    private static AuthResponse ToResponse(AuthTokens tokens)
    {
        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken
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
