# ZedUnity セットアップガイド

## 概要

ZedUnity は Unity パッケージ（`unity-package/`）として提供されます。
Unity Editor から Zed を外部エディタとして登録し、.csproj / .sln および Zed ワークスペース設定を自動生成します。

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
https://github.com/seless-yuu/ZedUnity.git?path=/unity-package
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

## Part 2: IntelliSense 設定

### 2-1. Roslyn（C# Language Server）

Zed の C# サポートには Roslyn を使用します。
Roslyn は Zed が自動で管理するため、**追加のインストールは不要**です。

### 2-2. Zed の設定

「Regenerate .csproj / .sln」を実行すると、`.zed/settings.json` が自動生成されます（既存の場合はスキップ）。

生成される `.zed/settings.json`：
```json
{
  "languages": {
    "CSharp": {
      "language_servers": ["roslyn", "!omnisharp", "..."],
      "format_on_save": "off"
    }
  }
}
```

> **注意**: Zed の C# 言語名は `"CSharp"` です（`"C#"` ではありません）。

---

## トラブルシューティング

| 問題 | 解決策 |
|---|---|
| Zed がドロップダウンに表示されない | `Edit → Preferences → External Tools → Browse...` で手動指定 |
| IntelliSense が効かない | `.zed/settings.json` の言語名が `"CSharp"`、`language_servers` に `"roslyn"` が含まれているか確認。「Regenerate .csproj / .sln」を再実行 |
