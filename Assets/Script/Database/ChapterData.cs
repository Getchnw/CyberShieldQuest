using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chapter" ,menuName = "Game Content/Chapter")]
public class ChapterData : ScriptableObject
{
   public int chapter_id;
   public string chapterName;
   public StoryData story;
}
