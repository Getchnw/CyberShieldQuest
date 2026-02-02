using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CleanupAllScenes : EditorWindow
{
    [MenuItem("Tools/Cleanup All Scenes")]
    static void CleanUpAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int totalRemoved = 0;
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(scenePath);
            
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            int sceneRemoved = 0;
            
            foreach (GameObject go in allObjects)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                sceneRemoved += removed;
            }
            
            if (sceneRemoved > 0)
            {
                Debug.Log($"Scene: {scene.name} - Removed {sceneRemoved} missing scripts");
                EditorSceneManager.SaveScene(scene);
                totalRemoved += sceneRemoved;
            }
        }
        
        if (totalRemoved > 0)
        {
            Debug.Log($"<color=green>Cleanup complete! Removed {totalRemoved} missing scripts from all scenes</color>");
            EditorUtility.DisplayDialog("Cleanup Complete", $"Removed {totalRemoved} missing scripts from all scenes", "OK");
        }
        else
        {
            Debug.Log("<color=yellow>No missing scripts found in any scene</color>");
            EditorUtility.DisplayDialog("Cleanup Complete", "No missing scripts found", "OK");
        }
    }
}
