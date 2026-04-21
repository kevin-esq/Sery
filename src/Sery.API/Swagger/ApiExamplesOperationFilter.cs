using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sery.API.Contracts.Auth;
using Sery.API.Contracts.Common;
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
            SetJsonRequestExample(operation, CreateRefreshExample());
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
            SetJsonRequestExample(operation, CreateRefreshExample());
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
            SetJsonRequestExample(operation, CreateRevokeOthersExample());
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

    private static OpenApiObject CreateRefreshExample()
    {
        return new OpenApiObject
        {
            ["refreshToken"] = new OpenApiString("9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...")
        };
    }

    private static OpenApiObject CreateRevokeOthersExample()
    {
        return new OpenApiObject
        {
            ["currentRefreshToken"] = new OpenApiString("9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...")
        };
    }

    private static OpenApiObject CreateAuthResponseExample()
    {
        return new OpenApiObject
        {
            ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
            ["refreshToken"] = new OpenApiString("9kV6j7YwQm5m7pP4Q9Q4lX2fB1P5vC1e8vQv2d4...")
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
