using UnityEngine;
using UnityEngine.UI;

public class StoryScroller : MonoBehaviour
{
[Header("Components")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Scroll Settings")]
    [Tooltip("จำนวน Story ทั้งหมดที่มี")]
    [SerializeField] private int totalStoryItems;
    
    [Tooltip("ความเร็วในการเลื่อน (ยิ่งมากยิ่งเร็ว)")]
    [SerializeField] private float scrollSpeed = 50f;

    private int currentItemIndex = 0; // Index ของ Story ที่กำลังแสดง
    private float targetNormalizedPos = 0f; // ตำแหน่งเป้าหมาย (0.0 คือซ้ายสุด, 1.0 คือขวาสุด)
    private bool isScrolling = false; // สถานะว่ากำลังเลื่อนอยู่หรือไม่

    void Start()
    {
        // ปิดการลากด้วยเมาส์หรือนิ้ว (ถ้าอยากให้ควบคุมด้วยปุ่มเท่านั้น)
        scrollRect.inertia = false; 
        
        // อัปเดตสถานะปุ่มตอนเริ่ม
        UpdateArrowButtons();
    }

    void Update()
    {
        // ถ้ากำลังเลื่อน ให้ค่อยๆ Lerp (เคลื่อนที่อย่างนุ่มนวล) ไปยังเป้าหมาย
        if (isScrolling)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition, 
                targetNormalizedPos, 
                Time.deltaTime * scrollSpeed
            );

            // ถ้าเลื่อนไปใกล้เป้าหมายมากพอแล้ว ให้หยุด
            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetNormalizedPos) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = targetNormalizedPos;
                isScrolling = false;
            }
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกโดยปุ่ม 'ขวา'
    public void ScrollRight()
    {
        if (currentItemIndex < totalStoryItems - 1)
        {
            currentItemIndex++;
            UpdateTargetPosition();
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกโดยปุ่ม 'ซ้าย'
    public void ScrollLeft()
    {
        if (currentItemIndex > 0)
        {
            currentItemIndex--;
            UpdateTargetPosition();
        }
    }

    // คำนวณตำแหน่งเป้าหมายใหม่
    private void UpdateTargetPosition()
    {
        // คำนวณตำแหน่ง normalized position
        // (มี totalStoryItems - 1 "ขั้น" ในการเลื่อน)
        targetNormalizedPos = (float)currentItemIndex / (float)(totalStoryItems - 1);
        isScrolling = true;
        
        // อัปเดตสถานะปุ่ม
        UpdateArrowButtons();
    }

    // เปิด/ปิด ปุ่มซ้ายขวา ตามความเหมาะสม
    private void UpdateArrowButtons()
    {
        if (leftButton != null)
        {
            // ถ้าอยู่ซ้ายสุด (index 0) ให้ปิดปุ่มซ้าย
            leftButton.interactable = (currentItemIndex > 0);
        }

        if (rightButton != null)
        {
            // ถ้าอยู่ขวาสุด ให้ปิดปุ่มขวา
            rightButton.interactable = (currentItemIndex < totalStoryItems - 1);
        }
    }

    public void LoadSence (string Namescene) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Namescene);
    }

    

}
