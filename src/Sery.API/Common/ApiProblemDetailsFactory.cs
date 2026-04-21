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
        return CreateProblem(
            context,
            StatusCodes.Status400BadRequest,
            ErrorCatalog.ValidationFailed,
            error,
            detailOverride);
    }

    public ProblemDetails CreateConflictProblem(HttpContext context, ApiError error, string? detailOverride = null)
    {
        return CreateProblem(
            context,
            StatusCodes.Status409Conflict,
            error,
            error,
            detailOverride);
    }

    public ProblemDetails CreateUnauthorizedProblem(HttpContext context, ApiError error, string? detailOverride = null)
    {
        return CreateProblem(
            context,
            StatusCodes.Status401Unauthorized,
            error,
            error,
            detailOverride);
    }

    public ProblemDetails CreateUnexpectedErrorProblem(
        HttpContext context,
        ApiError error,
        string? detailOverride = null)
    {
        return CreateProblem(
            context,
            StatusCodes.Status500InternalServerError,
            error,
            error,
            detailOverride);
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

    private static ProblemDetails CreateProblem(
        HttpContext context,
        int statusCode,
        ApiError titleError,
        ApiError detailError,
        string? detailOverride)
    {
        string title = Localize(titleError.MessageKey, titleError.DefaultMessage);
        string detail = detailOverride ?? Localize(detailError.MessageKey, detailError.DefaultMessage);

        return new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["code"] = detailError.Code,
                ["messageKey"] = detailError.MessageKey
            }
        };
    }
}
