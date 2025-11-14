using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_StoryItem : MonoBehaviour
{
    [Header("UI Components")]
    // ลาก "Image" ที่ใช้แสดง Artwork มาใส่
    [SerializeField] private Image artworkImage;
    
    // ลาก "TextMeshPro" ที่ชื่อ "Name" มาใส่
    [SerializeField] private TextMeshProUGUI nameText;
    
    // ลาก "Button" ที่ครอบการ์ด (ปุ่มโปร่งใส) มาใส่
    [SerializeField] private Button storyButton; 
    
    // ลาก "GameObject" ที่เป็น Text/Panel "Coming Soon" มาใส่
    [SerializeField] private GameObject comingSoonOverlay; 

    [SerializeField] private Image LockImage;
    
    /// นี่คือเมธอดหลักที่ UI_LoadStory จะเรียกใช้
    public void Setup(StoryData data, bool  isProgressUnlocked, System.Action<string> onSelectAction)
    {
        // --- 1. ใส่ข้อมูลลง UI ---
        artworkImage.sprite = data.artwork; 
        nameText.text = data.storyName;
        bool finalIsUnlocked = isProgressUnlocked && (data.storyStatus != StoryData.Status.Comingsoon);

        if (finalIsUnlocked)
        {
            // --- แบบ Ongoing (เล่นได้) ---
            comingSoonOverlay.SetActive(false); //
            storyButton.interactable = true;   //
            LockImage.gameObject.SetActive(false);

            // --- 3. ตั้งค่าปุ่ม ---
            storyButton.onClick.RemoveAllListeners(); //
            
            // เมื่อกดปุ่ม ให้เรียกฟังก์ชัน onSelectAction (ก็คือ SelectStory)
            // และส่ง story_id (string) กลับไป
            storyButton.onClick.AddListener(() => onSelectAction(data.story_id)); //
        }
        else
        {
            // --- แบบ Coming Soon หรือ ยังไม่ปลดล็อก ---
            comingSoonOverlay.SetActive(true); //
            storyButton.interactable = false;  //
        }
    }
}