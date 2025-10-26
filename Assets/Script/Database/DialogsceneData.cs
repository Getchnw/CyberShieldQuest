using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog Scenes" ,menuName = "Game Content/Dialog Scenes")]

public class DialogsceneData : ScriptableObject
{
   public int scene_id;
   public string sceneName;
   // Use Sprite (capital S) for image assets used in UI/2D
   public Sprite backgroundScene;
}


