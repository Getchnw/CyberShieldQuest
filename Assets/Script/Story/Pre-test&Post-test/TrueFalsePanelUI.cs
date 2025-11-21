using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrueFalsePanelUI : MonoBehaviour
{
    public event System.Action OnAnswerUpdated;
    [SerializeField] private TextMeshProUGUI statementText;
    [SerializeField] private Button trueButton;
    [SerializeField] private Button falseButton;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.5f, 0f); // สีเขียวเมื่อเลือก

    private TrueFalseQuestion questionData;
    private bool? selectedAnswer = null; // null = ยังไม่ตอบ

    // TestController จะเรียกใช้ฟังก์ชันนี้
    public void Setup(TrueFalseQuestion data)
    {
        questionData = data;
        statementText.text = data.statement;

        selectedAnswer = null; // รีเซ็ตคำตอบ
        trueButton.onClick.RemoveAllListeners();
        falseButton.onClick.RemoveAllListeners();

        trueButton.onClick.AddListener(() => OnAnswerSelected(true));
        falseButton.onClick.AddListener(() => OnAnswerSelected(false));
    }

    void OnAnswerSelected(bool answer)
    {
        selectedAnswer = answer;

        // เปลี่ยนสีปุ่มเพื่อให้รู้ว่าเลือกอันไหน
        if (answer == true)
        {
            trueButton.image.color = selectedColor;
            falseButton.image.color = defaultColor;
        }
        else
        {
            trueButton.image.color = defaultColor;
            falseButton.image.color = selectedColor;
        }

        // แจ้งเตือน Controller ทันทีที่เลือก
        OnAnswerUpdated?.Invoke();
    }

    // ฟังก์ชันเช็คว่าตอบหรือยัง (สำหรับเปิดปุ่ม Next)
    public bool IsAnswered()
    {
        return selectedAnswer != null;
    }

    // (Optional) ฟังก์ชันให้ TestController เรียกใช้ตอน "ส่งคำตอบ"
    public bool IsAnswerCorrect()
    {
        if (selectedAnswer == null) return false; // ยังไม่ตอบ
        return selectedAnswer.Value == questionData.isCorrectAnswerTrue;
    }
}