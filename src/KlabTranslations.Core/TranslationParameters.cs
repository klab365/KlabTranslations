using System.Reactive.Subjects;

namespace KlabTranslations.Core;

/// <summary>
/// A parameter collection that supports both indexed and named parameters.
/// Notifies when parameters change to trigger translation updates.
/// </summary>
public sealed class TranslationParameters : IDisposable
{
    private readonly Dictionary<int, object?> _indexedParameters = new();
    private readonly Dictionary<string, object?> _namedParameters = new();
    private readonly Subject<Unit> _parametersChanged = new();

    /// <summary>
    /// Event that fires when parameters change.
    /// </summary>
    internal IObservable<Unit> ParametersChanged => _parametersChanged;

    /// <summary>
    /// Gets or sets an indexed parameter value.
    /// </summary>
    /// <param name="index">The parameter index (0, 1, 2, etc.)</param>
    public object? this[int index]
    {
        get => _indexedParameters.TryGetValue(index, out object? value) ? value : null;
        set
        {
            bool changed = !_indexedParameters.TryGetValue(index, out object? current) ||
                          !Equals(current, value);

            if (changed)
            {
                if (value == null)
                {
                    _indexedParameters.Remove(index);
                }
                else
                {
                    _indexedParameters[index] = value;
                }

                NotifyParametersChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a named parameter value.
    /// </summary>
    /// <param name="name">The parameter name</param>
    public object? this[string name]
    {
        get => _namedParameters.TryGetValue(name, out object? value) ? value : null;
        set
        {
            bool changed = !_namedParameters.TryGetValue(name, out object? current) ||
                          !Equals(current, value);

            if (changed)
            {
                if (value == null)
                {
                    _namedParameters.Remove(name);
                }
                else
                {
                    _namedParameters[name] = value;
                }

                NotifyParametersChanged();
            }
        }
    }

    /// <summary>
    /// Gets the count of indexed parameters.
    /// </summary>
    public int IndexedParameterCount => _indexedParameters.Count;

    /// <summary>
    /// Gets the count of named parameters.
    /// </summary>
    public int NamedParameterCount => _namedParameters.Count;

    /// <summary>
    /// Checks if there are any parameters set.
    /// </summary>
    public bool HasParameters => _indexedParameters.Count > 0 || _namedParameters.Count > 0;

    /// <summary>
    /// Sets multiple indexed parameters at once.
    /// </summary>
    /// <param name="parameters">Array of parameter values</param>
    public void SetIndexedParameters(params object?[] parameters)
    {
        _indexedParameters.Clear();

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] != null)
            {
                _indexedParameters[i] = parameters[i];
            }
        }

        NotifyParametersChanged();
    }

    /// <summary>
    /// Sets multiple named parameters at once.
    /// </summary>
    /// <param name="parameters">Dictionary of parameter names and values</param>
    public void SetNamedParameters(IEnumerable<KeyValuePair<string, object?>> parameters)
    {
        _namedParameters.Clear();

        foreach (KeyValuePair<string, object?> kvp in parameters)
        {
            if (kvp.Value != null)
            {
                _namedParameters[kvp.Key] = kvp.Value;
            }
        }

        NotifyParametersChanged();
    }

    /// <summary>
    /// Clears all parameters.
    /// </summary>
    public void Clear()
    {
        bool hadParameters = HasParameters;

        _indexedParameters.Clear();
        _namedParameters.Clear();

        if (hadParameters)
        {
            NotifyParametersChanged();
        }
    }

    /// <summary>
    /// Gets indexed parameters as an array for the parameter resolver.
    /// </summary>
    internal object?[] GetIndexedParametersArray()
    {
        if (_indexedParameters.Count == 0)
        {
            return Array.Empty<object?>();
        }

        int maxIndex = _indexedParameters.Keys.Max();
        object?[] array = new object?[maxIndex + 1];

        foreach (KeyValuePair<int, object?> kvp in _indexedParameters)
        {
            array[kvp.Key] = kvp.Value;
        }

        return array;
    }

    /// <summary>
    /// Gets named parameters as a dictionary for the parameter resolver.
    /// </summary>
    internal IReadOnlyDictionary<string, object?> GetNamedParametersDictionary()
    {
        return _namedParameters;
    }

    private void NotifyParametersChanged()
    {
        _parametersChanged.OnNext(Unit.Default);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _parametersChanged.Dispose();
    }
}

/// <summary>
/// Unit type for parameter change notifications.
/// </summary>
internal readonly struct Unit
{
    public static readonly Unit Default = new();
}
