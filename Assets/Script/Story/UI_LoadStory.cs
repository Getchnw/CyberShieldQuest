using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_LoadStory : MonoBehaviour
{
// (ใหม่!) ลาก GameObject "Story1" ถึง "Story10" (ที่มี Script UI_StoryItem)
    [SerializeField] private List<UI_StoryItem> storyItemSlots;
    
    private List<StoryData> allStoryData;

    void Start()
    {
        // Load Data
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

        // วนลูปตามจำนวน "ช่อง UI" ที่เรามี
        for (int i = 0; i < storyItemSlots.Count; i++)
        {
            // เช็คว่ามี "ข้อมูล" พอสำหรับ "ช่อง" นี้หรือไม่
            if (i < allStoryData.Count)
            {
                // ถ้ามี:
                // 1. ดึง "ช่อง UI" (Story1, Story2, ...)
                UI_StoryItem uiSlot = storyItemSlots[i];
                
                // 2. ดึง "ข้อมูล" ที่ตรงกัน
                StoryData data = allStoryData[i];

                // 3. สั่งให้ "ช่อง UI" อัปเดตตัวเอง โดยส่ง "ข้อมูล" และ "ฟังก์ชัน" ไปให้
                uiSlot.gameObject.SetActive(true); // เปิดช่องนี้
                uiSlot.Setup(data, SelectStory); // << นี่คือจุดที่เชื่อมทุกอย่างเข้าด้วยกัน
            }
            else
            {
                // ถ้าไม่มีข้อมูลสำหรับช่องนี้ (เช่น มี 10 ช่อง แต่มีข้อมูลแค่ 8)
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
