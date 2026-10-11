using System.Globalization;
using System.Text;

namespace KlabTranslations.Cli;

/// <summary>Validates translation CSV files and reports language coverage.</summary>
public static class TranslationChecker
{
    /// <summary>Runs the CLI command and returns its process exit code.</summary>
    public static int Run(string[] args)
    {
        if (args.Length != 1 || args[0] is "--help" or "-h")
        {
            Console.WriteLine("Usage: klab-translations <translations.csv>");
            Console.WriteLine("Validates a translation CSV and reports coverage per language.");
            return args.Length == 1 ? 0 : 2;
        }

        string path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File not found: {path}");
            return 2;
        }

        try
        {
            List<string[]> rows = ReadCsv(path);
            if (rows.Count < 2)
                throw new FormatException("CSV must have a header and at least one data row.");

            string[] headers = rows[0];
            if (headers.Length < 2 || headers[0] != "Key" || headers.Skip(1).Any(string.IsNullOrWhiteSpace))
                throw new FormatException("Header must start with 'Key' and contain at least one language column.");
            if (headers.Distinct(StringComparer.Ordinal).Count() != headers.Length)
                throw new FormatException("Duplicate column name.");

            HashSet<string> keys = new(StringComparer.Ordinal);
            int[] populated = new int[headers.Length - 1];
            List<string> errors = [];
            for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                string[] row = rows[rowIndex];
                int line = rowIndex + 1;
                if (row.Length != headers.Length)
                {
                    errors.Add($"Line {line}: expected {headers.Length} columns but found {row.Length}.");
                    continue;
                }
                string key = row[0].Trim();
                if (key.Length == 0)
                    errors.Add($"Line {line}: translation key cannot be empty.");
                else if (!keys.Add(key))
                    errors.Add($"Line {line}: duplicate translation key '{key}'.");

                for (int column = 1; column < row.Length; column++)
                    if (!string.IsNullOrWhiteSpace(row[column])) populated[column - 1]++;
            }

            if (keys.Count == 0)
                errors.Add("No valid translation keys found.");
            foreach (string error in errors)
                Console.Error.WriteLine(error);

            Console.WriteLine($"Translation coverage ({keys.Count} keys):");
            for (int i = 1; i < headers.Length; i++)
            {
                int missing = keys.Count - populated[i - 1];
                double percent = keys.Count == 0 ? 0 : populated[i - 1] * 100d / keys.Count;
                Console.WriteLine($"  {headers[i]}: {populated[i - 1]}/{keys.Count} ({percent.ToString("0.##", CultureInfo.InvariantCulture)}%) — {missing} missing");
                if (missing > 0)
                {
                    for (int rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                        if (rows[rowIndex].Length == headers.Length && string.IsNullOrWhiteSpace(rows[rowIndex][i]))
                            Console.WriteLine($"    Missing: {rows[rowIndex][0]}");
                }
            }

            return errors.Count == 0 ? 0 : 1;
        }
        catch (Exception exception) when (exception is FormatException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Invalid CSV: {exception.Message}");
            return 1;
        }
    }

    private static List<string[]> ReadCsv(string path)
    {
        List<string[]> records = [];
        StringBuilder field = new();
        List<string> record = [];
        bool quoted = false;
        string text = File.ReadAllText(path);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool escapedQuote = quoted && c == '"' && i + 1 < text.Length && text[i + 1] == '"';
            if (escapedQuote)
            {
                field.Append('"');
                text = text.Remove(i + 1, 1);
            }
            else if (quoted && c == '"')
                quoted = false;
            else if (!quoted && c == '"')
                quoted = true;
            else if (c == ',' && !quoted)
            {
                record.Add(field.ToString().Trim());
                field.Clear();
            }
            else if ((c == '\n' || c == '\r') && !quoted)
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    continue;
                record.Add(field.ToString().Trim());
                field.Clear();
                if (record.Any(value => value.Length > 0)) records.Add(record.ToArray());
                record.Clear();
            }
            else
                field.Append(c);
        }
        if (quoted) throw new FormatException("Unclosed quoted field.");
        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString().Trim());
            records.Add(record.ToArray());
        }
        return records;
    }
}
