using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
using Sery.API.Contracts.Chat;
using Sery.Application.Chat;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Exposes chat endpoints for streaming user messages with Server-Sent Events.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ChatController(
    IChatMessageService chatMessageService,
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    /// Streams AI response using Server-Sent Events (SSE).
    /// </summary>
    /// <remarks>
    /// Response is sent as a stream of events.
    ///
    /// Each event follows:
    /// data: {"t":"text chunk","f":false,"c":"conversationId"}
    ///
    /// Final event:
    /// data: {"t":"","f":true,"c":"conversationId"}
    ///
    /// Content-Type: text/event-stream
    /// </remarks>
    [HttpPost("stream")]
    [Produces("text/event-stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    public async Task GetStream(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Message))
        {
            ProblemDetails problem = problemDetailsFactory.CreateValidationProblem(
                HttpContext,
                ErrorCatalog.RequiredUserIdAndMessage);

            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(problem, cancellationToken);
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var command = new QueueMessageCommand(request.UserId, request.Message);

        await foreach (StreamChunkDto chunk in chatMessageService.QueueMessageStreamAsync(command, cancellationToken))
        {
            string json = System.Text.Json.JsonSerializer.Serialize(chunk);
            await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
