# Blazor example

From the repository root, run `dotnet run --project samples/KlabTranslations.BlazorExample` and open the displayed URL. The sample uses interactive server rendering; the culture buttons and name input require the Blazor connection to be active.

The project references `KlabTranslations.Generator`, `KlabTranslations.Core`, and `KlabTranslations.Generator.Blazor`, and includes `Translations.csv` as an additional file. Register the adapter with `builder.Services.AddKlabTranslations()`, then use `<Translate Unit="Strings.hello_world" />` in Razor. `TranslationContext` is scoped per Blazor circuit, so changing one user's culture does not affect other users. The component supports named parameters through its `Parameters` dictionary. Missing culture translations fall back using the core library's normal key/invariant behavior.
