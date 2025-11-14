using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_LoadStory : MonoBehaviour
{
    //ลาก GameObject "Story1" ถึง "Story10"
    [SerializeField] private List<UI_StoryItem> storyItemSlots;
    
    private List<StoryData> allStoryData;

    void Start()
    {
        if (GameContentDatabase.Instance == null)
        {
            Debug.LogError("GameContentDatabase not found!");
            return;
        }
        allStoryData = GameContentDatabase.Instance.GetStoryAll();
        
        // เรียกใช้ฟังก์ชันสร้าง UI
        PopulateStoryUI();
    }

    // แก้ไขชื่อและโค้ดข้างใน
    public void PopulateStoryUI()
    {
        if (allStoryData == null)
        {
            Debug.LogWarning("No Story data loaded.");
            return;
        }
        //ดึงProgress
        var allChapterProgress = GameManager.Instance.CurrentGameData.chapterProgress ?? new List<PlayerChapterProgress>();
        // วนลูปตามจำนวน "ช่อง UI" ที่เรามี
        for (int i = 0; i < storyItemSlots.Count; i++)
        {
            // เช็คว่ามี "ข้อมูล" พอสำหรับ "ช่อง" นี้หรือไม่
            if (i < allStoryData.Count)
            {
                // 1. ดึง "ช่อง UI"
                UI_StoryItem uiSlot = storyItemSlots[i];
                
                // 2. ดึง "ข้อมูล"
                StoryData data = allStoryData[i];
                
                //ฟังก์ชัน เช็คstatus lock/unlock
                bool isProgressUnlocked = false;
                if (i == 0)
                {
                    // Story แรก ปลดล็อกเสมอ
                    isProgressUnlocked = true;
                }
                else
                {
                    // 1. หา Story ก่อนหน้า
                    StoryData previousStory = allStoryData[i - 1];
                    
                    // 2. หา Chapter ทั้งหมดของ Story ก่อนหน้า
                    List<ChapterData> previousStoryChapters = GameContentDatabase.Instance.GetChaptersByStoryID(previousStory.story_id);
                    
                    if (previousStoryChapters != null && previousStoryChapters.Count > 0)
                    {
                        // 3. หา Chapter "สุดท้าย" ของ Story ก่อนหน้า
                        ChapterData lastChapterOfPreviousStory = previousStoryChapters[previousStoryChapters.Count - 1];
                        
                        // 4. ตรวจสอบ Progress ของ Chapter สุดท้ายนั้น
                        PlayerChapterProgress previousProgress = allChapterProgress.Find(
                            p => p.chapter_id == lastChapterOfPreviousStory.chapter_id);
                        
                        // 5. ถ้า Chapter สุดท้ายนั้น "is_completed" = ปลดล็อก
                        if (previousProgress != null && previousProgress.is_completed)
                        {
                            isProgressUnlocked = true;
                        }
                        // ยังเรียนไม่เสร็จก็เป็นfalse
                    }
                }
                // 3. สั่งให้ "ช่อง UI" อัปเดตตัวเอง โดยส่ง "ข้อมูล" และ "ฟังก์ชัน" ไปให้
                uiSlot.gameObject.SetActive(true); // เปิดช่องนี้
                uiSlot.Setup(data, isProgressUnlocked, SelectStory);                
            }
            else
            {
                // ถ้าไม่มีข้อมูลสำหรับช่องนี้
                // ให้ปิดช่อง UI ที่เหลือทิ้งไป
                storyItemSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // เมธอดนี้จะถูกเรียกโดย UI_StoryItem เมื่อปุ่มถูกกด
    public void SelectStory(string storyId)
    {
        Debug.Log($"Player selected Story ID: {storyId}");

        // บันทึก Story ID ที่ผู้เล่นเลือก
        GameManager.Instance.SaveSelectedStory(storyId); 
        
        // (โค้ดไปหน้าเลือก Chapter ต่อไป...)
        LoadScene("Template_select_chapter_story");
    }

    // (แก้ชื่อเมธอดจาก LoadSence และเพิ่ม .SceneManagement)
    public void LoadScene (string namescene) 
    {
        SceneManager.LoadScene(namescene);
    }
}
