using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MQ_NewQuestion", menuName = "Pre&Post-Test/Matching Question")]
public class MatchingQuestion : BaseTestQuestion
{
    [Header("โจทย์")]
    public List<string> prompts; // 1, 2, 3, 4 (ทางซ้าย) 
    [Header("ตัวเลือก")]
    public List<string> options; // A, B, C, D (ทางขวา) 

    // เฉลย: [2, 0, 1, 3] หมายถึง 
    // Prompt 0 -> Option 2 (1-C)
    // Prompt 1 -> Option 0 (2-A)
    // Prompt 2 -> Option 1 (3-B)
    [Header("ลำดับคำตอบ")]// Prompt 3 -> Option 3 (4-D) [cite: 6]
    public List<int> correctAnswers_OptionIndex;

    public StoryData story;
}