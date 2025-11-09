using System.Globalization;
using AwesomeAssertions;
using KlabTranslations.Core;

namespace KlabTranslations.Core.Tests;

[Collection("TranslationProvider")]
public sealed class ParameterizedTranslationTests : IDisposable
{
    private readonly CultureInfo _originalCulture;

    public ParameterizedTranslationTests()
    {
        // Store original culture and set to English for consistent tests
        _originalCulture = TranslationProvider.CurrentCulture;
        TranslationProvider.SetCulture(new CultureInfo("en-US"));
    }

    public void Dispose()
    {
        // Restore original culture
        TranslationProvider.SetCulture(_originalCulture);
    }

    private static TranslationUnit CreateTranslationUnit(string key, Dictionary<string, string> translations)
    {
        TranslationUnit unit = new(key, translations);
        return unit;
    }

    [Fact]
    public void IndexedParameters_ShouldResolveCorrectly()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello {0}, you have {1} messages" },
            { "de", "Hallo {0}, Sie haben {1} Nachrichten" }
        };

        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);

        // Act
        unit.Parameters[0] = "John";
        unit.Parameters[1] = 5;

        // Assert
        unit.CurrentValue.Should().Be("Hello John, you have 5 messages");
    }

    [Fact]
    public void NamedParameters_ShouldResolveCorrectly()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Welcome {name}, you have {count} items" },
            { "de", "Willkommen {name}, Sie haben {count} Artikel" }
        };

        using TranslationUnit unit = CreateTranslationUnit("welcome", translations);

        // Act
        unit.Parameters["name"] = "Alice";
        unit.Parameters["count"] = 3;

        // Assert
        unit.CurrentValue.Should().Be("Welcome Alice, you have 3 items");
    }

    [Fact]
    public void ParametersWithCultureChange_ShouldUpdateCorrectly()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello {0}" },
            { "de", "Hallo {0}" }
        };

        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);
        unit.Parameters[0] = "World";

        // Act - Change culture
        TranslationProvider.SetCulture(new CultureInfo("de"));

        // Assert
        unit.CurrentValue.Should().Be("Hallo World");
    }

    [Fact]
    public void NoParameters_ShouldReturnBaseTranslation()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello {0}" },
            { "de", "Hallo {0}" }
        };

        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);

        // Act & Assert - Without setting parameters
        unit.CurrentValue.Should().Be("Hello {0}");
    }

    [Fact]
    public void MissingParameters_ShouldReturnBaseTranslation()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello {0}, you have {1} messages" },
        };

        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);

        // Act - Only set first parameter
        unit.Parameters[0] = "John";

        // Assert - Should return base translation when not all parameters are available
        unit.CurrentValue.Should().Be("Hello {0}, you have {1} messages");
    }

    [Fact]
    public void ClearParameters_ShouldReturnBaseTranslation()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Hello {0}" },
        };

        using TranslationUnit unit = CreateTranslationUnit("greeting", translations);
        unit.Parameters[0] = "World";

        // Act
        unit.Parameters.Clear();

        // Assert
        unit.CurrentValue.Should().Be("Hello {0}");
    }

    [Fact]
    public void ValueObservable_ShouldEmitWhenParametersChange()
    {
        // Arrange
        Dictionary<string, string> translations = new()
        {
            { "en", "Count: {0}" },
        };

        using TranslationUnit unit = CreateTranslationUnit("counter", translations);

        List<string> receivedValues = new();
        using IDisposable subscription = unit.Value.Subscribe(receivedValues.Add);

        // Act
        unit.Parameters[0] = 1;
        unit.Parameters[0] = 2;
        unit.Parameters[0] = 3;

        // Assert
        receivedValues.Count.Should().BeGreaterThanOrEqualTo(4); // Initial + 3 updates (might have duplicates)
        receivedValues[^1].Should().Be("Count: 3");
    }
}
