# KlabTranslations

[![CI](https://github.com/klab365/KlabTranslations/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/klab365/KlabTranslations/actions/workflows/ci.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=klab365_KlabTranslations&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=klab365_KlabTranslations)

A modern, developer-friendly translation framework for .NET applications with support for multiple UI frameworks and strong compile-time type safety.

## 🎯 Overview

KlabTranslations is a comprehensive translation framework designed for .NET applications. It provides:

- **CSV-based translation management** - Simple, spreadsheet-friendly format
- **Source code generation** - Compile-time type safety for translation keys
- **Multi-framework support** - Console, ASP.NET, WPF, Avalonia, and more
- **Parameter support** - Handle dynamic values in translations
- **Framework adapters** - Native integration with popular UI frameworks

## 🚀 Quick Start

### 1. Install Package

The `KlabTranslations.Generator` package can not work alone, it requires at least one adapter package to function.
Edit your `.csproj` file to add the package references:

```xml
<ItemGroup>
  <PackageReference Include="KlabTranslations.Generator" Version="*" />
</ItemGroup>

<!-- Include CSV file as AdditionalFiles so the generator can read it -->
<ItemGroup>
  <AdditionalFiles Include="Translations.csv" />
</ItemGroup>
```

For Avalonia projects, also add:

```xml
<ItemGroup>
  <PackageReference Include="KlabTranslations.Generator.Avalonia" Version="*" />
</ItemGroup>
```

### 2. Create Translations.csv

Create a `Translations.csv` file in the project root with the following format:

- Header row: 

```csv
Key,de-DE,en-US
hello_world,Hallo Welt,Hello World
goodbye,Auf Wiedersehen,Goodbye
welcome,Willkommen,Welcome
thank_you,Danke,Thank you
welcome_user,Willkommen {0}!,Welcome {0}!
user_info,{0} ist {1} Jahre alt,{0} is {1} years old
message_count,Sie haben {0} Nachrichten,You have {0} messages
```

### 3. Use Generated Translations

The generated class `Strings` can be found in your `namespace.Translations`:

```csharp
using YourNamespace.Translations;

// Type-safe translation access
string welcome = Strings.welcome.ToString();

// With parameters
string greeting = Strings.welcome_user.Resolve(new[] { userName });
string info = Strings.user_info.Resolve(new[] { userName, age });

// Access by culture
string germanWelcome = Strings.welcome.ToString("de-DE");
```

## 📦 Packages

### Available Packages

| Package | Purpose | NuGet |
|---------|---------|-------|
| **KlabTranslations.Generator** | C# source generator for CSV-based translations | [NuGet](https://www.nuget.org/packages/KlabTranslations.Generator/) |
| **KlabTranslations.Generator.Avalonia** | XAML markup extension for Avalonia UI applications | [NuGet](https://www.nuget.org/packages/KlabTranslations.Generator.Avalonia/) |

## 💡 Why CSV?

We chose CSV as our translation format because:

- **Simplicity**: Easy to read and write with minimal learning curve
- **Accessibility**: Open in Microsoft Excel, Google Sheets, or any text editor
- **Translator-Friendly**: Non-technical translators can work in familiar spreadsheet tools
- **Version Control**: Clean diff views when using Git
- **Tool Support**: Universal support across all platforms and operating systems

## 🎨 Framework Integration

### Avalonia UI

Use the `Translate` markup extension directly in XAML:

```axaml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:klab="using:KlabTranslations.Generator.Avalonia"
        xmlns:translations="using:KlabTranslations.AvaloniaExample.Translations">
  <TextBlock Text="{klab:Translate {x:Static translations:Strings.welcome}}" />
  <TextBlock Text="{klab:Translate {x:Static translations:Strings.welcome_user}, Param0='Test123'}" />
  <TextBlock Text="{klab:Translate {x:Static translations:Strings.welcome_user}, Param0={Binding UserName}}" />
</Window>
```

In the example is shown how to use the `Translate` markup extension to bind translated strings directly in XAML, including parameterized translations.
The parameters can be bound to view model properties or static values

## 📋 Examples

Complete working examples are available in the `samples/` directory:

- [Console Example](samples/KlabTranslations.ConsoleExample/) - Basic console application
- [Avalonia Example](samples/KlabTranslations.AvaloniaExample/) - Full Avalonia UI application

## 🛠️ Development

Every command is available in the `Justfile` and can be run with `just <command>`:

```bash
just build      # Build all projects
just test       # Run all tests
just clean      # Clean build artifacts
```

## 🔧 Architecture

KlabTranslations uses a modular architecture:

1. **Generator** - Reads `Translations.csv` at compile-time
2. **Code Generation** - Creates strongly-typed `Translations` class
3. **Core Library** - Provides `TranslationUnit` and parameter resolution
4. **Framework Adapters** - Integrate translations with specific UI frameworks

This approach ensures:
- ✅ Compile-time type safety
- ✅ Zero runtime overhead for static translations
- ✅ Developer-friendly API
- ✅ Easy maintenance

## 🆘 Support

For issues, questions, or feature requests, please open an issue in the repository.
