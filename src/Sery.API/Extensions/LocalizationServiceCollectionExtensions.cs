using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Sery.API.Common;

namespace Sery.API.Extensions;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddApiLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddSingleton<IApiProblemDetailsFactory, ApiProblemDetailsFactory>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            string[] supported = ["en", "en-US", "es", "es-MX"];
            options.SetDefaultCulture("en")
                .AddSupportedCultures(supported)
                .AddSupportedUICultures(supported);

            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            ];
        });

        return services;
    }
}
