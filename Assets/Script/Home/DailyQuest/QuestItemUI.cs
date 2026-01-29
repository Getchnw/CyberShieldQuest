using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image iconImage;

    private PlayerQuestData mySaveData;
    private DailyQuestsData myStaticData;

    // ฟังก์ชันนี้ Manager จะเป็นคนเรียก
    public void Setup(PlayerQuestData saveData, DailyQuestsData staticData)
    {
        mySaveData = saveData;
        myStaticData = staticData;
        RefreshUI();
    }

    public void RefreshUI()
    {
        // 1. รีเซ็ตข้อความให้เป็นค่าตั้งต้นเสมอก่อนเริ่ม Logic
        descText.text = myStaticData.description;
        iconImage.sprite = myStaticData.icon;

        // 2. อัปเดต Slider
        progressSlider.maxValue = myStaticData.targetAmount;
        progressSlider.value = mySaveData.currentAmount;

        // 3. แสดงข้อความ Progress
        progressText.text = $"{mySaveData.currentAmount}/{myStaticData.targetAmount}";

        // 4. เช็คเงื่อนไข
        bool isComplete = mySaveData.currentAmount >= myStaticData.targetAmount;

        // 5. จัดการปุ่มและสถานะ
        if (mySaveData.isClaimed)
        {
            // กรณีรับไปแล้ว
            claimButton.interactable = false;
            claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claimed"; // เปลี่ยนข้อความปุ่ม
            // descText.text += " <color=green>(สำเร็จ)</color>"; // ถ้ายังอยากใส่ต่อท้าย ให้ใส่วงเล็บนี้
        }
        else
        {
            // กรณีขอยังไม่รับ
            claimButton.interactable = isComplete; // กดได้เมื่อทำครบ
            claimButton.GetComponentInChildren<TextMeshProUGUI>().text = isComplete ? "Claim" : "Locked";
            claimButton.onClick.AddListener(OnClaimClicked);
        }
    }

    public void OnClaimClicked()
    {
        // เรียก Manager ให้แจกรางวัล
        DailyQuestManager.Instance.ClaimReward(mySaveData.questID);
        RefreshUI();
    }
}