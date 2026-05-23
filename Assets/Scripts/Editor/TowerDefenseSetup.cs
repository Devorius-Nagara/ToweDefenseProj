// This file MUST be in an "Editor" folder — it is excluded from runtime builds.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs after every script compilation.
/// Adds GameBootstrap to SampleScene automatically so the user only needs to press Play.
/// </summary>
[InitializeOnLoad]
public static class TowerDefenseSetup
{
    static TowerDefenseSetup()
    {
        EditorApplication.delayCall += EnsureBootstrapInScene;
    }

    [MenuItem("TowerDefense/Setup Scene")]
    public static void EnsureBootstrapInScene()
    {
        var scene = SceneManager.GetActiveScene();

        // Only run when SampleScene is open
        if (!scene.name.Contains("Sample") && !scene.name.Contains("Tower"))
        {
            Debug.Log("[TowerDefense] Open SampleScene then run TowerDefense > Setup Scene.");
            return;
        }

        // Don't add if already present
        var existing = Object.FindFirstObjectByType<GameBootstrap>();
        if (existing != null) return;

        var go = new GameObject("GameBootstrap");
        go.AddComponent<GameBootstrap>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[TowerDefense] ✓ GameBootstrap added to scene. Press Play!");
    }
}
