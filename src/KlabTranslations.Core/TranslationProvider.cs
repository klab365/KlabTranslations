using System.Globalization;

namespace KlabTranslations.Core;

/// <summary>
/// Provides culture management for translations.
/// </summary>
public static class TranslationProvider
{
    /// <summary>
    /// Occurs when the current culture is changed.
    /// </summary>
    public static event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>
    /// Gets the current culture used for translations.
    /// </summary>
    public static CultureInfo CurrentCulture { get; private set; }

    static TranslationProvider()
    {
        CurrentCulture = CultureInfo.CurrentUICulture;
    }

    /// <summary>
    /// Sets the current culture and raises the CultureChanged event.
    /// </summary>
    public static void SetCulture(CultureInfo culture)
    {
        if (CurrentCulture.Name == culture.Name)
        {
            return;
        }

        CurrentCulture = culture;
        CultureChanged?.Invoke(null, CurrentCulture);
    }

    /// <summary>
    /// Resets the TranslationProvider to its initial state. Used for testing.
    /// </summary>
    internal static void Reset()
    {
        CultureChanged = null;
        CurrentCulture = CultureInfo.CurrentUICulture;
    }
}
