# ZedUnity

Zed Editor の Unity 統合。Unity で Zed を外部スクリプトエディタとして使用し、OmniSharp による IntelliSense とデバッグを有効にします。

## クイックスタート

### 1. Unity パッケージをインストール

**Git URL（推奨）:**

Package Manager → `+` → "Add package from git URL..." に以下を入力：
```
https://github.com/seless-yuu/ZedUnity.git?path=/unity-package
```

**ローカルから追加:**

Package Manager → `+` → "Add package from disk..." → `unity-package/package.json`

### 2. Zed を外部エディタに設定

`Edit → Preferences → External Tools` → External Script Editor: **Zed**

### 3. プロジェクトファイルを生成

同じ Preferences 画面で **"Regenerate .csproj / .sln"** をクリック。
これにより `.zed/settings.json` と `omnisharp.json` も自動生成されます。

## 必要環境

- Unity 6 (6000.3) 以上
- Zed Editor（[zed.dev](https://zed.dev)）
- OmniSharp（`dotnet tool install -g omnisharp`）
