using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Common;
using Sery.API.Contracts.AgentCustomization;
using Sery.API.Contracts.Common;
using Sery.Application.AgentCustomization;
using Sery.Application.Common.Interfaces;

namespace Sery.API.Controllers.V1;

[ApiController]
[Authorize]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/agent-profile")]
public sealed class AgentCustomizationController(
    IAgentCustomizationService agentCustomizationService,
    IUserContext userContext,
    IApiProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AgentCustomizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AgentCustomizationResponse>> Get(CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        AgentCustomizationDto customization = await agentCustomizationService.GetAsync(userId.Value, cancellationToken);
        return Ok(ToResponse(customization));
    }

    [HttpPatch]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AgentCustomizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AgentCustomizationResponse>> Update(
        [FromBody] UpdateAgentCustomizationRequest request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        string? validationError = Validate(request);
        if (validationError is not null)
        {
            return BadRequest(problemDetailsFactory.CreateValidationProblem(
                HttpContext,
                ErrorCatalog.InvalidAgentCustomization,
                validationError));
        }

        AgentCustomizationDto customization = await agentCustomizationService.UpdateAsync(
            userId.Value,
            new UpdateAgentCustomizationCommand(
                request.IdentityPresentation,
                request.CoreDemeanor,
                request.Warmth,
                request.Directness,
                request.Sincerity,
                request.Charisma,
                request.Playfulness,
                request.Reflection,
                request.Proactivity,
                request.EmotionalExpressiveness,
                request.PreferredResponseLength,
                request.AskFollowUpQuestions,
                request.OfferActionSteps,
                request.RelationshipMode,
                request.Closeness,
                request.Tenderness,
                request.Protectiveness,
                request.Flirtiness,
                request.UsesAffectionateLanguage,
                request.AllowsRomanticFraming,
                request.PrioritizeSupportOverRoleplay),
            cancellationToken);

        return Ok(ToResponse(customization));
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiProblemDetailsResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;
        if (userId is null)
        {
            return Unauthorized(problemDetailsFactory.CreateUnauthorizedProblem(HttpContext, ErrorCatalog.Unauthorized));
        }

        await agentCustomizationService.ResetAsync(userId.Value, cancellationToken);
        return NoContent();
    }

    private static AgentCustomizationResponse ToResponse(AgentCustomizationDto customization)
    {
        return new AgentCustomizationResponse
        {
            AgentName = customization.AgentName,
            IdentityPresentation = customization.IdentityPresentation,
            CoreDemeanor = customization.CoreDemeanor,
            Warmth = customization.Warmth,
            Directness = customization.Directness,
            Sincerity = customization.Sincerity,
            Charisma = customization.Charisma,
            Playfulness = customization.Playfulness,
            Reflection = customization.Reflection,
            Proactivity = customization.Proactivity,
            EmotionalExpressiveness = customization.EmotionalExpressiveness,
            PreferredResponseLength = customization.PreferredResponseLength,
            AskFollowUpQuestions = customization.AskFollowUpQuestions,
            OfferActionSteps = customization.OfferActionSteps,
            RelationshipMode = customization.RelationshipMode,
            Closeness = customization.Closeness,
            Tenderness = customization.Tenderness,
            Protectiveness = customization.Protectiveness,
            Flirtiness = customization.Flirtiness,
            UsesAffectionateLanguage = customization.UsesAffectionateLanguage,
            AllowsRomanticFraming = customization.AllowsRomanticFraming,
            PrioritizeSupportOverRoleplay = customization.PrioritizeSupportOverRoleplay,
            IsCustomized = customization.IsCustomized,
            UpdatedAt = customization.UpdatedAt
        };
    }

    private static string? Validate(UpdateAgentCustomizationRequest request)
    {
        if (HasAnyValueOutsideRange(request.Warmth, request.Directness, request.Sincerity, request.Charisma, request.Playfulness,
                request.Reflection, request.Proactivity, request.EmotionalExpressiveness, request.Closeness, request.Tenderness,
                request.Protectiveness, request.Flirtiness))
        {
            return "Numeric customization values must be between 0 and 100.";
        }

        if (request.IdentityPresentation is not null &&
            request.IdentityPresentation is not ("neutral" or "feminine" or "masculine"))
        {
            return "IdentityPresentation must be one of: neutral, feminine, masculine.";
        }

        if (request.PreferredResponseLength is not null &&
            request.PreferredResponseLength is not ("short" or "medium" or "medium-long" or "long"))
        {
            return "PreferredResponseLength must be one of: short, medium, medium-long, long.";
        }

        if (request.RelationshipMode is not null &&
            request.RelationshipMode.ToLowerInvariant() is not ("listener" or "friend" or "closecompanion" or "close_companion" or "close-companion" or "romanticcompanion" or "romantic_companion" or "romantic-companion" or "supportcoach" or "support_coach" or "support-coach"))
        {
            return "RelationshipMode must be one of: listener, friend, close-companion, romantic-companion, support-coach.";
        }

        if (request.CoreDemeanor is not null && string.IsNullOrWhiteSpace(request.CoreDemeanor))
        {
            return "CoreDemeanor cannot be empty.";
        }

        return null;
    }

    private static bool HasAnyValueOutsideRange(params int?[] values)
    {
        return values.Any(x => x is < 0 or > 100);
    }
}
