using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
using Sery.API.Contracts.Common;
using Sery.API.Contracts.Conversations;
using Sery.Application.Common.Interfaces;
using Sery.Application.Conversations;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Exposes conversation management endpoints for the authenticated user.
/// </summary>
[ApiController]
[Authorize]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/conversations")]
public sealed class ConversationsController(
    IConversationService conversationService,
    IUserContext userContext,
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    /// Lists the user's conversations ordered by most recent activity.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Conversation summaries for the authenticated user.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 200: conversations returned successfully.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// </remarks>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ConversationListItemResponse>>> GetConversations(
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        IReadOnlyList<ConversationListItemDto> conversations =
            await conversationService.GetConversationsAsync(userId.Value, cancellationToken);

        return Ok(conversations.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Returns metadata for a specific conversation.
    /// </summary>
    /// <param name="id">Conversation identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The conversation metadata if it belongs to the authenticated user.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 200: conversation found.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// - 404: conversation does not exist for the current user. Error code: `SERY-API-404-001`.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ConversationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDetailsResponse>> GetConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        ConversationDetailsDto? conversation =
            await conversationService.GetConversationAsync(userId.Value, id, cancellationToken);

        if (conversation is null)
        {
            return NotFound(problemDetailsFactory.CreateNotFoundProblem(HttpContext, ErrorCatalog.ConversationNotFound));
        }

        return Ok(ToResponse(conversation));
    }

    /// <summary>
    /// Returns messages for a specific conversation.
    /// </summary>
    /// <param name="id">Conversation identifier.</param>
    /// <param name="skip">Messages to skip from the beginning of the timeline.</param>
    /// <param name="take">Maximum number of messages to return.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The message timeline for the target conversation.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 200: messages returned successfully.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// - 404: conversation does not exist for the current user. Error code: `SERY-API-404-001`.
    /// </remarks>
    [HttpGet("{id:guid}/messages")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationMessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ConversationMessageResponse>>> GetMessages(
        Guid id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        ConversationDetailsDto? conversation =
            await conversationService.GetConversationAsync(userId.Value, id, cancellationToken);

        if (conversation is null)
        {
            return NotFound(problemDetailsFactory.CreateNotFoundProblem(HttpContext, ErrorCatalog.ConversationNotFound));
        }

        IReadOnlyList<ConversationMessageDto> messages =
            await conversationService.GetMessagesAsync(userId.Value, id, skip, take, cancellationToken);

        return Ok(messages.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Renames, archives, or pins a conversation.
    /// </summary>
    /// <param name="id">Conversation identifier.</param>
    /// <param name="request">Conversation metadata changes.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>The updated conversation metadata.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 200: metadata updated successfully.
    /// - 400: invalid payload. Error codes: `SERY-API-400-003`, `SERY-API-400-004`.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// - 404: conversation does not exist for the current user. Error code: `SERY-API-404-001`.
    /// </remarks>
    [HttpPatch("{id:guid}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ConversationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDetailsResponse>> UpdateConversation(
        Guid id,
        [FromBody] UpdateConversationRequest request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        bool hasNoChanges = request.Title is null && request.IsArchived is null && request.IsPinned is null;
        if (hasNoChanges)
        {
            return BadRequest(problemDetailsFactory.CreateValidationProblem(HttpContext, ErrorCatalog.InvalidConversationUpdate));
        }

        if (request.Title is not null &&
            (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 120))
        {
            return BadRequest(problemDetailsFactory.CreateValidationProblem(HttpContext, ErrorCatalog.InvalidConversationTitle));
        }

        ConversationDetailsDto? updated = await conversationService.UpdateMetadataAsync(
            userId.Value,
            id,
            new ConversationMetadataUpdateDto(request.Title, request.IsArchived, request.IsPinned),
            cancellationToken);

        if (updated is null)
        {
            return NotFound(problemDetailsFactory.CreateNotFoundProblem(HttpContext, ErrorCatalog.ConversationNotFound));
        }

        return Ok(ToResponse(updated));
    }

    /// <summary>
    /// Deletes a conversation and its messages.
    /// </summary>
    /// <param name="id">Conversation identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>No content when the conversation is deleted.</returns>
    /// <remarks>
    /// Requires bearer authentication.
    ///
    /// Outcomes:
    /// - 204: conversation deleted successfully.
    /// - 401: caller is not authenticated. Error code: `SERY-API-401-003`.
    /// - 404: conversation does not exist for the current user. Error code: `SERY-API-404-001`.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        bool deleted = await conversationService.DeleteConversationAsync(userId.Value, id, cancellationToken);
        if (!deleted)
        {
            return NotFound(problemDetailsFactory.CreateNotFoundProblem(HttpContext, ErrorCatalog.ConversationNotFound));
        }

        return NoContent();
    }

    private static ConversationListItemResponse ToResponse(ConversationListItemDto conversation)
    {
        return new ConversationListItemResponse
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Summary = conversation.Summary,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt,
            Preview = conversation.Preview,
            MessageCount = conversation.MessageCount,
            IsArchived = conversation.IsArchived,
            IsPinned = conversation.IsPinned
        };
    }

    private static ConversationDetailsResponse ToResponse(ConversationDetailsDto conversation)
    {
        return new ConversationDetailsResponse
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Summary = conversation.Summary,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt,
            Preview = conversation.Preview,
            MessageCount = conversation.MessageCount,
            IsArchived = conversation.IsArchived,
            IsPinned = conversation.IsPinned
        };
    }

    private static ConversationMessageResponse ToResponse(ConversationMessageDto message)
    {
        return new ConversationMessageResponse
        {
            Id = message.Id,
            Role = message.Role,
            Content = message.Content,
            CreatedAt = message.CreatedAt
        };
    }
}
