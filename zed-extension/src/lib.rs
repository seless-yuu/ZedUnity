use std::fs;
use zed_extension_api::{self as zed, settings::LspSettings, LanguageServerId, Result, Worktree};

/// Unity C# Zed extension.
///
/// Configures OmniSharp to work correctly with Unity-generated .csproj files.
/// The Unity package (com.zedunity.ide.zed) generates the .csproj / .sln files
/// and writes an initial .zed/settings.json; this extension fine-tunes OmniSharp
/// startup based on the workspace layout detected at runtime.
struct UnityCSharpExtension;

impl zed::Extension for UnityCSharpExtension {
    fn new() -> Self {
        UnityCSharpExtension
    }

    /// Returns the command used to start OmniSharp for a C# Unity workspace.
    ///
    /// OmniSharp must be installed separately.
    /// Recommended install methods:
    ///   - `dotnet tool install -g omnisharp`  (requires .NET SDK)
    ///   - Manually download from https://github.com/OmniSharp/omnisharp-roslyn/releases
    ///
    /// Zed also ships built-in C# support via the `csharp` extension; this
    /// extension supplements it with Unity-specific initialization options.
    fn language_server_command(
        &mut self,
        _language_server_id: &LanguageServerId,
        worktree: &Worktree,
    ) -> Result<zed::Command> {
        // Detect the OmniSharp binary.
        // Checks (in order):
        //   1. User-configured path in lsp.omnisharp.binary
        //   2. `omnisharp` on PATH (installed via `dotnet tool install -g omnisharp`)
        //   3. `OmniSharp` on PATH (capitalised, common on Windows)
        let settings = LspSettings::for_worktree("omnisharp", worktree)
            .ok()
            .and_then(|s| s.binary);

        let (binary, mut args) = if let Some(b) = settings {
            let path = b.path.unwrap_or_else(|| "omnisharp".to_string());
            let extra = b.arguments.unwrap_or_default();
            (path, extra)
        } else {
            ("omnisharp".to_string(), vec![])
        };

        // Core OmniSharp arguments for Unity projects.
        // --languageserver  – run in language-server (stdio) mode
        // -s <path>         – solution / project root
        let root = worktree.root_path();
        args.extend([
            "--languageserver".to_string(),
            "-s".to_string(),
            root.to_string(),
            // Enable import completion (shows types from referenced assemblies)
            "RoslynExtensionsOptions:EnableImportCompletion=true".to_string(),
        ]);

        // If a .sln file exists in the workspace root, point OmniSharp at it
        // directly so it picks up all Unity assembly projects automatically.
        let sln = find_sln(&root);
        if let Some(sln_path) = sln {
            // Replace the generic `-s root` with the specific solution path
            let s_pos = args.iter().position(|a| a == "-s");
            if let Some(pos) = s_pos {
                args[pos + 1] = sln_path;
            }
        }

        Ok(zed::Command {
            command: binary,
            args,
            env: Default::default(),
        })
    }
}

/// Searches the workspace root for a Unity .sln file.
fn find_sln(root: &str) -> Option<String> {
    let entries = fs::read_dir(root).ok()?;
    for entry in entries.flatten() {
        let path = entry.path();
        if path.extension().and_then(|e| e.to_str()) == Some("sln") {
            return path.to_str().map(str::to_owned);
        }
    }
    None
}

zed::register_extension!(UnityCSharpExtension);
