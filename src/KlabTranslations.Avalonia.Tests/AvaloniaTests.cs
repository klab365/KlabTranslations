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

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithParameterBinding_ShouldShowBoundParameterValue()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "TestUser" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();

        // assert - should show the bound UserName value
        window.BoundUserNameTextBlock.Text.Should().Be("Welcome TestUser!");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithParameterBinding_ShouldUpdateWhenParameterChanges()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "Alice" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();
        window.BoundUserNameTextBlock.Text.Should().Be("Welcome Alice!");

        // act - change the parameter
        viewModel.UserName = "Bob";

        // assert - should update the translation
        window.BoundUserNameTextBlock.Text.Should().Be("Welcome Bob!");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithIntParameterBinding_ShouldShowBoundIntValue()
    {
        // arrange
        MainWindowViewModel viewModel = new() { MessageCount = 5 };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();

        // assert - should show the bound MessageCount value
        window.BoundMessageCountTextBlock.Text.Should().Be("You have 5 messages");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithIntParameterBinding_ShouldUpdateWhenParameterChanges()
    {
        // arrange
        MainWindowViewModel viewModel = new() { MessageCount = 3 };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();
        window.BoundMessageCountTextBlock.Text.Should().Be("You have 3 messages");

        // act - change the parameter
        viewModel.MessageCount = 10;

        // assert - should update the translation
        window.BoundMessageCountTextBlock.Text.Should().Be("You have 10 messages");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithMixedParameters_ShouldShowBoundAndStaticValues()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "Charlie" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();

        // assert - should show bound UserName and static age 42
        window.BoundMixedTextBlock.Text.Should().Be("Charlie is 42 years old");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithMixedParameters_ShouldUpdateWhenBoundParameterChanges()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "Diana" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();
        window.BoundMixedTextBlock.Text.Should().Be("Diana is 42 years old");

        // act - change the bound parameter
        viewModel.UserName = "Eve";

        // assert - should update with new bound value, static value stays the same
        window.BoundMixedTextBlock.Text.Should().Be("Eve is 42 years old");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithParameterBinding_ShouldUpdateTranslationOnCultureChange()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "Frank" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();
        window.BoundUserNameTextBlock.Text.Should().Be("Welcome Frank!");

        // act - switch to German culture
        window.BtnSetCultureGerman.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // assert - should show German translation with bound parameter
        window.BoundUserNameTextBlock.Text.Should().Be("Willkommen Frank!");
    }

    [AvaloniaFact]
    public void TranslateMarkupExtension_WithParameterBinding_ShouldUpdateBothTranslationAndParameterOnChange()
    {
        // arrange
        MainWindowViewModel viewModel = new() { UserName = "Grace" };
        MainWindow window = new()
        {
            DataContext = viewModel
        };

        window.Show();
        window.BoundUserNameTextBlock.Text.Should().Be("Welcome Grace!");

        // act - switch to German culture
        window.BtnSetCultureGerman.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // assert - should show German translation
        window.BoundUserNameTextBlock.Text.Should().Be("Willkommen Grace!");

        // act - change the parameter while in German
        viewModel.UserName = "Henry";

        // assert - should update with new parameter in German
        window.BoundUserNameTextBlock.Text.Should().Be("Willkommen Henry!");
    }
}

#pragma warning restore CA1822
