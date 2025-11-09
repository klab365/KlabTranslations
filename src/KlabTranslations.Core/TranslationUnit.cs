using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace KlabTranslations.Core;

/// <summary>
/// Represents a translation unit with support for multiple cultures.
/// </summary>
public sealed record TranslationUnit : IDisposable
{
    private readonly BehaviorSubject<string> _value;
    private readonly TranslationParameters _parameters;

    /// <summary>
    /// Value stream that emits the current translation based on the active culture and parameters.
    /// </summary>
    public IObservable<string> Value => _value;

    /// <summary>
    /// Gets the current translation value for the active culture with parameters resolved.
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
    /// Gets the parameters collection for this translation unit.
    /// Use this to set indexed parameters like: unit.Parameters[0] = "value"
    /// Or named parameters like: unit.Parameters["name"] = "value"
    /// </summary>
    public TranslationParameters Parameters => _parameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationUnit"/> class.
    /// </summary>
    public TranslationUnit(string key, Dictionary<CultureInfo, string> translations)
    {
        Key = key;
        Translations = translations;
        _parameters = new TranslationParameters();
        _value = new BehaviorSubject<string>(GetResolvedTranslation(TranslationProvider.CurrentCulture));

        // Subscribe to culture changes
        TranslationProvider.CultureChanged += OnCultureChanged;

        // Subscribe to parameter changes
        _parameters.ParametersChanged.Subscribe(_ => OnParametersChanged());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslationUnit"/> class from string-based culture codes.
    /// </summary>
    public TranslationUnit(
        string key,
        Dictionary<string, string> translations)
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
        string translation = GetResolvedTranslation(e);
        _value.OnNext(translation);
    }

    private void OnParametersChanged()
    {
        string translation = GetResolvedTranslation(TranslationProvider.CurrentCulture);
        _value.OnNext(translation);
    }

    private string GetResolvedTranslation(CultureInfo culture)
    {
        string baseTranslation = GetTranslationForCulture(culture);

        // If no parameters are set, return the base translation
        if (!_parameters.HasParameters)
        {
            return baseTranslation;
        }

        // If the translation doesn't have parameter placeholders, return as-is
        if (!ParameterResolver.HasParameters(baseTranslation))
        {
            return baseTranslation;
        }

        try
        {
            // Try named parameters first
            if (_parameters.NamedParameterCount > 0)
            {
                IReadOnlyDictionary<string, object?> namedParams = _parameters.GetNamedParametersDictionary();
                HashSet<string> namedParamNames = ParameterResolver.GetNamedParameterNames(baseTranslation);

                // Check if all required named parameters are available
                if (namedParamNames.All(name => namedParams.ContainsKey(name)))
                {
                    return ParameterResolver.ResolveNamedParameters(baseTranslation, namedParams);
                }
            }

            // Try indexed parameters
            if (_parameters.IndexedParameterCount > 0)
            {
                object?[] indexedParams = _parameters.GetIndexedParametersArray();
                HashSet<int> requiredIndices = ParameterResolver.GetIndexedParameterIndices(baseTranslation);

                // Check if all required indexed parameters are available
                if (requiredIndices.All(index => index < indexedParams.Length && indexedParams[index] != null))
                {
                    return ParameterResolver.ResolveIndexedParameters(baseTranslation, indexedParams);
                }
            }

            // If we can't resolve parameters, return the base translation
            return baseTranslation;
        }
        catch (ArgumentException)
        {
            // If parameter resolution fails, return the base translation
            return baseTranslation;
        }
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
        _parameters.Dispose();
        _value.Dispose();
    }
}
