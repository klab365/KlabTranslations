using System.Globalization;
using System.Reactive.Linq;
using AwesomeAssertions;
using KlabTranslations.Core;

namespace KlabTranslations.Core.Tests;

#pragma warning disable S1481 // Unused local variables should be removed

[Collection("TranslationProvider")]
public sealed class TranslationUnitTests : IDisposable
{
    private readonly CultureInfo _originalCulture;

    public TranslationUnitTests()
    {
        // Store original culture and set to English for consistent tests
        _originalCulture = TranslationProvider.CurrentCulture;
        TranslationProvider.SetCulture(new CultureInfo("en-US"));
    }

    public void Dispose()
    {
        // Restore original culture to avoid affecting other tests
        TranslationProvider.SetCulture(_originalCulture);
    }

    private static TranslationUnit CreateTranslationUnit(string key, Dictionary<string, string> translations)
    {
        TranslationUnit unit = new(key, translations);
        return unit;
    }

    [Fact]
    public void NamedCountParameter_ShouldSelectCultureSpecificPluralForm()
    {
        Dictionary<CultureInfo, string> translations = new()
        {
            [new CultureInfo("en")] = "{count} messages",
            [new CultureInfo("ru")] = "{count} сообщений"
        };
        Dictionary<CultureInfo, Dictionary<PluralCategory, string>> plurals = new()
        {
            [new CultureInfo("en")] = new()
            {
                [PluralCategory.One] = "{count} message",
                [PluralCategory.Other] = "{count} messages"
            },
            [new CultureInfo("ru")] = new()
            {
                [PluralCategory.One] = "{count} сообщение",
                [PluralCategory.Few] = "{count} сообщения",
                [PluralCategory.Many] = "{count} сообщений",
                [PluralCategory.Other] = "{count} сообщения"
            }
        };
        using TranslationUnit unit = new("message_count", translations, plurals);
        unit.Parameters["count"] = 1;
        unit.CurrentValue.Should().Be("1 message");
        unit.Parameters["count"] = 5;
        unit.CurrentValue.Should().Be("5 messages");
        TranslationProvider.SetCulture(new CultureInfo("ru-RU"));
        unit.Parameters["count"] = 2;
        unit.CurrentValue.Should().Be("2 сообщения");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithTranslationForCurrentCulture()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };

        // Act
        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);

        // Assert
        unit.CurrentValue.Should().Be("Hello"); // Should be English translation, not key
        unit.Key.Should().Be("greeting");
        unit.Translations.Should().HaveCount(2);
        unit.Translations.Values.Should().Contain("Hello").And.Contain("Bonjour");
    }

    [Fact]
    public void Constructor_ShouldInitializeValueObservable()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };

        // Act
        using TranslationUnit unit = new("greeting", translations);
        string initialValue = unit.Value.Take(1).Wait();

        // Assert
        initialValue.Should().Be("Hello"); // Should be English translation
    }

    [Fact]
    public void OnCultureChanged_ShouldUpdateValueWhenTranslationExists()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" },
            { "de", "Hallo" }
        };
        using TranslationUnit unit = new("greeting", translations);
        List<string> valueChanges = new();
        unit.Value.Subscribe(valueChanges.Add);

        // Act
        TranslationProvider.SetCulture(new CultureInfo("fr-FR"));

        // Assert
        unit.CurrentValue.Should().Be("Bonjour");
        valueChanges.Should().Contain("Bonjour");
    }

    [Fact]
    public void OnCultureChanged_ShouldUseInvariantTranslationWhenSelectedCultureIsMissing()
    {
        Dictionary<CultureInfo, string> translations = new()
        {
            [CultureInfo.InvariantCulture] = "Default"
        };
        using TranslationUnit unit = new("greeting", translations);

        TranslationProvider.SetCulture(new CultureInfo("es-ES"));

        unit.CurrentValue.Should().Be("Default");
    }

    [Fact]
    public void OnCultureChanged_ShouldUseKeyWhenCultureAndInvariantTranslationAreMissing()
    {
        Dictionary<string, string> translations = new()
        {
            ["en"] = "Hello"
        };
        using TranslationUnit unit = new("greeting", translations);

        TranslationProvider.SetCulture(new CultureInfo("es-ES"));

        unit.CurrentValue.Should().Be("greeting");
    }

    [Fact]
    public void OnCultureChanged_ShouldThrowWhenTranslationDoesNotExist()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };
        using TranslationUnit unit = new("greeting", translations);

        // Act & Assert
        Action act = () => TranslationProvider.SetCulture(new CultureInfo("es-ES")); // Spanish not in translations
        act.Should().NotThrow();
        unit.CurrentValue.Should().Be("greeting");
    }

    [Fact]
    public void OnCultureChanged_ShouldHandleMultipleCultureChanges()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" },
            { "de", "Hallo" }
        };
        using TranslationUnit unit = new("tmp", translations);
        List<string> valueChanges = new();
        unit.Value.Subscribe(valueChanges.Add);

        // Act
        TranslationProvider.SetCulture(new CultureInfo("fr-FR"));
        TranslationProvider.SetCulture(new CultureInfo("de-DE"));

        // Assert
        unit.CurrentValue.Should().Be("Hallo");
        valueChanges.Should().ContainInOrder("Hello", "Bonjour", "Hallo");
    }

    [Fact]
    public void OnCultureChanged_ShouldUseTwoLetterISOLanguageName()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };
        using TranslationUnit unit = new("greeting", translations);

        // Act
        TranslationProvider.SetCulture(new CultureInfo("en-GB")); // Should use "en"

        // Assert
        unit.CurrentValue.Should().Be("Hello");
    }

    [Fact]
    public void Value_ShouldBeObservableStream()
    {
        // Arrange
        Dictionary<string, string> translations = new()

        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };
        using TranslationUnit unit = new("greeting", translations);
        List<string> receivedValues = new();

        // Act
        using IDisposable subscription = unit.Value.Subscribe(receivedValues.Add);
        TranslationProvider.SetCulture(new CultureInfo("fr-FR"));

        // Assert
        receivedValues.Should().HaveCount(2);
        receivedValues[0].Should().Be("Hello");   // Initial English
        receivedValues[1].Should().Be("Bonjour"); // French
    }

    [Fact]
    public void Constructor_WithEmptyTranslations_ShouldReturnKey()
    {
        // Arrange
        Dictionary<string, string> emptyTranslations = new();

        // Act & Assert
        using TranslationUnit unit = new("test-key", emptyTranslations);
        unit.CurrentValue.Should().Be("test-key");
    }

    [Theory]
    [InlineData("en", "Hello")]
    [InlineData("fr", "Bonjour")]
    [InlineData("de", "Hallo")]
    public void OnCultureChanged_WithExistingCultures_ShouldReturnTranslation(string culture, string expected)
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" },
            { "de", "Hallo" }
        };
        using TranslationUnit unit = new("greeting", translations);

        // Act
        TranslationProvider.SetCulture(new CultureInfo(culture));

        // Assert
        unit.CurrentValue.Should().Be(expected);
    }

    [Theory]
    [InlineData("es")]
    [InlineData("it")]
    [InlineData("ru")]
    public void OnCultureChanged_WithNonExistingCultures_ShouldFallBackToKey(string culture)
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" },
            { "de", "Hallo" }
        };

        using TranslationUnit unit = new("greeting", translations);

        // Act & Assert
        Action act = () => TranslationProvider.SetCulture(new CultureInfo(culture));
        act.Should().NotThrow();
        unit.CurrentValue.Should().Be("greeting");
    }

    [Fact]
    public void GetTranslationForCulture_ShouldReturnCorrectTranslation()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello" },
            { "fr", "Bonjour" }
        };
        using TranslationUnit unit = new("greeting", translations);

        // Act
        TranslationProvider.SetCulture(new CultureInfo("fr-CA")); // French Canada should use "fr"

        // Assert
        unit.CurrentValue.Should().Be("Bonjour");
    }
}

#pragma warning restore S1481 // Unused local variables should be removed
