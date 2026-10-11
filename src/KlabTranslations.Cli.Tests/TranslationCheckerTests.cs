using KlabTranslations.Cli;
using Xunit;

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

    [Theory]
    [InlineData("Key,en-US\nhello,Hello\nhello,Again\n", "duplicate translation key 'hello'")]
    [InlineData("Key,en-US\n,Hello\n", "key cannot be empty")]
    [InlineData("Key,en-US,en-US\nhello,Hello,Hallo\n", "Duplicate column name")]
    [InlineData("Wrong,en-US\nhello,Hello\n", "Header must start with 'Key'")]
    public void Run_InvalidCsv_ReturnsFailureAndExplainsError(string csv, string expectedError)
    {
        string path = CreateCsv(csv);
        TextWriter original = Console.Error;
        using StringWriter error = new();
        try
        {
            Console.SetError(error);
            int result = TranslationChecker.Run([path]);

            Assert.Equal(1, result);
            Assert.Contains(expectedError, error.ToString());
        }
        finally
        {
            Console.SetError(original);
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("Key,en-US\n")]
    [InlineData("Key,en-US\nhello,\"unfinished\n")]
    public void Run_IncompleteCsv_ReturnsFailure(string csv)
    {
        string path = CreateCsv(csv);
        TextWriter original = Console.Error;
        using StringWriter error = new();
        try
        {
            Console.SetError(error);
            Assert.Equal(1, TranslationChecker.Run([path]));
            Assert.Contains("Invalid CSV", error.ToString());
        }
        finally
        {
            Console.SetError(original);
            File.Delete(path);
        }
    }

    [Fact]
    public void Run_NoArguments_ReturnsUsageFailure()
    {
        Assert.Equal(2, TranslationChecker.Run([]));
    }

    [Fact]
    public void Run_HelpArgument_PrintsUsageAndSucceeds()
    {
        TextWriter original = Console.Out;
        using StringWriter output = new();
        try
        {
            Console.SetOut(output);
            Assert.Equal(0, TranslationChecker.Run(["--help"]));
            Assert.Contains("Usage: klab-translations", output.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [Fact]
    public void Run_QuotedCommasAndEscapedQuotes_ParsesCorrectly()
    {
        string path = CreateCsv("Key,en-US\nhello,\"Hello, \\\"world\\\"\"\n");
        TextWriter original = Console.Out;
        using StringWriter output = new();
        try
        {
            Console.SetOut(output);
            Assert.Equal(0, TranslationChecker.Run([path]));
            Assert.Contains("en-US: 1/1 (100%)", output.ToString());
        }
        finally
        {
            Console.SetOut(original);
            File.Delete(path);
        }
    }

    private static string CreateCsv(string contents)
    {
        string path = Path.Combine(Path.GetTempPath(), $"translations-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, contents);
        return path;
    }
}
