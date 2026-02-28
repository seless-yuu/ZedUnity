# ZedUnity セットアップガイド

## 概要

ZedUnity は2つのコンポーネントで構成されています：

| コンポーネント | 役割 |
|---|---|
| `unity-package/` | Unity Editor から Zed を外部エディタとして登録する Unity パッケージ |
| `zed-extension/` | OmniSharp LSP を Unity プロジェクト向けに設定する Zed 拡張機能 |

---

## Part 1: Unity → Zed 外部エディタ設定

### 1-1. Unity パッケージのインストール

**方法A: ローカルパスから追加（推奨・開発中）**

1. Unity Editor を開く
2. `Window → Package Manager` を開く
3. 左上の `+` ボタン → **"Add package from disk..."**
4. `ZedUnity/unity-package/package.json` を選択

**方法B: Git URL から追加**

Package Manager → `+` → **"Add package from git URL..."** に以下を入力：
```
https://github.com/seless/ZedUnity.git?path=/unity-package
```

### 1-2. Zed を外部エディタとして設定

1. Unity メニュー: `Edit → Preferences → External Tools`
2. **External Script Editor** ドロップダウンから **Zed** を選択
   - 表示されない場合は **Browse...** で Zed の実行ファイルを手動指定
     Windows: `%LOCALAPPDATA%\Programs\Zed\zed.exe`
3. **"Regenerate .csproj / .sln"** ボタンをクリック

### 1-3. 動作確認

Unity の Project ウィンドウで任意の `.cs` ファイルをダブルクリック →
Zed がファイルを正しい行で開けば成功です。

---

## Part 2: OmniSharp / IntelliSense 設定

### 2-1. OmniSharp のインストール

Zed の C# サポートは OmniSharp（または Roslyn）を使用します。

**.NET SDK がある場合（推奨）:**
```bash
dotnet tool install -g omnisharp
```

**手動インストール:**
[OmniSharp リリースページ](https://github.com/OmniSharp/omnisharp-roslyn/releases) から
プラットフォームに合うバイナリをダウンロードし、PATH の通った場所に配置。

### 2-2. Zed の設定

`unity-package` を使って「Regenerate .csproj / .sln」を実行すると、自動で以下が生成されます：

- `<ProjectRoot>/omnisharp.json` — OmniSharp 設定
- `<ProjectRoot>/.zed/settings.json` — Zed ワークスペース設定

生成された `.zed/settings.json` の内容（カスタマイズ可）：
```json
{
  "languages": {
    "C#": {
      "language_servers": ["omnisharp"],
      "format_on_save": "off"
    }
  },
  "lsp": {
    "omnisharp": {
      "initialization_options": {
        "RoslynExtensionsOptions": {
          "enableImportCompletion": true
        }
      }
    }
  }
}
```

---

## Part 3: デバッグ設定

Unity のデバッグには **DAP（Debug Adapter Protocol）** を使用します。

### 3-1. Unity Debug Adapter のインストール

1. [vscode-unity-debug releases](https://github.com/Unity-Technologies/vscode-unity-debug/releases) から
   最新の `.vsix` ファイルをダウンロード
2. `.vsix` は ZIP 形式なので、拡張子を `.zip` に変更して展開
3. 展開先の `extension/` ディレクトリ（アダプター本体）を任意の場所に配置
   （例: `~/.local/share/unity-debug/`）

> **注意**: `npm install -g unity-debug` というパッケージは存在しません。
> VS Code を使用している場合は、マーケットプレイスから `Unity.unity-debug` 拡張をインストールすると
> 自動的に `~/.vscode/extensions/` 以下に展開されます。

### 3-2. Unity 側の設定

`Edit → Preferences → External Tools` で **"Editor Attaching"** にチェックを入れる。

### 3-3. launch.json の配置

`ZedUnity/.vscode/launch.json` をUnity プロジェクトルートの `.vscode/` にコピー：

```bash
cp ZedUnity/.vscode/launch.json <UnityProjectRoot>/.vscode/launch.json
```

Zed はフォールバックとして `.vscode/launch.json` を読み込みます。

### 3-4. デバッグの開始

1. Unity Editor でゲームを **Play** モードで実行
2. Zed で `F4` (または `debugger: start`) → **"Attach to Unity Editor"** を選択
3. ブレークポイントが機能することを確認

### デバッグ設定の詳細

```json
{
  "name": "Attach to Unity Editor",
  "type": "unity",
  "request": "attach"
}
```

| フィールド | 説明 |
|---|---|
| `type: "unity"` | Unity Debug Adapter を使用 |
| `request: "attach"` | 実行中の Unity プロセスにアタッチ |
| `address` | デフォルト: `localhost`（リモートデバッグ時に変更） |
| `port` | デフォルト: 自動検出（手動指定: `56000`） |

---

## Zed 拡張機能（オプション）

`zed-extension/` ディレクトリに OmniSharp を Unity 向けに設定する Zed 拡張があります。
Rust/Cargo が必要です：

```bash
cd zed-extension
rustup target add wasm32-wasip1
cargo build --release --target wasm32-wasip1
```

ビルドした拡張は Zed の開発モードでロード可能：
Zed: `zed: install dev extension` → `zed-extension/` ディレクトリを指定

---

## トラブルシューティング

| 問題 | 解決策 |
|---|---|
| Zed がドロップダウンに表示されない | `Edit → Preferences → External Tools → Browse...` で手動指定 |
| IntelliSense が効かない | 「Regenerate .csproj / .sln」を再実行、OmniSharp を再起動 |
| ブレークポイントが機能しない | Unity の "Editor Attaching" が有効か確認、vscode-unity-debug アダプターのインストールを確認 |
| `omnisharp` が見つからない | `dotnet tool install -g omnisharp` を実行し、PATH を確認 |
