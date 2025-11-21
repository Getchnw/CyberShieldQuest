using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SentenceBlank
{
    [TextArea(2, 4)] public string sentencePart1; // "5. ...ที่ดีที่สุด คือการ"
    [TextArea(2, 4)] public string sentencePart2; // "ID ให้เป็นรหัสสุ่ม..." [cite: 35, 37]
    public string correctAnswer; // "พรางตา (GUID)" [cite: 34]
}

[CreateAssetMenu(fileName = "FB_NewQuestion", menuName = "Pre&Post-Test/Fill in the Blank")]
public class FillInBlankQuestion : BaseTestQuestion
{
    public List<string> wordBank; // คลังคำ 
    public List<SentenceBlank> sentences; // ประโยคที่มีช่องว่าง
    public StoryData story;
}