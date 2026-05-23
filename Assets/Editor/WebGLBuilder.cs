using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Headless WebGL builder invoked via:
//   Unity -batchmode -nographics -quit -projectPath . \
//         -buildTarget WebGL -executeMethod WebGLBuilder.Build
// Output: <projectPath>/Build/WebGL/
public static class WebGLBuilder
{
    public static void Build()
    {
        // Disable compression — GitHub Pages cannot serve Brotli/Gzip
        // with the Content-Encoding header Unity's loader expects.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        // Resolve scenes: prefer Build Settings list; fall back to every .unity asset.
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToList();

        if (scenes.Count == 0)
        {
            scenes = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .ToList();
            Debug.Log($"[WebGLBuilder] Build Settings is empty; using {scenes.Count} discovered scenes.");
        }

        if (scenes.Count == 0)
        {
            Debug.LogError("[WebGLBuilder] No scenes found — aborting.");
            EditorApplication.Exit(2);
            return;
        }

        var outDir = Path.Combine(Directory.GetCurrentDirectory(), "Build", "WebGL");
        Directory.CreateDirectory(outDir);

        var opts = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        Debug.Log($"[WebGLBuilder] Building {scenes.Count} scene(s) → {outDir}");
        foreach (var s in scenes) Debug.Log($"[WebGLBuilder]   scene: {s}");

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        var summary = report.summary;
        Debug.Log($"[WebGLBuilder] Result: {summary.result}, size={summary.totalSize} bytes, errors={summary.totalErrors}, warnings={summary.totalWarnings}");

        EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
