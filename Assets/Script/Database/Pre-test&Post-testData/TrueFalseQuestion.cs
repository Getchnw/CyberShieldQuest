using UnityEngine;

[CreateAssetMenu(fileName = "TF_NewQuestion", menuName = "Pre&Post-Test/True-False Question")]
public class TrueFalseQuestion : BaseTestQuestion
{
    [TextArea(4, 6)]
    public string statement; // "8. ภัยคุกคาม IDOR ที่ร้ายแรงที่สุด..." 
    public bool isCorrectAnswerTrue; // (ติ๊กถูกถ้าคำตอบคือ "ถูกต้อง") [cite: 49]
    public StoryData story;
}