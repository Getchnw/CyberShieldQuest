using UnityEngine;
using System.Collections.Generic;
using TMPro;                       
using UnityEngine.UI;                
using UnityEngine.SceneManagement; 

public class UI_ChapterSelect : MonoBehaviour
{
    [Header("Content GameObject")]
    [SerializeField] private Transform buttonContainer;  
    
    [Header("Chapter Card Prefab")]
    [SerializeField] private GameObject chapterButtonPrefab; 

    void Start()
    {
        //ล้างปุ่มเก่าที่อาจค้างอยู่
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // เริ่มสร้าง UI
        PopulateChapterList();
    }

    void PopulateChapterList()
    {
        // 1. "ถาม" GameManager ว่าเราเลือก Story ไหนมา
        string selectedStoryId = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        Debug.Log($"Loading Chapters for Story ID: {selectedStoryId}");
        // 2. "ถาม" Database ว่า Story ID นี้ มี Chapter อะไรบ้าง
        List<ChapterData> chapters = GameContentDatabase.Instance.GetChaptersByStoryID(selectedStoryId);
        Debug.Log($"Found {chapters.Count} chapters for Story ID: {selectedStoryId}");
        if (chapters == null || chapters.Count == 0)
        {
            Debug.LogWarning($"ไม่พบ Chapter สำหรับ Story ID: {selectedStoryId}");
            return;
        }

        // 3. วนลูปสร้างปุ่มตามจำนวน Chapter ที่ดึงมาได้
        foreach (ChapterData chap in chapters)
        {
            // 4. สร้างปุ่มจาก Prefab แล้วยัดใส่ Container
            GameObject newButton = Instantiate(chapterButtonPrefab, buttonContainer);
            
            // 5. ค้นหาส่วนประกอบใน Prefab ที่เพิ่งสร้าง
            // (GetComponentInChildren จะค้นหา Text, Button ที่อยู่ข้างใน)
            TextMeshProUGUI Name = newButton.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonComponent = newButton.GetComponentInChildren<Button>();
            Transform comingSoonTransform = newButton.transform.Find("ComingSoon");


            // 6. ใส่ข้อมูล (สมมติ ChapterData มีตัวแปร 'chapterName')
            if (Name != null)
            {
                Name.text = chap.chapterName; 
            }

            if (comingSoonTransform != null)
            {
                comingSoonTransform.gameObject.SetActive(false); // สั่งซ่อน GameObject นี้
            }
            // 7. ตั้งค่าปุ่ม
            if (buttonComponent != null)
            {
                Debug.Log("interactable chapter button created.");
                buttonComponent.interactable = true;
                // สั่งให้ปุ่มนี้ เมื่อถูกกด ให้เรียก SelectChapter
                buttonComponent.onClick.AddListener(() => SelectChapter(chap.chapter_id));
            }
        }
    }

    void SelectChapter(int chapterId)
    {
        Debug.Log($"เลือก Chapter ID: {chapterId}");
        // (คุณอาจจะฝาก Chapter ID ไว้กับ GameManager อีกที)
        GameManager.Instance.SaveSelectedChapter(chapterId); 
        
        // (แล้วโหลด Scene ถัดไป เช่น Scene เนื้อเรื่อง หรือ Scene ต่อสู้)
        SceneManager.LoadScene("Template_StoryScene");
    }
}
