using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreDetailRowUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI answerText;
    //[SerializeField] private Image statusIcon; // (Optional) รูปถูก/ผิด
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    // ฟังก์ชันสำหรับใส่ข้อมูล
    public void Setup(string question, string answer, int score)
    {
        // 1. ป้องกัน Error กรณีลืมลาก Text มาใส่ใน Inspector
        if (questionText == null || answerText == null) return;

        // 2. ใส่ข้อความ (ตัดคำว่า Matching: ออก)
        // ใช้ string.Empty แทน "" เพื่อความสวยงาม (Optional)
        questionText.text = question.Replace("Matching: ", string.Empty);
        answerText.text = answer;

        // 3. เปลี่ยนสีตามคะแนน
        bool isCorrect = score > 0;
        answerText.color = isCorrect ? correctColor : wrongColor;
    }
}