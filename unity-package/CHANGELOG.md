# Changelog

## [0.1.0] - 2026-02-27

### Added
- Initial release
- `ZedCodeEditor` – `IExternalCodeEditor` implementation registering Zed as Unity's external script editor
- `ZedDiscovery` – automatic Zed installation detection for Windows, macOS, Linux
- `ProjectGeneration` – generates `.csproj` and `.sln` files from `CompilationPipeline` assemblies
- Auto-generates `omnisharp.json` and `.zed/settings.json` on project sync
- Preferences GUI panel with "Regenerate .csproj / .sln" button
- File open with line/column support: `zed path:line:column`
