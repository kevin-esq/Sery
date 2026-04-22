namespace Sery.API.Common;

public static class ErrorCatalog
{
    public static readonly ApiError ValidationFailed = new(
        "SERY-API-400-001",
        "error.validation.failed",
        "Validation failed.");

    public static readonly ApiError RequiredMessage = new(
        "SERY-API-400-002",
        "error.chat.required_message",
        "Message is required.");

    public static readonly ApiError InvalidConversationUpdate = new(
        "SERY-API-400-003",
        "error.conversation.invalid_update",
        "At least one conversation field must be provided.");

    public static readonly ApiError InvalidConversationTitle = new(
        "SERY-API-400-004",
        "error.conversation.invalid_title",
        "Conversation title must not be empty and must be shorter than 120 characters.");

    public static readonly ApiError InvalidAgentCustomization = new(
        "SERY-API-400-005",
        "error.agent.invalid_customization",
        "Agent customization payload is invalid.");

    public static readonly ApiError UserAlreadyExists = new(
        "SERY-API-409-001",
        "error.auth.user_exists",
        "User already exists.");

    public static readonly ApiError ConversationNotFound = new(
        "SERY-API-404-001",
        "error.conversation.not_found",
        "Conversation was not found.");

    public static readonly ApiError InvalidCredentials = new(
        "SERY-API-401-001",
        "error.auth.invalid_credentials",
        "Invalid credentials.");

    public static readonly ApiError InvalidRefreshToken = new(
        "SERY-API-401-002",
        "error.auth.invalid_refresh_token",
        "Invalid or expired refresh token.");

    public static readonly ApiError Unauthorized = new(
        "SERY-API-401-003",
        "error.auth.unauthorized",
        "Authentication is required.");

    public static readonly ApiError UnexpectedError = new(
        "SERY-API-500-001",
        "error.unexpected",
        "An unexpected error occurred.");
}

public sealed record ApiError(string Code, string MessageKey, string DefaultMessage);
