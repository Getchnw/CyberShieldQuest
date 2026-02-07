using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimButton;
    // [SerializeField] private Image LockImage;
    [SerializeField] private Image IconRewardImage;
    [SerializeField] private TextMeshProUGUI amountRewardText;
    [SerializeField] private Image checkImage;

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
        // Gold
        IconRewardImage.sprite = myStaticData.icon;
        amountRewardText.text = $"{myStaticData.rewardGold} G";

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
            claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "<color=green>Claimed</color>"; // เปลี่ยนข้อความปุ่ม

            // ปิดรูป
            checkImage.gameObject.SetActive(true);
            // descText.text += " <color=green>(สำเร็จ)</color>"; // ถ้ายังอยากใส่ต่อท้าย ให้ใส่วงเล็บนี้
        }
        else if (isComplete)
        {
            // กรณีทำครบแล้ว แต่ยังไม่รับ
            claimButton.interactable = isComplete; // กดได้เมื่อทำครบ
            claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "Claim";
            claimButton.onClick.AddListener(OnClaimClicked);
        }
        else
        {
            // กรณียังไม่ครบ
            claimButton.GetComponentInChildren<TextMeshProUGUI>().text = "Go";
            claimButton.onClick.AddListener(() => GotoQuest(myStaticData.targetScene));
        }
    }

    public void OnClaimClicked()
    {
        // เรียก Manager ให้แจกรางวัล
        DailyQuestManager.Instance.ClaimReward(mySaveData.questID);
        RefreshUI();
    }

    public void GotoQuest(string Namescene)
    {
        SceneManager.LoadScene(Namescene);
    }
}