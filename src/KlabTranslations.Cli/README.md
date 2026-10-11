# KlabTranslations CLI

Install the .NET tool globally:

```sh
dotnet tool install --global KlabTranslations.Cli
```

Or install it with [mise](https://mise.jdx.dev/) as a project tool:

```sh
mise use dotnet:KlabTranslations.Cli
```

This adds the tool to `mise.toml`; run `mise install` to install it and `mise exec -- klab-translations Translations.csv` to invoke it. (The package/tool name is `KlabTranslations.Cli`; the executable command is `klab-translations`.)

Validate a translation CSV and print missing translations and per-language coverage:

```sh
klab-translations Translations.csv
```

Exit codes: `0` valid and complete CSV, `1` invalid CSV or missing translations, `2` invalid usage or missing file.
