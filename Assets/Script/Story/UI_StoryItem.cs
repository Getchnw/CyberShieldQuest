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

    /// <summary>
    /// นี่คือเมธอดหลักที่ UI_LoadStory จะเรียกใช้
    /// </summary>
    /// <param name="data">ข้อมูล Story ที่ดึงมาจาก Database</param>
    /// <param name="onSelectAction">ฟังก์ชันที่จะให้ปุ่มนี้เรียกใช้ (คือ SelectStory)</param>
    public void Setup(StoryData data, System.Action<string> onSelectAction)
    {
        // --- 1. ใส่ข้อมูลลง UI ---
        
        // (ผมสมมติว่า StoryData ของคุณมีตัวแปรชื่อ artworkSprite และ storyName)
        artworkImage.sprite = data.artwork; 
        nameText.text = data.storyName; // นี่จะใส่ "A01 Broken Access Control"

        // --- 2. ตรวจสอบสถานะของ Story ---
        
        // (ผมสมมติว่า StoryData ของคุณมีตัวแปร bool ชื่อ isComingSoon)
        if (data.storyStatus == StoryData.Status.Comingsoon)
        {
            // --- แบบ Coming Soon ---
            comingSoonOverlay.SetActive(true); // เปิดป้าย "Coming Soon"
            storyButton.interactable = false;  // ปิดปุ่ม
        }
        else
        {
            // --- แบบ Ongoing (เล่นได้) ---
            comingSoonOverlay.SetActive(false); // ปิดป้าย "Coming Soon"
            storyButton.interactable = true;   // เปิดปุ่ม

            // --- 3. ตั้งค่าปุ่ม ---
            storyButton.onClick.RemoveAllListeners(); // ล้างของเก่ากันพลาด
            
            // เมื่อกดปุ่ม ให้เรียกฟังก์ชัน onSelectAction (ก็คือ SelectStory)
            // และส่ง story_id (string) กลับไป
            storyButton.onClick.AddListener(() => onSelectAction(data.story_id));
        }
    }
}