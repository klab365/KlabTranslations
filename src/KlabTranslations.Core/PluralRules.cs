using System.Globalization;

namespace KlabTranslations.Core;

/// <summary>Resolves cardinal plural categories for supported languages.</summary>
public static class PluralRules
{
    /// <summary>Gets the cardinal category for a number and culture.</summary>
    public static PluralCategory GetCategory(CultureInfo culture, decimal count)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));
        decimal n = Math.Abs(count);
        int i = (int)n;
        bool integer = n == i;
        return culture.TwoLetterISOLanguageName switch
        {
            "fr" => n is >= 0 and < 2 ? PluralCategory.One : PluralCategory.Other,
            "ru" when integer && i % 10 == 1 && i % 100 != 11 => PluralCategory.One,
            "ru" when integer && i % 10 is >= 2 and <= 4 && (i % 100 < 12 || i % 100 > 14) => PluralCategory.Few,
            "ru" when integer => PluralCategory.Many,
            "ru" => PluralCategory.Other,
            "ar" when n == 0 => PluralCategory.Zero,
            "ar" when n == 1 => PluralCategory.One,
            "ar" when n == 2 => PluralCategory.Two,
            "ar" when integer && i % 100 is >= 3 and <= 10 => PluralCategory.Few,
            "ar" when integer && i % 100 is >= 11 and <= 99 => PluralCategory.Many,
            _ => n == 1 ? PluralCategory.One : PluralCategory.Other
        };
    }
}
