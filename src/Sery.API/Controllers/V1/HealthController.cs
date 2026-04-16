using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Contracts.Health;

namespace Sery.API.Controllers.V1;

/// <summary>
/// Provides API health and liveness status.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
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
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public IActionResult Get()
    {
        return Ok(new HealthResponse
        {
            Status = "ok",
            Service = "Sery.API"
        });
    }
}
