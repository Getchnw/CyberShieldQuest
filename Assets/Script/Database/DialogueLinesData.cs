using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog Line" ,menuName = "Game Content/Dialog Lines")]

public class DialogueLinesData : ScriptableObject
{
    public int line_id;
    public string Dialog_Text;
    [Header("ลำดับของบทพูด")]
    public int sequence_order;
    public DialogsceneData scene_id;
}


