using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "A01_Test", menuName = "Pre&Post-Test/Test Container")]
public class PrePostTestData : ScriptableObject
{
    public string testID; // เช่น "A01" [cite: 1]

    // ลากไฟล์ ScriptableObject ของคำถามทั้งหมดมาใส่ที่นี่
    public List<BaseTestQuestion> allQuestions;
}