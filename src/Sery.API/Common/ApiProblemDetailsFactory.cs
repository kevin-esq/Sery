using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Mvc;
using Sery.API.Resources;

namespace Sery.API.Common;

public sealed class ApiProblemDetailsFactory : IApiProblemDetailsFactory
{
    private static readonly ResourceManager ResourceManager = new(
        $"{typeof(ApiMessages).Namespace}.{nameof(ApiMessages)}",
        typeof(ApiMessages).Assembly);

    public ProblemDetails CreateValidationProblem(HttpContext context, ApiError error, string? detailOverride = null)
    {
        string title = Localize(ErrorCatalog.ValidationFailed.MessageKey, ErrorCatalog.ValidationFailed.DefaultMessage);
        string detail = detailOverride ?? Localize(error.MessageKey, error.DefaultMessage);

        var problem = new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest,
            Type = "https://httpstatuses.com/400",
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = error.Code,
                ["messageKey"] = error.MessageKey
            }
        };

        return problem;
    }

    public ProblemDetails CreateUnexpectedErrorProblem(
        HttpContext context,
        ApiError error,
        string? detailOverride = null)
    {
        string title = Localize(error.MessageKey, error.DefaultMessage);

        var problem = new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://httpstatuses.com/500",
            Detail = detailOverride,
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = error.Code,
                ["messageKey"] = error.MessageKey
            }
        };

        return problem;
    }

    private static string Localize(string key, string fallback)
    {
        try
        {
            string? value = ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
        catch (MissingManifestResourceException)
        {
            return fallback;
        }
    }
}
