using Avalonia;
using Avalonia.Data;
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

    /// <summary>
    /// Parameter 0 for parameterized translations (supports static values and bindings)
    /// </summary>
    public object? Param0 { get; set; }

    /// <summary>
    /// Parameter 1 for parameterized translations (supports static values and bindings)
    /// </summary>
    public object? Param1 { get; set; }

    /// <summary>
    /// Parameter 2 for parameterized translations (supports static values and bindings)
    /// </summary>
    public object? Param2 { get; set; }

    /// <summary>
    /// Parameter 3 for parameterized translations (supports static values and bindings)
    /// </summary>
    public object? Param3 { get; set; }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // Collect parameters with their indices
        object?[] parameters = new[] { Param0, Param1, Param2, Param3 };

        // Check if any parameters are bindings or have values
        List<int> activeParamIndices = new();
        List<object?> activeParams = new();

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] != null)
            {
                activeParamIndices.Add(i);
                activeParams.Add(parameters[i]);
            }
        }

        // Check if any active parameters are bindings
        bool hasBindings = activeParams.Any(p => p is IBinding);

        if (hasBindings && activeParams.Count > 0)
        {
            // Use MultiBinding for dynamic parameter support
            MultiBinding multiBinding = new()
            {
                Converter = new ParameterizedTranslationConverter(_unit, activeParamIndices.ToArray()),
                Mode = BindingMode.OneWay
            };

            // Add each active parameter as a binding or convert static values to bindings
            foreach (object? param in activeParams)
            {
                if (param is IBinding binding)
                {
                    multiBinding.Bindings.Add(binding);
                }
                else if (param != null)
                {
                    // Create a binding that returns a static value
                    Binding staticBinding = new() { Source = param };
                    multiBinding.Bindings.Add(staticBinding);
                }
            }

            // Add the translation unit's value observable as the last binding
            // This ensures the converter is called whenever the translation changes (e.g., on culture change)
            IBinding translationBinding = _unit.Value.ToBinding();
            multiBinding.Bindings.Add(translationBinding);

            return multiBinding;
        }
        else
        {
            // Use simple binding for static parameters (backward compatible)
            if (Param0 != null)
            {
                _unit.Parameters[0] = Param0;
            }
            if (Param1 != null)
            {
                _unit.Parameters[1] = Param1;
            }
            if (Param2 != null)
            {
                _unit.Parameters[2] = Param2;
            }
            if (Param3 != null)
            {
                _unit.Parameters[3] = Param3;
            }

            return _unit.Value.ToBinding();
        }
    }
}
