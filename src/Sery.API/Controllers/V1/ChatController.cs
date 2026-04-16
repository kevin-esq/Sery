using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
using Sery.API.Contracts.Chat;
using Sery.Application.Chat;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Exposes chat endpoints for sending user messages.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ChatController(
    IChatMessageService chatMessageService,
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    /// Queues a chat message to be processed by the application layer.
    /// </summary>
    /// <param name="request">User message payload.</param>
    /// <param name="cancellationToken">Cancellation token for request scope.</param>
    /// <returns>Accepted response with queue metadata.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 202: Message accepted and queued for processing.
    /// - 400: Validation error when UserId is empty or Message is blank.
    /// </remarks>
    [HttpPost("message")]
    [ProducesResponseType(typeof(SendMessageAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Produces("application/json", "application/problem+json")]
    [Consumes("application/json")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Message))
        {
            ProblemDetails problem = problemDetailsFactory.CreateValidationProblem(
                HttpContext,
                ErrorCatalog.RequiredUserIdAndMessage);

            return BadRequest(problem);
        }

        var command = new QueueMessageCommand(request.UserId, request.Message);
        QueueMessageResult result = await chatMessageService.QueueMessageAsync(command, cancellationToken);
        var response = new SendMessageAcceptedResponse
        {
            UserId = result.UserId,
            Message = result.Message,
            Status = result.Status
        };

        return Accepted(response);
    }
}
