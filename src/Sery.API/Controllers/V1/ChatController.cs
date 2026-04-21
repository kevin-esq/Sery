using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
using Sery.API.Contracts.Chat;
using Sery.API.Contracts.Common;
using Sery.Application.Chat;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Exposes chat endpoints for streaming user messages with Server-Sent Events.
/// </summary>
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/chat")]
public sealed class ChatController(
    IChatMessageService chatMessageService,
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    /// Streams AI response using Server-Sent Events (SSE).
    /// </summary>
    /// <param name="request">Chat message payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 200: stream opened successfully and SSE events are emitted.
    /// - 400: message is missing or blank. Error code: `SERY-API-400-002`.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// </remarks>
    [Authorize]
    [HttpPost("stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task GetStream(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            ProblemDetails problem = problemDetailsFactory.CreateValidationProblem(
                HttpContext,
                ErrorCatalog.RequiredMessage);

            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(problem, cancellationToken);
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var command = new QueueMessageCommand(request.Message);

        await foreach (StreamChunkDto chunk in chatMessageService.QueueMessageStreamAsync(command, cancellationToken))
        {
            string json = System.Text.Json.JsonSerializer.Serialize(chunk);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
