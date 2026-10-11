#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
version="0.0.0-consumertest"
packages="$(mktemp -d "${TMPDIR:-/tmp}/klab-package-consumer.XXXXXX")"
trap 'rm -rf "$packages"' EXIT

# Build first so pack has the adapter assemblies available. The adapter targets
# package the generator DLL into analyzers/dotnet/cs. Include Core in the local
# feed because the adapters declare it as a package dependency.
dotnet build "$root/src/KlabTranslations.Core/KlabTranslations.Core.csproj" -c Release -p:Version="$version"
dotnet pack "$root/src/KlabTranslations.Core/KlabTranslations.Core.csproj" -c Release -p:Version="$version" --no-build -o "$packages"
dotnet build "$root/src/KlabTranslations.Generator.Avalonia/KlabTranslations.Generator.Avalonia.csproj" -c Release -p:Version="$version"
dotnet pack "$root/src/KlabTranslations.Generator.Avalonia/KlabTranslations.Generator.Avalonia.csproj" -c Release -p:Version="$version" --no-build -o "$packages"
dotnet build "$root/src/KlabTranslations.Generator.Blazor/KlabTranslations.Generator.Blazor.csproj" -c Release -p:Version="$version"
dotnet pack "$root/src/KlabTranslations.Generator.Blazor/KlabTranslations.Generator.Blazor.csproj" -c Release -p:Version="$version" --no-build -o "$packages"

for framework in Avalonia Blazor; do
  project="$packages/$framework.Consumer"
  dotnet new console --framework net10.0 --output "$project" --no-restore
  cat > "$project/NuGet.Config" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF
  if [[ "$framework" == "Avalonia" ]]; then
    package="KlabTranslations.Generator.Avalonia"
  else
    package="KlabTranslations.Generator.Blazor"
  fi
  dotnet add "$project" package "$package" --version "$version"
  cat > "$project/Translations.csv" <<'EOF'
Key,en-US
hello,Hello
EOF
  cat > "$project/Program.cs" <<EOF
using $framework.Consumer.Translations;
Console.WriteLine(Strings.hello.ToString());
EOF
  python3 - "$project/$framework.Consumer.csproj" <<'PY'
import sys
from pathlib import Path
p = Path(sys.argv[1])
s = p.read_text()
s = s.replace('</Project>', '  <ItemGroup><AdditionalFiles Include="Translations.csv" /></ItemGroup>\n</Project>')
p.write_text(s)
PY
  dotnet build "$project" --no-restore
  dotnet build "$project"
done
