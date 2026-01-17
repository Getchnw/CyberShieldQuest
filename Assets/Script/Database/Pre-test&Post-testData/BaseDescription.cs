using UnityEngine;


public abstract class BaseDescription : ScriptableObject
{
    [TextArea(3, 5)]
    public string DescriptionContext;
    public string UserAnswertext; // เก็บคำตอบที่ผู้ใช้เลือก
}

