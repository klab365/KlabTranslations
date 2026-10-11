using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using KlabTranslations.Core;

namespace KlabTranslations.Generator.Avalonia;

/// <summary>
/// Multi-value converter that handles translation with parameter bindings.
/// Combines parameter bindings with the translation unit's value stream.
/// </summary>
internal class ParameterizedTranslationConverter : IMultiValueConverter
{
    private readonly TranslationUnit _unit;
    private readonly int[] _parameterIndices;

    /// <summary>
    /// Create a converter for parameterized translations
    /// </summary>
    public ParameterizedTranslationConverter(TranslationUnit unit, int[] parameterIndices)
    {
        _unit = unit;
        _parameterIndices = parameterIndices;
    }

    /// <inheritdoc/>
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // Last value is the translation unit's current value from the observable
        if (values.Count == 0)
        {
            return _unit.CurrentValue;
        }

        // All values except the last are parameters
        int paramCount = values.Count - 1;
        for (int i = 0; i < paramCount; i++)
        {
            int paramIndex = _parameterIndices[i];
            if (values[i] != null && values[i] is not BindingNotification)
            {
                _unit.Parameters[paramIndex] = values[i];
                if (paramIndex == 0 && _unit.PluralTranslations.Count > 0)
                    _unit.Parameters["count"] = values[i];
            }
        }

        // Setting parameters synchronously updates the unit. The observable value in this
        // MultiBinding may still be stale during a re-entrant converter call.
        return _unit.CurrentValue;
    }
}
