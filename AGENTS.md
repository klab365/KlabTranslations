# Project guide

## Project
- KlabTranslations is a .NET solution. Shared build, package, analyzer, and test settings are in `Directory.Build.props`.
- Source projects are under `src/`; runnable examples are under `samples/`.
- Use the `Justfile` as the canonical entry point for development commands.

## Development and validation
- `just build` builds the solution.
- `just test` runs the test suite.
- `just check-format` verifies formatting.
- Keep nullable reference types and warnings-as-errors in mind; do not suppress diagnostics without a clear reason.

## Releases
- Use Conventional Commits (for example, `feat:`, `fix:`, `docs:`, `chore:`). Release Please uses these commit types to determine version bumps and release notes.
- Release Please opens/updates a release PR from changes on `main`; merging it updates `Changelog.md`, `release-please-manifest.json`, and creates a `V*` tag/GitHub release.
- The CD workflow builds and publishes NuGet packages for the created version tag. Do not manually edit generated release versions or tags.
