using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialog Line" ,menuName = "Game Content/Dialog Lines")]

public class DialogueLinesData : ScriptableObject
{
    public int line_id;
    public List<string> Dialog_Text;
    [Header("ลำดับของบทพูด")]
    public int sequence_order;
    public DialogsceneData scene;
    public CharacterData character;
}


