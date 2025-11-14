using UnityEngine;
using System.Collections.Generic;
using TMPro;                       
using UnityEngine.UI;                
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_ChapterSelect : MonoBehaviour
{
    [Header("Content GameObject")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject BoxPrefab;
    
    [Header("Chapter Card Prefab")]
    [SerializeField] private GameObject chapterButtonPrefab; 

    [Header("Navigation Buttons")]
    [SerializeField] private Button buttonNext; // 3. ลากปุ่ม "ถัดไป" มาใส่
    [SerializeField] private Button buttonPrev; 

    // เอาไว้เก็บPageที่สร้าง
    private List<GameObject> instantiatedPages = new List<GameObject>();
    private int currentPageIndex = 0;

    // private void OnEnable()
    // {
    //     // Subscribe to data changes so we refresh chapter locks when returning from quiz
    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.OnDataLoaded += RefreshChapterLocks;
    //         Debug.Log("Subscribed to OnDataLoaded");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("GameManager.Instance is null in OnEnable");
    //     }
    // }

    // private void OnDisable()
    // {
    //     // Clean up subscription
    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.OnDataLoaded -= RefreshChapterLocks;
    //         Debug.Log("Unsubscribed from OnDataLoaded");
    //     }
    // }

    /// <summary>
    /// Refreshes the lock/unlock status of all chapter buttons based on current progress
    /// This is called when returning from a quiz so locks update immediately
    /// </summary>
    // private void RefreshChapterLocks()
    // {
        
    //     if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null) 
    //     {
    //         return;
    //     }
    //     var chapterProgress = GameManager.Instance.CurrentGameData.chapterProgress ?? new List<PlayerChapterProgress>();
    //     var chapters = GameContentDatabase.Instance.GetChaptersByStoryID(
    //         GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);

    //     if (chapters == null || chapters.Count == 0) 
    //     {
    //         Debug.LogWarning("⚠️ No chapters found");
    //         return;
    //     }

    //     Debug.Log($"✅ Updating lock status for {chapters.Count} chapters");

    //     // Update each chapter button's lock status based on previous chapter completion
    //     int buttonCount = 0;
    //     foreach (GameObject page in instantiatedPages)
    //     {
    //         Button[] buttonsInPage = page.GetComponentsInChildren<Button>();
    //         foreach (Button btn in buttonsInPage)
    //         {
    //             if (buttonCount >= chapters.Count) break;

    //             bool isUnlocked = false;
    //             Transform lockTransform = btn.transform.Find("Lock");
    //             Image LockImage = null;
    //             if (lockTransform != null) LockImage = lockTransform.GetComponent<Image>();
    //             if (buttonCount == 0)
    //             {
    //                 isUnlocked = true;
    //                 LockImage.gameObject.SetActive(!isUnlocked);
    //             }
    //             else
    //             {
    //                 ChapterData previousChapter = chapters[buttonCount - 1];
    //                 PlayerChapterProgress previousProgress = chapterProgress.Find(
    //                     p => p.chapter_id == previousChapter.chapter_id);
    //                 if (previousProgress != null && previousProgress.is_completed)
    //                 {
    //                     isUnlocked = true;
    //                     LockImage.gameObject.SetActive(!isUnlocked);
    //                 }
    //             }

    //             btn.interactable = isUnlocked;
    //             // Transform comingSoon = btn.transform.Find("ComingSoon");
    //             // if (comingSoon != null)
    //             //     comingSoon.gameObject.SetActive(!isUnlocked);

    //             Debug.Log($"  Chapter {buttonCount}: isUnlocked={isUnlocked}");
    //             buttonCount++;
    //         }
    //     }
    // }

    IEnumerator Start()
    {
        // 1. ล้างปุ่มเก่า
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        instantiatedPages.Clear();

        PopulateChapterList();

        // รอให้จบเฟรมนี้ก่อน เพื่อให้ GridLayoutGroup มีเวลาทำงานจัดเรียงการ์ด
        yield return new WaitForEndOfFrame(); 

        foreach (GameObject page in instantiatedPages)
        {
            page.SetActive(false);
        }

        SetupPagination();
    }

    void PopulateChapterList()
    {
        // 1. "ถาม" GameManager ว่าเราเลือก Story ไหนมา
        string selectedStoryId = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        // 2. "ถาม" Database ว่า Story ID นี้ มี Chapter อะไรบ้าง
        List<ChapterData> chapters = GameContentDatabase.Instance.GetChaptersByStoryID(selectedStoryId);        
        var chapterProgress = GameManager.Instance.CurrentGameData.chapterProgress ?? new List<PlayerChapterProgress>();
        if (chapters == null || chapters.Count == 0)
        {
            Debug.LogWarning($"ไม่พบ Chapter สำหรับ Story ID: {selectedStoryId}");
            return;
        }

        GameObject currentPageBox = null;
        int cardsOnCurrentPage = 0;
        int chapterCounter = 0;
        const int cardsPerPage = 4;
        // 3. วนลูปสร้างปุ่มตามจำนวน Chapter ที่ดึงมาได้
        foreach (ChapterData chap in chapters)
        {
            // 4. สร้างปุ่มจาก Prefab แล้วยัดใส่ Box
            if (currentPageBox == null || cardsOnCurrentPage >= cardsPerPage)
            {
                currentPageBox = Instantiate(BoxPrefab, buttonContainer);
                instantiatedPages.Add(currentPageBox); // เก็บหน้าไว้ใน List
                cardsOnCurrentPage = 0;
            }

            // 5. ค้นหาส่วนประกอบใน Prefab ที่เพิ่งสร้าง
            // (GetComponentInChildren จะค้นหา Text, Button ที่อยู่ข้างใน)
            GameObject newButton = Instantiate(chapterButtonPrefab, currentPageBox.transform);
            TextMeshProUGUI Name = newButton.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonComponent = newButton.GetComponent<Button>(); 
            Image cardImage = newButton.GetComponent<Image>();
            Transform lockTransform = newButton.transform.Find("Lock");
            Image LockImage = null;
            if (lockTransform != null) LockImage = lockTransform.GetComponent<Image>();
            // Transform comingSoonTransform = newButton.transform.Find("ComingSoon");


            // 6. ใส่ข้อมูล (สมมติ ChapterData มีตัวแปร 'chapterName')
            if (Name != null)
            {
                Name.text = chap.chapterName;
            }
            if (cardImage != null && chap.chapterImage != null)
            {
                // นำ Sprite จาก Database มาใส่ใน Image component
                cardImage.sprite = chap.chapterImage;
            }
            //กำหนดค่า สำหรับปลดล็อกด่าน
            bool isUnlocked = false;
            int currentIndex = chapterCounter;

            if (currentIndex == 0)
            {
                isUnlocked = true;
            }
            else
            {
                ChapterData previousChapter = chapters[currentIndex - 1];
                PlayerChapterProgress previousProgress = chapterProgress.Find(
                    p => p.chapter_id == previousChapter.chapter_id);
                // previousProgress uses snake_case fields (is_completed)
                if (previousProgress != null && previousProgress.is_completed) isUnlocked = true;
            }
            // 7. ตั้งค่าปุ่ม
            if (buttonComponent != null)
            {
                if (isUnlocked)
                {
                    buttonComponent.interactable = true;
                    int capturedId = chap.chapter_id; // capture loop variable
                    buttonComponent.onClick.AddListener(() => SelectChapter(capturedId));
                    //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
                    if (LockImage != null) LockImage.gameObject.SetActive(false);
                }
                else
                {
                    buttonComponent.interactable = false;
                    //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
                }
            }

            cardsOnCurrentPage++;
            chapterCounter++;
        }
    }

    void SetupPagination()
    {
        currentPageIndex = 0;

        // ถ้ามีหน้าเดียว (การ์ด 1-4 ใบ)
        if (instantiatedPages.Count <= 1)
        {
            if(buttonNext != null) buttonNext.gameObject.SetActive(false);
            if(buttonPrev != null) buttonPrev.gameObject.SetActive(false);
        }
        
        // แสดงเฉพาะหน้าแรก
        if (instantiatedPages.Count > 0)
        {
            instantiatedPages[0].SetActive(true);
        }

        // อัปเดตสถานะปุ่ม
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        if (buttonPrev != null)
        {
            // ถ้าอยู่หน้าแรกให้ซ่อน/disable
            buttonPrev.interactable = (currentPageIndex > 0);
            bool hasPrev = (currentPageIndex > 0);
            buttonPrev.interactable = hasPrev;
            //buttonPrev.gameObject.SetActive(currentPageIndex > 0);
        }
        if (buttonNext != null)
        {
            // ถ้าอยู่หน้าสุดท้าย ให้ซ่อน/disable
            bool hasNext = (currentPageIndex < instantiatedPages.Count - 1);
            buttonNext.interactable = hasNext;
            buttonNext.gameObject.SetActive(hasNext);
        }
    }

    public void NextPage()
    {
        if (currentPageIndex < instantiatedPages.Count - 1)
        {
            // ซ่อนหน้าปัจจุบัน
            instantiatedPages[currentPageIndex].SetActive(false);
            // เลื่อนไปหน้าถัดไป
            currentPageIndex++;
            // แสดงหน้าใหม่
            instantiatedPages[currentPageIndex].SetActive(true);
            
            UpdateNavigationButtons(); // อัปเดตสถานะปุ่ม
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            // ซ่อนหน้าปัจจุบัน
            instantiatedPages[currentPageIndex].SetActive(false);
            // ย้อนกลับ
            currentPageIndex--;
            // แสดงหน้าใหม่
            instantiatedPages[currentPageIndex].SetActive(true);
            
            UpdateNavigationButtons(); // อัปเดตสถานะปุ่ม
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
