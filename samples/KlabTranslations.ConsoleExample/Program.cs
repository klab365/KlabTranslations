// See https://aka.ms/new-console-template for more information
using KlabTranslations.ConsoleExample;
using KlabTranslations.ConsoleExample.Translations;
using KlabTranslations.Core;

Console.WriteLine("=== Testing Translation Generator ===");
Console.WriteLine($"Class {typeof(Strings).FullName} generated successfully.");
Console.WriteLine();

Console.WriteLine("Accessing some translation units:");
Console.WriteLine($"Key of HelloWorld: {Strings.hello_world}");
Console.WriteLine($"Current value of HelloWorld: {Strings.hello_world.CurrentValue}");

Console.WriteLine("Changing culture to 'de' (German)...");
TranslationProvider.SetCulture(new System.Globalization.CultureInfo("de"));
Console.WriteLine($"Current value of HelloWorld: {Strings.hello_world.CurrentValue}");
