using Avalonia;
using Avalonia.Markup.Xaml;
using KlabTranslations.Core;

namespace KlabTranslations.Generator.Avalonia;

/// <summary>
/// Markup for Avalonia to interact
/// with the translation unit.
/// </summary>
public sealed class Translate : MarkupExtension
{
    private readonly TranslationUnit _unit;

    /// <summary>
    /// Create a xaml markup extension
    /// </summary>
    public Translate(TranslationUnit unit)
    {
        _unit = unit;
    }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _unit.Value.ToBinding();
    }
}
