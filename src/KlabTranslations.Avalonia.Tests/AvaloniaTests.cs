using System.Globalization;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using AwesomeAssertions;
using KlabTranslations.AvaloniaExample;
using KlabTranslations.AvaloniaExample.ViewModels;
using KlabTranslations.AvaloniaExample.Views;
using KlabTranslations.Core;
using KlabTranslations.Generator.Avalonia;

namespace KlabTranslations.Avalonia.Tests;

#pragma warning disable CA1822

public sealed class AvaloniaTests : IDisposable
{
    public AvaloniaTests()
    {
        // Set to English so constructor doesn't fail
        TranslationProvider.SetCulture(new CultureInfo("en-US"));
    }

    public void Dispose()
    {
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_ShouldShowTheGermanTranslatedText()
    {
        // arrange
        MainWindow window = new()
        {
            DataContext = new MainWindowViewModel()
        };

        window.Show();

        window.ResultTextBlock.Text.Should().Be("Hello World");

        // act
        window.BtnSetCultureGerman.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // assert
        window.ResultTextBlock.Text.Should().Be("Hallo Welt");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_ShouldShowTheEnglishTranslatedTextFromGerman()
    {
        // arrange
        MainWindow window = new()
        {
            DataContext = new MainWindowViewModel()
        };

        window.Show();

        window.ResultTextBlock.Text.Should().Be("Hello World");
        window.BtnSetCultureGerman.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        window.ResultTextBlock.Text.Should().Be("Hallo Welt");

        // act
        window.BtnSetCultureEnglish.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // assert
        window.ResultTextBlock.Text.Should().Be("Hello World");
    }
}

#pragma warning restore CA1822
