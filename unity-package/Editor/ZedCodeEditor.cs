using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZedUnity.Editor
{
    /// <summary>
    /// Integrates Zed Editor with Unity 6.
    ///
    /// Since com.unity.code-editor was removed in Unity 6, this implementation:
    ///   - Uses [OnOpenAsset] to intercept C# file opens and forward them to Zed
    ///   - Provides a Preferences page at "Preferences / Zed Editor"
    ///   - Adds menu items under "Tools / Zed Editor"
    /// </summary>
    [InitializeOnLoad]
    public static class ZedCodeEditor
    {
        // -----------------------------------------------------------------------
        // EditorPrefs keys
        // -----------------------------------------------------------------------

        internal const string k_EnabledKey = "ZedUnity.Enabled";
        internal const string k_EditorPathKey = "ZedUnity.EditorPath";

        // -----------------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------------

        static ZedCodeEditor()
        {
            // Auto-detect Zed on first load if no path is configured yet
            if (string.IsNullOrEmpty(EditorPrefs.GetString(k_EditorPathKey)))
            {
                var detected = ZedDiscovery.GetInstallationPaths().FirstOrDefault();
                if (!string.IsNullOrEmpty(detected))
                    EditorPrefs.SetString(k_EditorPathKey, detected);
            }
        }

        // -----------------------------------------------------------------------
        // [OnOpenAsset] – intercept C# / shader file opens
        // -----------------------------------------------------------------------

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line, int column)
        {
            if (!EditorPrefs.GetBool(k_EnabledKey, false))
                return false;

            var assetPath = AssetDatabase.GetAssetPath(instanceID);
            if (!IsTextAsset(assetPath))
                return false;

            var editorPath = EditorPrefs.GetString(k_EditorPathKey);
            if (string.IsNullOrEmpty(editorPath))
            {
                UnityEngine.Debug.LogWarning(
                    "[ZedUnity] Zed path is not configured. " +
                    "Open Preferences → Zed Editor to set it.");
                return false;
            }

            var fullPath = Path.GetFullPath(assetPath);
            var args = BuildArgs(fullPath, line, column);
            return Launch(editorPath, args);
        }

        private static bool IsTextAsset(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".cs" or ".shader" or ".hlsl" or ".cginc" or ".glsl"
                      or ".asmdef" or ".asmref" or ".json" or ".xml";
        }

        // -----------------------------------------------------------------------
        // Menu items
        // -----------------------------------------------------------------------

        [MenuItem("Tools/Zed Editor/Open Project in Zed")]
        public static void OpenProjectInZed()
        {
            var editorPath = EditorPrefs.GetString(k_EditorPathKey);
            if (string.IsNullOrEmpty(editorPath))
            {
                EditorUtility.DisplayDialog("Zed Editor",
                    "Zed path is not configured.\nOpen Preferences → Zed Editor.", "OK");
                return;
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            Launch(editorPath, Quote(projectRoot));
        }

        [MenuItem("Tools/Zed Editor/Regenerate Project Files")]
        public static void RegenerateProjectFiles()
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var gen = new ProjectGeneration.ProjectGeneration(projectRoot);
            gen.GenerateAll();
            EditorUtility.DisplayDialog("Zed Editor",
                "Project files regenerated.\nReopen the project in Zed to apply changes.", "OK");
        }

        // -----------------------------------------------------------------------
        // Internal helpers
        // -----------------------------------------------------------------------

        private static string BuildArgs(string filePath, int line, int column)
        {
            // Zed CLI: zed <project_root> <path>:<line>:<col>
            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            var fileArg = ZedDiscovery.BuildOpenFileArgs(filePath, line, column);
            return $"{Quote(projectRoot)} {fileArg}";
        }

        internal static bool Launch(string executable, string args)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = args,
                    UseShellExecute = true,
                });
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ZedUnity] Failed to launch Zed: {ex.Message}");
                return false;
            }
        }

        private static string Quote(string value) =>
            value.Contains(' ') ? $"\"{value}\"" : value;
    }

    // -----------------------------------------------------------------------
    // Preferences page
    // -----------------------------------------------------------------------

    internal static class ZedEditorSettings
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Preferences/Zed Editor", SettingsScope.User)
            {
                label = "Zed Editor",
                guiHandler = DrawGUI,
                keywords = new HashSet<string> { "zed", "editor", "ide", "external" },
            };
        }

        private static void DrawGUI(string searchContext)
        {
            var enabled = EditorPrefs.GetBool(ZedCodeEditor.k_EnabledKey, false);
            var editorPath = EditorPrefs.GetString(ZedCodeEditor.k_EditorPathKey, string.Empty);

            EditorGUILayout.Space(4);

            // --- Enable toggle ---
            var newEnabled = EditorGUILayout.Toggle("Use Zed as Script Editor", enabled);
            if (newEnabled != enabled)
                EditorPrefs.SetBool(ZedCodeEditor.k_EnabledKey, newEnabled);

            EditorGUILayout.Space(8);

            // --- Path field ---
            EditorGUI.BeginDisabledGroup(!newEnabled);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Zed Executable Path");
                var newPath = EditorGUILayout.TextField(editorPath);
                if (newPath != editorPath)
                    EditorPrefs.SetString(ZedCodeEditor.k_EditorPathKey, newPath);

                if (GUILayout.Button("Browse…", GUILayout.Width(70)))
                {
                    var picked = EditorUtility.OpenFilePanel("Select Zed executable", "", "exe");
                    if (!string.IsNullOrEmpty(picked))
                        EditorPrefs.SetString(ZedCodeEditor.k_EditorPathKey, picked);
                }
            }

            // --- Auto-detect ---
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Auto-Detect", GUILayout.Width(100)))
                {
                    var detected = ZedDiscovery.GetInstallationPaths().FirstOrDefault();
                    if (!string.IsNullOrEmpty(detected))
                    {
                        EditorPrefs.SetString(ZedCodeEditor.k_EditorPathKey, detected);
                        UnityEngine.Debug.Log($"[ZedUnity] Detected Zed at: {detected}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Zed Editor",
                            "Could not auto-detect Zed. Please set the path manually.", "OK");
                    }
                }
            }

            EditorGUILayout.Space(8);

            // --- Project files ---
            EditorGUILayout.LabelField("Project Files", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Regenerate .csproj / .sln", GUILayout.Width(200)))
                    ZedCodeEditor.RegenerateProjectFiles();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "When enabled, double-clicking a C# file in Unity opens it in Zed " +
                "at the correct line.\n\n" +
                "IntelliSense requires OmniSharp: dotnet tool install -g omnisharp\n" +
                "For debug support see docs/setup-guide.md.",
                MessageType.Info);
        }
    }
}
