namespace Sery.API.Common;

public static class ErrorCatalog
{
    public static readonly ApiError ValidationFailed = new(
        "SERY-API-400-001",
        "error.validation.failed",
        "Validation failed.");

    public static readonly ApiError RequiredUserIdAndMessage = new(
        "SERY-API-400-002",
        "error.chat.required_userid_message",
        "UserId and Message are required.");

    public static readonly ApiError UnexpectedError = new(
        "SERY-API-500-001",
        "error.unexpected",
        "An unexpected error occurred.");
}

public sealed record ApiError(string Code, string MessageKey, string DefaultMessage);
