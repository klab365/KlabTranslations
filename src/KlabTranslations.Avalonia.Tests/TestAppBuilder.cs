using Avalonia;
using Avalonia.Headless;
using KlabTranslations.Avalonia.Tests;
using KlabTranslations.AvaloniaExample;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace KlabTranslations.Avalonia.Tests;


public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
