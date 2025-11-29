using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileLevelSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image expFillImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    void Start()
    {
        // 1. อัปเดตหน้าจอทันทีตอนเริ่มเกม โดยดึงข้อมูลจาก GameManager
        UpdateUI_FromManager();

        // 2. สมัครรับข้อมูล (Subscribe) ถ้าค่าเปลี่ยน ให้หน้าจอเปลี่ยนตามอัตโนมัติ
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnExperienceChanged += OnExpChanged;
            GameManager.Instance.OnLevelChanged += OnLevelChanged;
        }
    }

    void OnDestroy()
    {
        // ยกเลิกการสมัครเมื่อ Object นี้หายไป (ป้องกัน Error)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnExperienceChanged -= OnExpChanged;
            GameManager.Instance.OnLevelChanged -= OnLevelChanged;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกอัตโนมัติเมื่อ EXP เปลี่ยน
    void OnExpChanged(int newExp)
    {
        UpdateUI_FromManager();
    }

    // ฟังก์ชันนี้จะถูกเรียกอัตโนมัติเมื่อ Level เปลี่ยน
    void OnLevelChanged(int newLevel)
    {
        UpdateUI_FromManager();
        // (Optional) ใส่ Effect พลุ หรือเสียง Level Up ตรงนี้ได้เลย
    }

    void UpdateUI_FromManager()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null) return;

        // ดึงข้อมูลล่าสุด
        int currentLevel = GameManager.Instance.CurrentGameData.profile.level;
        int currentExp = GameManager.Instance.CurrentGameData.profile.experience;

        // ให้ GameManager ช่วยคำนวณ Max Exp ให้ (ตามกราฟ Curve)
        int maxExp = GameManager.Instance.GetMaxExpForLevel(currentLevel);

        // คำนวณหลอด
        if (maxExp > 0)
            expFillImage.fillAmount = (float)currentExp / maxExp;
        else
            expFillImage.fillAmount = 0;

        // อัปเดตข้อความ
        if (levelText != null) levelText.text = "LV." + currentLevel;
        if (expText != null) expText.text = $"{currentExp} / {maxExp}";
    }
}