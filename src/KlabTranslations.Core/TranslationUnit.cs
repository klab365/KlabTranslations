using System.Globalization;
using System.Reactive.Subjects;

namespace KlabTranslations.Core;

/// <summary>
/// Represents a translation unit with support for multiple cultures.
/// </summary>
public sealed record TranslationUnit : IDisposable
{
    private readonly BehaviorSubject<string> _value;

    /// <summary>
    /// Value stream that emits the current translation based on the active culture.
    /// </summary>
    public IObservable<string> Value => _value;

    /// <summary>
    /// Gets the current translation value for the active culture.
    /// </summary>
    public string CurrentValue => _value.Value;

    /// <summary>
    /// Gets the key identifying this translation unit.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the dictionary of translations mapped by culture.
    /// </summary>
    public IReadOnlyDictionary<CultureInfo, string> Translations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationUnit"/> class.
    /// </summary>
    public TranslationUnit(string key, Dictionary<CultureInfo, string> translations)
    {
        Key = key;
        Translations = translations;
        _value = new BehaviorSubject<string>(GetTranslationForCulture(TranslationProvider.CurrentCulture));
        TranslationProvider.CultureChanged += OnCultureChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationUnit"/> class from string-based culture codes.
    /// </summary>
    public TranslationUnit(string key, Dictionary<string, string> translations)
        : this(key, ConvertStringDictionaryToCultureDictionary(translations))
    {
    }

    private static Dictionary<CultureInfo, string> ConvertStringDictionaryToCultureDictionary(Dictionary<string, string> translations)
    {
        Dictionary<CultureInfo, string> result = new();
        foreach (KeyValuePair<string, string> kvp in translations)
        {
            try
            {
                CultureInfo culture = new(kvp.Key);
                result[culture] = kvp.Value;
            }
            catch (CultureNotFoundException)
            {
                // Skip invalid culture codes
                continue;
            }
        }
        return result;
    }

    private void OnCultureChanged(object? sender, CultureInfo e)
    {
        string translation = GetTranslationForCulture(e);
        _value.OnNext(translation);
    }

    private string GetTranslationForCulture(CultureInfo culture)
    {
        // Try exact match first (e.g., en-US)
        if (Translations.TryGetValue(culture, out string? translation))
        {
            return translation;
        }

        // Try parent culture match (e.g., en for en-US)
        if (!culture.IsNeutralCulture && culture.Parent != CultureInfo.InvariantCulture &&
            Translations.TryGetValue(culture.Parent, out translation))
        {
            return translation;
        }

        // Try by two-letter ISO code
        CultureInfo? matchingCulture = Translations.Keys
            .FirstOrDefault(c => c.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName);

        if (matchingCulture != null)
        {
            return Translations[matchingCulture];
        }

        throw new KeyNotFoundException($"No translation found for culture '{culture.Name}' and key '{Key}'.");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        TranslationProvider.CultureChanged -= OnCultureChanged;
        _value.Dispose();
    }
}
