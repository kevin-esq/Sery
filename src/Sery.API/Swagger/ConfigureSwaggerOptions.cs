using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sery.API.Swagger;

public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

        if (File.Exists(xmlFilePath))
        {
            options.IncludeXmlComments(xmlFilePath);
        }

        options.DocInclusionPredicate((documentName, apiDescription) =>
            string.Equals(apiDescription.GroupName, documentName, StringComparison.OrdinalIgnoreCase));
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        string text = description.IsDeprecated ? " This API version is deprecated." : string.Empty;

        return new OpenApiInfo
        {
            Title = "Sery API",
            Version = description.ApiVersion.ToString(),
            Description = $"HTTP API for Sery. OpenAPI document for {description.GroupName}.{text}"
        };
    }
}
