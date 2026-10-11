# KlabTranslations

[![CI](https://github.com/klab365/KlabTranslations/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/klab365/KlabTranslations/actions/workflows/ci.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=klab365_KlabTranslations&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=klab365_KlabTranslations)

KlabTranslations generates strongly typed C# translation units from a CSV file at build time. The generated API can be used from ordinary .NET code or through the optional Avalonia and Blazor adapters.

## Getting started

### Requirements

- A .NET project (the repository currently targets the .NET SDK version declared in `global.json`/`mise.toml`; see the project files if targeting a different SDK).
- The `KlabTranslations.Generator` NuGet package.
- A file named exactly `Translations.csv` included as an MSBuild `AdditionalFiles` item.

### 1. Add the generator

Install `KlabTranslations.Generator` from NuGet, or add the package reference:

```xml
<ItemGroup>
  <PackageReference Include="KlabTranslations.Generator" Version="<current-version>" />
  <AdditionalFiles Include="Translations.csv" />
</ItemGroup>
```

The source generator runs during compilation; no runtime CSV loading is required. The package supplies the core types used by generated code. Add an adapter only when using its framework integration.

### 2. Create `Translations.csv`

```csv
Key,de-DE,en-US
hello_world,Hallo Welt,Hello world
welcome_user,"Willkommen, {0}!","Welcome, {0}!"
```

The first column must be `Key`; every other header is a culture name such as `de-DE` or `en-US`. Each data row must have a unique, non-empty key and one value per language column. Quote CSV fields containing commas and escape quotes using standard CSV double-quote escaping.

Plural forms can be expressed by adding a category to a culture column, for example `en-US.one` and `en-US.other`. Supported categories are `zero`, `one`, `two`, `few`, `many`, and `other`.

### 3. Use the generated API

The generator creates `Strings` in `<AssemblyName>.Translations` (normally your project's root namespace plus `.Translations`). For example, if the assembly name is `MyApp`:

```csharp
using MyApp.Translations;

string text = Strings.hello_world.ToString();
string german = Strings.hello_world.ToString("de-DE");
string greeting = Strings.welcome_user.Resolve(new object?[] { "Ada" });
```

Use your actual assembly name in the `using` directive. Generated source is produced during build and may not appear in the source tree until the project has compiled.

For an end-to-end runnable example, see [the console sample](samples/KlabTranslations.ConsoleExample/). Avalonia and Blazor examples are in `samples/` as well.

## Culture and fallback

Lookups use the requested/current culture, then its parent culture where available, then the invariant/default translation, and finally the key itself. An absent translation therefore does not fail lookup. For parameterized strings, provide values matching the `{0}`, `{1}`, … placeholders.

## Framework adapters

- **Avalonia:** reference `KlabTranslations.Generator.Avalonia`; it includes the source generator. See [the Avalonia sample](samples/KlabTranslations.AvaloniaExample/).
- **Blazor:** reference `KlabTranslations.Generator.Blazor`; it includes the source generator. Register the service with `builder.Services.AddKlabTranslations()` and use `<Translate Unit="Strings.hello_world" />`. Culture state is scoped to the Blazor circuit. See [the Blazor sample](samples/KlabTranslations.BlazorExample/).

## FAQ / common stumbling blocks

### No `Strings` class or generated translations

Check that the file is named exactly `Translations.csv`, is included as an `<AdditionalFiles>` item in the project that compiles it, and has content. Then rebuild. The generator searches for that filename; a differently named CSV is not picked up.

### `Strings` is in a different namespace than expected

The generated namespace is based on the assembly name, followed by `.Translations`. Check `<AssemblyName>` in your `.csproj` and adjust the `using` statement accordingly.

### Build fails with a CSV diagnostic

The header must start with `Key`, have at least one non-empty language column, and contain no duplicate column names. Every row must have the same number of fields, unique non-empty keys, and valid plural category suffixes. Inspect the diagnostic for the file and line.

### A value containing a comma breaks the CSV

Wrap the field in double quotes, e.g. `hello,"Hallo, Welt"`. A literal double quote inside a quoted field must be doubled (`""`).

### Translation shows the key or another language

Verify the culture name/header and that the requested culture is set as expected. Missing entries fall back through the parent culture, then the invariant value, and ultimately the key. An empty/missing value is not a translated string.

### Parameter placeholders are not replaced

Use `Resolve(...)` and supply parameters in placeholder order, starting at index 0. Check that the number and types of supplied values are appropriate for the format string.

### Avalonia/Blazor markup cannot resolve the translation

Ensure the matching adapter package is referenced in addition to the generator, and follow the sample's XML/Razor namespaces and service registration. Rebuild after changing package references.

## Packages

| Package | Purpose |
|---|---|
| `KlabTranslations.Generator` | CSV source generator and core translation API |
| `KlabTranslations.Generator.Avalonia` | Avalonia XAML markup integration |
| `KlabTranslations.Generator.Blazor` | Blazor translation component and scoped culture context |

## Repository development

The repository uses `mise.toml` for tool versions and tasks. Install the configured tools and restore dependencies, then run:

```bash
mise install
mise run build
mise run test
mise run check-format
```

Runnable samples are under `samples/`; source projects and tests are under `src/`. For issues or feature requests, open a GitHub issue.
