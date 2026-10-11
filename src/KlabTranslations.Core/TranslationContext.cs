using System.Globalization;

namespace KlabTranslations.Core;

/// <summary>Manages a culture independently from other translation contexts.</summary>
public sealed class TranslationContext
{
    /// <summary>Raised when this context's culture changes.</summary>
    public event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>The active culture for this context.</summary>
    public CultureInfo CurrentCulture { get; private set; }

    /// <summary>Creates an independent translation context.</summary>
    public TranslationContext(CultureInfo? initialCulture = null)
    {
        CurrentCulture = initialCulture ?? CultureInfo.CurrentUICulture;
    }

    /// <summary>Changes this context's culture and notifies listeners.</summary>
    public void SetCulture(CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));
        if (CurrentCulture.Name == culture.Name)
            return;

        CurrentCulture = culture;
        CultureChanged?.Invoke(this, culture);
    }
}
