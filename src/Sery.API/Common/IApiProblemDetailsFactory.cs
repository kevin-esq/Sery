using Microsoft.AspNetCore.Mvc;

namespace Sery.API.Common;

public interface IApiProblemDetailsFactory
{
    ProblemDetails CreateValidationProblem(HttpContext context, ApiError error, string? detailOverride = null);

    ProblemDetails CreateConflictProblem(HttpContext context, ApiError error, string? detailOverride = null);

    ProblemDetails CreateNotFoundProblem(HttpContext context, ApiError error, string? detailOverride = null);

    ProblemDetails CreateUnauthorizedProblem(HttpContext context, ApiError error, string? detailOverride = null);

    ProblemDetails CreateUnexpectedErrorProblem(HttpContext context, ApiError error, string? detailOverride = null);
}
