using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sery.API.Contracts.Auth;
using Sery.API.Contracts.Common;
using Sery.API.Contracts.Conversations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sery.API.Swagger;

public sealed class ApiExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string path = "/" + context.ApiDescription.RelativePath?.TrimStart('/').ToLowerInvariant();
        string method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

        ApplyAuthExamples(path, method, operation, context);
        ApplyChatExamples(path, method, operation, context);
        ApplyConversationExamples(path, method, operation, context);
    }

    private static void ApplyAuthExamples(
        string path,
        string method,
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (method == "POST" && path == "/api/v{version:apiversion}/auth/register")
        {
            SetJsonRequestExample(operation, CreateRegisterExample());
            SetJsonResponseExample(operation, context, "200", typeof(AuthResponse), CreateAuthResponseExample());
            SetProblemResponseExample(operation, context, "409", CreateProblemExample(
                "https://httpstatuses.com/409",
                "User already exists.",
                409,
                "User already exists.",
                "/api/v1/auth/register",
                "SERY-API-409-001",
                "error.auth.user_exists"));
            return;
        }

        if (method == "POST" && path == "/api/v{version:apiversion}/auth/login")
        {
            SetJsonRequestExample(operation, CreateLoginExample());
            SetJsonResponseExample(operation, context, "200", typeof(AuthResponse), CreateAuthResponseExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Invalid credentials.",
                401,
                "Invalid credentials.",
                "/api/v1/auth/login",
                "SERY-API-401-001",
                "error.auth.invalid_credentials"));
            return;
        }

        if (method == "POST" && path == "/api/v{version:apiversion}/auth/refresh")
        {
            SetJsonResponseExample(operation, context, "200", typeof(AuthResponse), CreateAuthResponseExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Invalid or expired refresh token.",
                401,
                "Invalid or expired refresh token.",
                "/api/v1/auth/refresh",
                "SERY-API-401-002",
                "error.auth.invalid_refresh_token"));
            return;
        }

        if (method == "POST" && path == "/api/v{version:apiversion}/auth/logout")
        {
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/auth/logout",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            return;
        }

        if (method == "GET" && path == "/api/v{version:apiversion}/auth/sessions")
        {
            SetJsonArrayResponseExample(operation, context, "200", typeof(SessionResponse), CreateSessionArrayExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/auth/sessions",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            return;
        }

        if (method == "DELETE" && path == "/api/v{version:apiversion}/auth/sessions/{id}")
        {
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/auth/sessions/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            return;
        }

        if (method == "DELETE" && path == "/api/v{version:apiversion}/auth/sessions/other")
        {
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/auth/sessions/other",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            return;
        }

        if (method == "GET" && path == "/api/v{version:apiversion}/auth/me")
        {
            SetJsonResponseExample(operation, context, "200", typeof(MeResponse), CreateMeExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/auth/me",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
        }
    }

    private static void ApplyChatExamples(
        string path,
        string method,
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (method != "POST" || path != "/api/v{version:apiversion}/chat/stream")
        {
            return;
        }

        SetJsonRequestExample(operation, CreateChatStreamExample());
        SetSseResponseExample(operation, "200");
        SetProblemResponseExample(operation, context, "400", CreateProblemExample(
            "https://httpstatuses.com/400",
            "Validation failed.",
            400,
            "Message is required.",
            "/api/v1/chat/stream",
            "SERY-API-400-002",
            "error.chat.required_message"));
        SetProblemResponseExample(operation, context, "401", CreateProblemExample(
            "https://httpstatuses.com/401",
            "Authentication is required.",
            401,
            "Authentication is required.",
            "/api/v1/chat/stream",
            "SERY-API-401-003",
            "error.auth.unauthorized"));
    }

    private static void ApplyConversationExamples(
        string path,
        string method,
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (method == "GET" && path == "/api/v{version:apiversion}/conversations")
        {
            SetJsonArrayResponseExample(
                operation,
                context,
                "200",
                typeof(ConversationListItemResponse),
                CreateConversationListExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/conversations",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            return;
        }

        if (method == "PATCH" && path == "/api/v{version:apiversion}/conversations/{id}")
        {
            SetJsonRequestExample(operation, CreateConversationUpdateExample());
            SetJsonResponseExample(
                operation,
                context,
                "200",
                typeof(ConversationDetailsResponse),
                CreateConversationDetailsExample());
            SetProblemResponseExample(operation, context, "400", CreateProblemExample(
                "https://httpstatuses.com/400",
                "Validation failed.",
                400,
                "Conversation title must not be empty and must be shorter than 120 characters.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-400-004",
                "error.conversation.invalid_title"));
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            SetProblemResponseExample(operation, context, "404", CreateProblemExample(
                "https://httpstatuses.com/404",
                "Conversation was not found.",
                404,
                "Conversation was not found.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-404-001",
                "error.conversation.not_found"));
            return;
        }

        if (method == "GET" && path == "/api/v{version:apiversion}/conversations/{id}")
        {
            SetJsonResponseExample(
                operation,
                context,
                "200",
                typeof(ConversationDetailsResponse),
                CreateConversationDetailsExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            SetProblemResponseExample(operation, context, "404", CreateProblemExample(
                "https://httpstatuses.com/404",
                "Conversation was not found.",
                404,
                "Conversation was not found.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-404-001",
                "error.conversation.not_found"));
            return;
        }

        if (method == "GET" && path == "/api/v{version:apiversion}/conversations/{id}/messages")
        {
            SetJsonArrayResponseExample(
                operation,
                context,
                "200",
                typeof(ConversationMessageResponse),
                CreateConversationMessagesExample());
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38/messages",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            SetProblemResponseExample(operation, context, "404", CreateProblemExample(
                "https://httpstatuses.com/404",
                "Conversation was not found.",
                404,
                "Conversation was not found.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38/messages",
                "SERY-API-404-001",
                "error.conversation.not_found"));
            return;
        }

        if (method == "DELETE" && path == "/api/v{version:apiversion}/conversations/{id}")
        {
            SetProblemResponseExample(operation, context, "401", CreateProblemExample(
                "https://httpstatuses.com/401",
                "Authentication is required.",
                401,
                "Authentication is required.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-401-003",
                "error.auth.unauthorized"));
            SetProblemResponseExample(operation, context, "404", CreateProblemExample(
                "https://httpstatuses.com/404",
                "Conversation was not found.",
                404,
                "Conversation was not found.",
                "/api/v1/conversations/1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38",
                "SERY-API-404-001",
                "error.conversation.not_found"));
        }
    }

    private static void SetJsonRequestExample(OpenApiOperation operation, OpenApiObject example)
    {
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out OpenApiMediaType? mediaType) == true)
        {
            mediaType.Example = example;
        }
    }

    private static void SetJsonResponseExample(
        OpenApiOperation operation,
        OperationFilterContext context,
        string statusCode,
        Type responseType,
        OpenApiObject example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
        {
            return;
        }

        OpenApiSchema schema = context.SchemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
        response.Content.Clear();
        response.Content["application/json"] = new OpenApiMediaType
        {
            Schema = schema,
            Example = example
        };
    }

    private static void SetJsonArrayResponseExample(
        OpenApiOperation operation,
        OperationFilterContext context,
        string statusCode,
        Type itemType,
        OpenApiArray example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
        {
            return;
        }

        OpenApiSchema itemSchema = context.SchemaGenerator.GenerateSchema(itemType, context.SchemaRepository);
        response.Content.Clear();
        response.Content["application/json"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "array",
                Items = itemSchema
            },
            Example = example
        };
    }

    private static void SetProblemResponseExample(
        OpenApiOperation operation,
        OperationFilterContext context,
        string statusCode,
        OpenApiObject example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
        {
            return;
        }

        OpenApiSchema schema = context.SchemaGenerator.GenerateSchema(typeof(ApiProblemDetailsResponse), context.SchemaRepository);
        response.Content.Clear();
        response.Content["application/problem+json"] = new OpenApiMediaType
        {
            Schema = schema,
            Example = example
        };
        response.Content["application/json"] = new OpenApiMediaType
        {
            Schema = schema,
            Example = example
        };
    }

    private static void SetSseResponseExample(OpenApiOperation operation, string statusCode)
    {
        if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
        {
            return;
        }

        response.Content.Clear();
        response.Content["text/event-stream"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "string"
            },
            Example = new OpenApiString(
                "data: {\"t\":\"Hello \",\"f\":false,\"c\":\"11111111-1111-1111-1111-111111111111\"}\n\n" +
                "data: {\"t\":\"from integration stub\",\"f\":false,\"c\":\"11111111-1111-1111-1111-111111111111\"}\n\n" +
                "data: {\"t\":\"\",\"f\":true,\"c\":\"11111111-1111-1111-1111-111111111111\"}\n\n")
        };
    }

    private static OpenApiObject CreateRegisterExample()
    {
        return new OpenApiObject
        {
            ["email"] = new OpenApiString("user@example.com"),
            ["password"] = new OpenApiString("P@ssw0rd123!")
        };
    }

    private static OpenApiObject CreateLoginExample() => CreateRegisterExample();



    private static OpenApiObject CreateChatStreamExample()
    {
        return new OpenApiObject
        {
            ["message"] = new OpenApiString("Hola Sery, hoy me siento nervioso."),
            ["conversationId"] = new OpenApiString("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38")
        };
    }

    private static OpenApiObject CreateAuthResponseExample()
    {
        return new OpenApiObject
        {
            ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")
        };
    }

    private static OpenApiArray CreateSessionArrayExample()
    {
        return
        [
            new OpenApiObject
            {
                ["id"] = new OpenApiString("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38"),
                ["deviceInfo"] = new OpenApiString("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36"),
                ["ipAddress"] = new OpenApiString("203.0.113.10"),
                ["createdAt"] = new OpenApiString("2026-04-21T20:56:52Z"),
                ["expiresAt"] = new OpenApiString("2026-04-28T20:56:52Z")
            }
        ];
    }

    private static OpenApiObject CreateMeExample()
    {
        return new OpenApiObject
        {
            ["userId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["email"] = new OpenApiString("user@example.com"),
            ["tenantId"] = new OpenApiString("22222222-2222-2222-2222-222222222222")
        };
    }

    private static OpenApiArray CreateConversationListExample()
    {
        return
        [
            new OpenApiObject
            {
                ["id"] = new OpenApiString("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38"),
                ["title"] = new OpenApiString("Anxiety before tomorrow's meeting"),
                ["summary"] = new OpenApiString("The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues."),
                ["createdAt"] = new OpenApiString("2026-04-21T20:56:52Z"),
                ["updatedAt"] = new OpenApiString("2026-04-21T21:01:18Z"),
                ["lastMessageAt"] = new OpenApiString("2026-04-21T21:01:18Z"),
                ["preview"] = new OpenApiString("Respira conmigo. Vamos paso a paso."),
                ["messageCount"] = new OpenApiInteger(14),
                ["isArchived"] = new OpenApiBoolean(false),
                ["isPinned"] = new OpenApiBoolean(true)
            }
        ];
    }

    private static OpenApiObject CreateConversationDetailsExample()
    {
        return new OpenApiObject
        {
            ["id"] = new OpenApiString("1c9a18d3-18b3-4f45-8bc3-bcb6b9fe7f38"),
            ["title"] = new OpenApiString("Anxiety before tomorrow's meeting"),
            ["summary"] = new OpenApiString("The conversation focuses on work pressure. The user's recent tone has been mostly anxious. Helpful responses so far use grounding and breathing cues."),
            ["createdAt"] = new OpenApiString("2026-04-21T20:56:52Z"),
            ["updatedAt"] = new OpenApiString("2026-04-21T21:01:18Z"),
            ["lastMessageAt"] = new OpenApiString("2026-04-21T21:01:18Z"),
            ["preview"] = new OpenApiString("Respira conmigo. Vamos paso a paso."),
            ["messageCount"] = new OpenApiInteger(14),
            ["isArchived"] = new OpenApiBoolean(false),
            ["isPinned"] = new OpenApiBoolean(true)
        };
    }

    private static OpenApiObject CreateConversationUpdateExample()
    {
        return new OpenApiObject
        {
            ["title"] = new OpenApiString("Plan para bajar mi ansiedad esta semana"),
            ["isArchived"] = new OpenApiBoolean(false),
            ["isPinned"] = new OpenApiBoolean(true)
        };
    }

    private static OpenApiArray CreateConversationMessagesExample()
    {
        return
        [
            new OpenApiObject
            {
                ["id"] = new OpenApiString("3c1b26d0-84f7-40f3-a3a8-00a517d9bf03"),
                ["role"] = new OpenApiString("User"),
                ["content"] = new OpenApiString("Hola Sery, hoy me siento nervioso."),
                ["createdAt"] = new OpenApiString("2026-04-21T20:57:00Z")
            },
            new OpenApiObject
            {
                ["id"] = new OpenApiString("f8a44a6d-bef2-4a08-96ca-1ae3ff8d2821"),
                ["role"] = new OpenApiString("Assistant"),
                ["content"] = new OpenApiString("Estoy contigo. Vamos a bajar el ritmo un momento."),
                ["createdAt"] = new OpenApiString("2026-04-21T20:57:03Z")
            }
        ];
    }

    private static OpenApiObject CreateProblemExample(
        string type,
        string title,
        int status,
        string detail,
        string instance,
        string code,
        string messageKey)
    {
        return new OpenApiObject
        {
            ["type"] = new OpenApiString(type),
            ["title"] = new OpenApiString(title),
            ["status"] = new OpenApiInteger(status),
            ["detail"] = new OpenApiString(detail),
            ["instance"] = new OpenApiString(instance),
            ["code"] = new OpenApiString(code),
            ["messageKey"] = new OpenApiString(messageKey)
        };
    }
}
