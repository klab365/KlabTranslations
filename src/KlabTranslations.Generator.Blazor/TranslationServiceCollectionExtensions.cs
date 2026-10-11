using KlabTranslations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace KlabTranslations.Generator.Blazor;

/// <summary>Dependency-injection setup for Blazor translations.</summary>
public static class TranslationServiceCollectionExtensions
{
    /// <summary>Registers an independent translation culture context per Blazor circuit or request.</summary>
    public static IServiceCollection AddKlabTranslations(this IServiceCollection services)
    {
        services.AddScoped(_ => new TranslationContext());
        return services;
    }
}
