using System.Text.RegularExpressions;

namespace KlabTranslations.Core;

/// <summary>
/// Utility for resolving parameters in translation strings.
/// Supports both indexed ({0}, {1}) and named ({name}, {age}) parameters.
/// </summary>
internal static class ParameterResolver
{
    private static readonly Regex IndexedParameterRegex = new(@"\{(\d+)\}", RegexOptions.Compiled);
    private static readonly Regex NamedParameterRegex = new(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled);
    private static readonly Regex EscapedBraceRegex = new(@"\{\{|\}\}", RegexOptions.Compiled);

    /// <summary>
    /// Resolves parameters in a translation string using indexed parameters.
    /// </summary>
    /// <param name="template">The template string with {0}, {1}, etc. placeholders</param>
    /// <param name="parameters">Array of parameter values</param>
    /// <returns>The string with parameters resolved</returns>
    public static string ResolveIndexedParameters(string template, object?[] parameters)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        if (parameters == null || parameters.Length == 0)
        {
            return UnescapeBraces(template);
        }

        string result = IndexedParameterRegex.Replace(template, match =>
        {
            if (int.TryParse(match.Groups[1].Value, out int index))
            {
                if (index >= 0 && index < parameters.Length)
                {
                    return parameters[index]?.ToString() ?? string.Empty;
                }

                // Parameter index out of range - keep placeholder or throw?
                throw new ArgumentException($"Parameter index {index} is out of range. Available parameters: 0-{parameters.Length - 1}");
            }

            return match.Value; // Keep original if parsing fails
        });

        return UnescapeBraces(result);
    }

    /// <summary>
    /// Resolves parameters in a translation string using named parameters.
    /// </summary>
    /// <param name="template">The template string with {name}, {age}, etc. placeholders</param>
    /// <param name="parameters">Dictionary of parameter names and values</param>
    /// <returns>The string with parameters resolved</returns>
    public static string ResolveNamedParameters(string template, IReadOnlyDictionary<string, object?> parameters)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        if (parameters == null || parameters.Count == 0)
        {
            return UnescapeBraces(template);
        }

        string result = NamedParameterRegex.Replace(template, match =>
        {
            string paramName = match.Groups[1].Value;

            if (parameters.TryGetValue(paramName, out object? value))
            {
                return value?.ToString() ?? string.Empty;
            }

            // Parameter not found - keep placeholder or throw?
            throw new ArgumentException($"Parameter '{paramName}' not found in provided parameters. Available parameters: {string.Join(", ", parameters.Keys)}");
        });

        return UnescapeBraces(result);
    }

    /// <summary>
    /// Checks if a template string contains any parameter placeholders.
    /// </summary>
    /// <param name="template">The template string to check</param>
    /// <returns>True if the string contains parameters, false otherwise</returns>
    public static bool HasParameters(string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return false;
        }

        return IndexedParameterRegex.IsMatch(template) || NamedParameterRegex.IsMatch(template);
    }

    /// <summary>
    /// Gets all indexed parameter indices found in the template.
    /// </summary>
    /// <param name="template">The template string to analyze</param>
    /// <returns>Set of parameter indices found</returns>
    public static HashSet<int> GetIndexedParameterIndices(string template)
    {
        HashSet<int> indices = new();

        if (string.IsNullOrEmpty(template))
        {
            return indices;
        }

        foreach (Match match in IndexedParameterRegex.Matches(template))
        {
            if (int.TryParse(match.Groups[1].Value, out int index))
            {
                indices.Add(index);
            }
        }

        return indices;
    }

    /// <summary>
    /// Gets all named parameter names found in the template.
    /// </summary>
    /// <param name="template">The template string to analyze</param>
    /// <returns>Set of parameter names found</returns>
    public static HashSet<string> GetNamedParameterNames(string template)
    {
        HashSet<string> names = new();

        if (string.IsNullOrEmpty(template))
        {
            return names;
        }

        foreach (Match match in NamedParameterRegex.Matches(template))
        {
            names.Add(match.Groups[1].Value);
        }

        return names;
    }

    /// <summary>
    /// Converts escaped braces {{, }} back to single braces.
    /// </summary>
    private static string UnescapeBraces(string text)
    {
        return EscapedBraceRegex.Replace(text, match => match.Value[0].ToString());
    }
}
