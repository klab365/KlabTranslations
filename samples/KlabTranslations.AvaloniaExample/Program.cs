using System;
using System.Globalization;
using Avalonia;
using KlabTranslations.AvaloniaExample.Translations;
using KlabTranslations.Core;

namespace KlabTranslations.AvaloniaExample;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        TranslationProvider.SetCulture(CultureInfo.GetCultureInfo("en-US"));

        Console.WriteLine($"Current value of HelloWorld: {Strings.hello_world.CurrentValue}");

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
