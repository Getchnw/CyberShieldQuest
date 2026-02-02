using UnityEngine;
using UnityEditor;

public class CleanupMissingScripts : EditorWindow
{
    [MenuItem("Tools/Cleanup Missing Scripts")]
    static void CleanUp()
    {
        int count = 0;
        
        // ค้นหาทุก GameObject ใน Scene ปัจจุบัน
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject go in allObjects)
        {
            // นับจำนวน components ก่อน
            int componentsBefore = go.GetComponents<Component>().Length;
            
            // ลบ missing components
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            
            if (removed > 0)
            {
                Debug.Log($"Removed {removed} missing script(s) from: {go.name}", go);
                count += removed;
            }
        }
        
        if (count > 0)
        {
            Debug.Log($"<color=green>Cleanup complete! Removed {count} missing script(s)</color>");
            EditorUtility.DisplayDialog("Cleanup Complete", $"Removed {count} missing script(s)", "OK");
        }
        else
        {
            Debug.Log("<color=yellow>No missing scripts found</color>");
            EditorUtility.DisplayDialog("Cleanup Complete", "No missing scripts found", "OK");
        }
    }
}
