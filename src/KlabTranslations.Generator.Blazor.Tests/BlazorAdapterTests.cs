using System.Globalization;
using AwesomeAssertions;
using KlabTranslations.Core;
using KlabTranslations.Generator.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KlabTranslations.Generator.Blazor.Tests;

public sealed class BlazorAdapterTests
{
    [Fact]
    public void AddKlabTranslations_UsesIndependentScopedContexts()
    {
        ServiceCollection services = new();
        services.AddKlabTranslations();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();
        TranslationContext first = firstScope.ServiceProvider.GetRequiredService<TranslationContext>();
        TranslationContext second = secondScope.ServiceProvider.GetRequiredService<TranslationContext>();

        first.Should().NotBeSameAs(second);
        first.SetCulture(new CultureInfo("de-DE"));
        first.CurrentCulture.Name.Should().Be("de-DE");
        second.CurrentCulture.Name.Should().Be(CultureInfo.CurrentUICulture.Name);
    }

    [Fact]
    public async Task Translate_RendersLocalizedValueAndNamedParameters()
    {
        ServiceCollection services = new();
        services.AddKlabTranslations();
        services.AddLogging();
        using ServiceProvider provider = services.BuildServiceProvider();
        TranslationContext context = provider.GetRequiredService<TranslationContext>();
        context.SetCulture(new CultureInfo("en-US"));
        using HtmlRenderer renderer = new(provider, provider.GetRequiredService<ILoggerFactory>());
        TranslationUnit unit = new("greeting", new Dictionary<string, string>
        {
            ["en"] = "Hello {name}",
            ["de"] = "Hallo {name}"
        });

        HtmlRootComponent component = await renderer.Dispatcher.InvokeAsync(() =>
            renderer.RenderComponentAsync<Translate>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(Translate.Unit)] = unit,
                [nameof(Translate.Parameters)] = new Dictionary<string, object?> { ["name"] = "Ada" }
            })));

        (await renderer.Dispatcher.InvokeAsync(component.ToHtmlString)).Should().Contain("Hello Ada");
        unit.Dispose();
    }
}
