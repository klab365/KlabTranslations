namespace KlabTranslations.Core.Tests;

/// <summary>
/// Test collection to ensure tests that modify TranslationProvider run sequentially
/// </summary>
[CollectionDefinition("TranslationProvider")]
public class TranslationProviderCollection : ICollectionFixture<TranslationProviderFixture>
{
}

/// <summary>
/// Fixture to ensure proper test isolation for TranslationProvider tests
/// </summary>
public sealed class TranslationProviderFixture : IDisposable
{
    public TranslationProviderFixture()
    {
        // Reset to a known state
        TranslationProvider.SetCulture(new System.Globalization.CultureInfo("en-US"));
    }

    public void Dispose()
    {
        // Reset back to default after tests
        TranslationProvider.SetCulture(new System.Globalization.CultureInfo("en-US"));
    }
}
