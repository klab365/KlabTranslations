using Xunit;

using KlabTranslations.Cli;

namespace KlabTranslations.Cli.Tests;

public class TranslationCheckerTests
{
    [Fact]
    public void Run_ValidCsvWithMissingTranslation_ReportsCoverageAndSucceeds()
    {
        string path = CreateCsv("Key,en-US,de-DE\nhello,Hello,Hallo\nbye,Goodbye,\n");
        TextWriter original = Console.Out;
        using StringWriter output = new();
        try
        {
            Console.SetOut(output);
            int result = TranslationChecker.Run([path]);
            Assert.Equal(0, result);
            Assert.Contains("en-US: 2/2 (100%)", output.ToString());
            Assert.Contains("de-DE: 1/2 (50%)", output.ToString());
            Assert.Contains("Missing: bye", output.ToString());
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Run_InvalidColumnCount_ReturnsFailureAndExplainsError()
    {
        string path = CreateCsv("Key,en-US\nhello,Hello,extra\n");
        TextWriter original = Console.Error;
        using StringWriter error = new();
        try
        {
            Console.SetError(error);
            int result = TranslationChecker.Run([path]);
            Assert.Equal(1, result);
            Assert.Contains("expected 2 columns but found 3", error.ToString());
        }
        finally
        {
            Console.SetError(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Run_MissingFile_ReturnsUsageFailure()
    {
        int result = TranslationChecker.Run([Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))]);

        Assert.Equal(2, result);
    }

    private static string CreateCsv(string contents)
    {
        string path = Path.Combine(Path.GetTempPath(), $"translations-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, contents);
        return path;
    }
}
