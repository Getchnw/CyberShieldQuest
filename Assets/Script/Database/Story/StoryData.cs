using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Story" ,menuName = "Game Content/Story")]
public class StoryData : ScriptableObject
{
    public enum Status
    {
        Ongoing,
        Comingsoon
    }

    public string story_id;
    public string storyName;
    [TextArea(3,6)]
    public string description;
    public Status storyStatus;
    public Sprite artwork;
}
