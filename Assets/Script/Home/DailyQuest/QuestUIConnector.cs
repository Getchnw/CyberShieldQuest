using UnityEngine;
using TMPro;

public class QuestUIConnector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText; // ลาก Text เงินมาใส่ (ถ้ามี)

    // ใช้ OnEnable แทน Start เพื่อให้ทำงานทุกครั้งที่ "เปิดหน้าต่าง"
    private void OnEnable()
    {
        if (DailyQuestManager.Instance != null)
        {
            Debug.Log("📢 UI ตื่นแล้ว! กำลังลงทะเบียนกับ Manager...");
            // ส่งตัวเอง (Transform) ไปให้ Manager รู้จัก
            DailyQuestManager.Instance.RegisterUI(this.transform, goldText);
        }
    }
}