using KlabTranslations.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace KlabTranslations.Generator.Tests;

public class GeneratorTests
{
    [Theory]
    [InlineData("Key,de,en\nHello,Hallo,Hello", false)]
    [InlineData("Key,de,en\nHello,Hallo", true)]
    [InlineData("Key,de,en\nHello,Hallo,Hello\nHello,Noch einmal,Again", true)]
    [InlineData("Key,de,en\nHello,{name},Hello {other}", true)]
    [InlineData("Key,de,en\nHello,{name},Hallo {name}", false)]
    public void Reports_csv_errors_as_diagnostics(string csv, bool expectedError)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new Generator().AsSourceGenerator()],
            additionalTexts: [new CsvAdditionalText(csv)]);

        driver = driver.RunGenerators(CSharpCompilation.Create("Test"), TestContext.Current.CancellationToken);

        Assert.Equal(expectedError, driver.GetRunResult().Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    private sealed class CsvAdditionalText(string content) : AdditionalText
    {
        public override string Path => "Translations.csv";

        public override SourceText GetText(System.Threading.CancellationToken cancellationToken = default)
            => SourceText.From(content);
    }
}
