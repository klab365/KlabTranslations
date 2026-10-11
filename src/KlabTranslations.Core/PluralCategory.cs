namespace KlabTranslations.Core;

/// <summary>Plural categories used by CLDR-style translation data.</summary>
public enum PluralCategory
{
    /// <summary>Zero category.</summary>
    Zero,
    /// <summary>Singular category.</summary>
    One,
    /// <summary>Dual category.</summary>
    Two,
    /// <summary>Few category.</summary>
    Few,
    /// <summary>Many category.</summary>
    Many,
    /// <summary>Fallback category.</summary>
    Other
}
