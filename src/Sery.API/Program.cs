using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Sery.API.Configuration;
using Sery.API.Extensions;
using Sery.API.Swagger;
using Sery.Application;
using Sery.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

{
    string? dir = Directory.GetCurrentDirectory();
    for (int i = 0; i < 8 && dir is not null; i++)
    {
        string path = Path.Combine(dir, ".env");
        if (File.Exists(path))
        {
            DotNetEnv.Env.Load(path);
            break;
        }

        dir = Directory.GetParent(dir)?.FullName;
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApiFoundation(builder.Configuration);
builder.Services.AddApiLocalization();
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = Sery.API.Common.ApiVersions.Version1;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.Configure<AuthSecurityOptions>(
    builder.Configuration.GetSection(AuthSecurityOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")))
        };
    });

// ── CORS ────────────────────────────────────────────────────────────────
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SeryFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── Rate Limiting ───────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth-login", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: GetLoginPartitionKey(context),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(2),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            }));

    options.AddPolicy("auth-register", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: GetIpPartitionKey(context),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                SegmentsPerWindow = 5,
                QueueLimit = 0
            }));

    options.AddPolicy("auth-refresh", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: GetIpPartitionKey(context),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 2,
                QueueLimit = 0
            }));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();

        foreach (ApiVersionDescription description in descriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Sery API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

// ── Security Headers ────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("X-XSS-Protection", "0");
    context.Response.Headers.Remove("Server");

    if (!context.Request.Host.Host.Contains("localhost"))
    {
        context.Response.Headers.Append(
            "Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLocalization();
app.UseCors("SeryFrontend");
app.UseRateLimiter();
app.UseApiFoundation();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

// ── Rate Limiter Helpers ────────────────────────────────────────────────

static string GetIpPartitionKey(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static string GetLoginPartitionKey(HttpContext context)
{
    string ip = GetIpPartitionKey(context);

    if (context.Request.ContentType?.Contains("application/json") is true)
    {
        context.Request.EnableBuffering();
        try
        {
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            string body = reader.ReadToEndAsync().GetAwaiter().GetResult();
            context.Request.Body.Position = 0;

            var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("email", out var emailProp))
            {
                string email = emailProp.GetString()?.Trim().ToLowerInvariant() ?? "";
                if (!string.IsNullOrEmpty(email))
                {
                    return $"{ip}|{email}";
                }
            }
        }
        catch
        {
            // Fall through to IP-only partition on parse failure.
        }
    }

    return ip;
}

public abstract partial class Program;
