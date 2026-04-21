using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Contracts.Health;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Provides API health and liveness status.
/// </summary>
[ApiController]
[ApiVersionNeutral]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Returns API liveness status.
    /// </summary>
    /// <returns>Status payload.</returns>
    /// <remarks>
    /// Outcomes:
    /// - 200: API is alive and reachable.
    /// </remarks>
    [HttpGet("api/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public IActionResult Get()
    {
        return Ok(CreateResponse());
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("api/v{version:apiVersion}/health")]
    public IActionResult GetVersionedCompatibility()
    {
        return Ok(CreateResponse());
    }

    private static HealthResponse CreateResponse()
    {
        return new HealthResponse
        {
            Status = "ok",
            Service = "Sery.API"
        };
    }
}
