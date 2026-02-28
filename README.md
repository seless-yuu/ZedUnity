# ZedUnity

Unity integration for Zed Editor. Use Zed as your external script editor with IntelliSense powered by OmniSharp.

[日本語](README.ja.md)

## Quick Start

### 1. Install the Unity package

**Via Git URL (recommended):**

Package Manager → `+` → "Add package from git URL..." and enter:
```
https://github.com/seless-yuu/ZedUnity.git?path=/unity-package
```

**Via local disk:**

Package Manager → `+` → "Add package from disk..." → `unity-package/package.json`

### 2. Set Zed as the external editor

`Edit → Preferences → External Tools` → External Script Editor: **Zed**

### 3. Generate project files

Click **"Regenerate .csproj / .sln"** in the same Preferences panel.
This also auto-generates `.zed/settings.json` and `omnisharp.json`.

## Requirements

- Unity 2020.3 or later
- Zed Editor ([zed.dev](https://zed.dev))
- OmniSharp (`dotnet tool install -g omnisharp`)

## License

MIT
