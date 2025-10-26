using UnityEngine;
using System.Collections.Generic; // << 1. (สำคัญ) เพิ่มบรรทัดนี้

public class testLoaddat : MonoBehaviour
{
    // (public List<StoryData> storyData; << ไม่จำเป็น ลบทิ้งได้)

    public void loadStory()
    {
        // (แนะนำ) ตรวจสอบก่อนว่า Instance พร้อมใช้งาน
        if (GameContentDatabase.Instance == null)
        {
            Debug.LogError("GameContentDatabase is not ready!");
            return;
        }

        // เรียกใช้เมธอดที่เราเพิ่งแก้ไข
        List<StoryData> allStories = GameContentDatabase.Instance.GetStoryAll();

        // ตรวจสอบว่าโหลดสำเร็จหรือไม่
        if (allStories == null || allStories.Count == 0)
        {
            Debug.LogWarning("No stories found in the database!");
            return;
        }

        // 2. แก้ไข Debug.Log ให้ถูกต้อง (แสดงจำนวนที่โหลดได้)
        Debug.Log($"Load Success! Found {allStories} stories. Listing them:");

        // 3. วนลูป allStories (ไม่ใช่ storyData)
        foreach (StoryData story in allStories)
        {
            // (แนะนำ) ตรวจสอบว่า story ไม่ null ก่อน
            if (story != null)
            {
                Debug.Log("Story ID: " + story.story_id + ", Story Name: " + story.storyName);
            }
        }
    }
}