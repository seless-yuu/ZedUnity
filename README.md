# ZedUnity

Zed Editor の Unity 統合。Unity で Zed を外部スクリプトエディタとして使用し、OmniSharp による IntelliSense とデバッグを有効にします。

## 構成

```
ZedUnity/
├── unity-package/          # Unity パッケージ (com.zedunity.ide.zed)
│   └── Editor/
│       ├── ZedCodeEditor.cs        # IExternalCodeEditor 実装
│       ├── ZedDiscovery.cs         # Zed インストール検出
│       └── ProjectGeneration/
│           └── ProjectGeneration.cs # .csproj / .sln 生成
├── zed-extension/          # Zed 拡張機能 (Rust/WASM)
│   ├── extension.toml
│   └── src/lib.rs          # OmniSharp LSP 設定
├── .zed/
│   └── settings.json       # Zed ワークスペース設定テンプレート
└── .vscode/
    └── launch.json         # デバッグ設定テンプレート
```

## クイックスタート

### 1. Unity パッケージをインストール

Package Manager → `+` → "Add package from disk..." → `unity-package/package.json`

### 2. Zed を外部エディタに設定

`Edit → Preferences → External Tools` → External Script Editor: **Zed**

### 3. プロジェクトファイルを生成

同じ Preferences 画面で **"Regenerate .csproj / .sln"** をクリック。
これにより `.zed/settings.json` と `omnisharp.json` も自動生成されます。

### 4. デバッグ（オプション）

Unity のデバッグには [vscode-unity-debug](https://github.com/Unity-Technologies/vscode-unity-debug/releases)
DAP アダプターが必要です。詳細は [docs/setup-guide.md](docs/setup-guide.md) を参照してください。

## 必要環境

- Unity 6 (6000.3) 以上
- Zed Editor（[zed.dev](https://zed.dev)）
- OmniSharp（`dotnet tool install -g omnisharp`）

## ライセンス

MIT
