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
    private readonly TranslationContext? _context;

    /// <summary>
    /// Value stream that emits the current translation based on the active culture and parameters.
    /// </summary>
    public IObservable<string> Value => _value;

    /// <summary>
    /// Gets the current translation value for the active culture with parameters resolved.
    /// </summary>
    public string CurrentValue => _value.Value;

    /// <summary>Gets this translation resolved for an explicitly supplied culture.</summary>
    public string GetValue(CultureInfo culture) => GetResolvedTranslation(culture);

    /// <summary>Creates an independent translation unit bound to the supplied culture context.</summary>
    public TranslationUnit CreateForContext(TranslationContext context) => new(
        Key,
        Translations.ToDictionary(entry => entry.Key, entry => entry.Value),
        PluralTranslations.ToDictionary(entry => entry.Key, entry => new Dictionary<PluralCategory, string>(entry.Value)),
        context);

    /// <summary>
    /// Gets the key identifying this translation unit.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the dictionary of translations mapped by culture.
    /// </summary>
    public IReadOnlyDictionary<CultureInfo, string> Translations { get; }

    /// <summary>Plural translations indexed by culture and plural category.</summary>
    public IReadOnlyDictionary<CultureInfo, Dictionary<PluralCategory, string>> PluralTranslations { get; }

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
        : this(key, translations, new Dictionary<CultureInfo, Dictionary<PluralCategory, string>>())
    {
    }

    /// <summary>Initializes a translation unit with culture-specific plural forms.</summary>
    public TranslationUnit(string key, Dictionary<CultureInfo, string> translations, Dictionary<CultureInfo, Dictionary<PluralCategory, string>> pluralTranslations)
        : this(key, translations, pluralTranslations, null, false)
    {
    }

    /// <summary>Initializes a translation unit that follows an independent culture context.</summary>
    public TranslationUnit(string key, Dictionary<CultureInfo, string> translations, TranslationContext context)
        : this(key, translations, new Dictionary<CultureInfo, Dictionary<PluralCategory, string>>(), context, true)
    {
    }

    /// <summary>Initializes a translation unit from language codes and an independent culture context.</summary>
    public TranslationUnit(string key, Dictionary<string, string> translations, TranslationContext context)
        : this(key, ConvertStringDictionaryToCultureDictionary(translations), new Dictionary<CultureInfo, Dictionary<PluralCategory, string>>(), context, true)
    {
    }

    /// <summary>Initializes a translation unit with plural forms and an independent culture context.</summary>
    public TranslationUnit(string key, Dictionary<CultureInfo, string> translations, Dictionary<CultureInfo, Dictionary<PluralCategory, string>> pluralTranslations, TranslationContext context)
        : this(key, translations, pluralTranslations, context, true)
    {
    }

    private TranslationUnit(string key, Dictionary<CultureInfo, string> translations, Dictionary<CultureInfo, Dictionary<PluralCategory, string>> pluralTranslations, TranslationContext? context, bool useContext)
    {
        Key = key;
        Translations = translations;
        PluralTranslations = pluralTranslations;
        _context = context;
        _parameters = new TranslationParameters();
        CultureInfo initialCulture = useContext ? context!.CurrentCulture : TranslationProvider.CurrentCulture;
        _value = new BehaviorSubject<string>(GetResolvedTranslation(initialCulture));

        if (useContext)
            context!.CultureChanged += OnCultureChanged;
        else
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

    /// <summary>Initializes a translation unit from language-code translations and plural forms.</summary>
    public TranslationUnit(string key, Dictionary<string, string> translations, Dictionary<CultureInfo, Dictionary<PluralCategory, string>> pluralTranslations)
        : this(key, ConvertStringDictionaryToCultureDictionary(translations), pluralTranslations)
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
        string translation = GetResolvedTranslation(_context == null ? TranslationProvider.CurrentCulture : _context.CurrentCulture);
        _value.OnNext(translation);
    }

    private string GetResolvedTranslation(CultureInfo culture)
    {
        string baseTranslation = GetTranslationForCulture(culture);
        if (_parameters.NamedParameterCount > 0 && _parameters.GetNamedParametersDictionary().TryGetValue("count", out object? countValue) &&
            countValue != null && TryConvertCount(countValue, out decimal count))
        {
            Dictionary<PluralCategory, string>? forms = GetPluralForms(culture);
            if (forms != null && (forms.TryGetValue(PluralRules.GetCategory(culture, count), out string? plural) || forms.TryGetValue(PluralCategory.Other, out plural)))
                baseTranslation = plural;
        }

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
                if (namedParamNames.Count > 0 && namedParamNames.All(name => namedParams.ContainsKey(name)))
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

    private static bool TryConvertCount(object value, out decimal count)
    {
        try
        { count = Convert.ToDecimal(value, CultureInfo.InvariantCulture); return true; }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException) { count = 0; return false; }
    }

    private Dictionary<PluralCategory, string>? GetPluralForms(CultureInfo culture)
    {
        if (PluralTranslations.TryGetValue(culture, out Dictionary<PluralCategory, string>? forms))
            return forms;
        if (!culture.IsNeutralCulture && PluralTranslations.TryGetValue(culture.Parent, out forms))
            return forms;
        return PluralTranslations.FirstOrDefault(x => x.Key.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName).Value;
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

        // Fall back to the invariant-culture translation when available, then to the key itself.
        if (Translations.TryGetValue(CultureInfo.InvariantCulture, out translation))
        {
            return translation;
        }

        return Key;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_context == null)
            TranslationProvider.CultureChanged -= OnCultureChanged;
        else
            _context.CultureChanged -= OnCultureChanged;
        _parameters.Dispose();
        _value.Dispose();
    }
}
