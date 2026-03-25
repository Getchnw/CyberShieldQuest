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

    public Recheck_PopUp recheck_Popup;
    public Button YesButton_Recheck;

    // เอาไว้เก็บPageที่สร้าง
    private List<GameObject> instantiatedPages = new List<GameObject>();
    private int currentPageIndex = 0;

    IEnumerator Start()
    {
        // // 1. ล้างปุ่มเก่า
        // foreach (Transform child in buttonContainer)
        // {
        //     Destroy(child.gameObject);
        // }
        // instantiatedPages.Clear();

        // PopulateChapterList();

        // // รอให้จบเฟรมนี้ก่อน เพื่อให้ GridLayoutGroup มีเวลาทำงานจัดเรียงการ์ด
        // yield return new WaitForEndOfFrame();

        // foreach (GameObject page in instantiatedPages)
        // {
        //     page.SetActive(false);
        // }

        // SetupPagination();

        // รอให้จบเฟรมนี้ก่อน เพื่อให้ GridLayoutGroup มีเวลาทำงานจัดเรียงการ์ด

        // Fix: เรียก RefreshUI() ใน Start() เพื่อให้แน่ใจว่า UI ถูกสร้างและจัดเรียงอย่างถูกต้องตั้งแต่แรก
        yield return new WaitForEndOfFrame();
        RefreshUI();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            // เมื่อได้รับสัญญาณจาก GameManager ให้รันฟังก์ชัน Refresh
            GameManager.Instance.OnPostTestCompleted += RefreshUI;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPostTestCompleted -= RefreshUI;
        }
    }

    void RefreshUI()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        instantiatedPages.Clear();

        PopulateChapterList();

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
        // ดึงด่านต่อสู้ของ Story นี้มาเช็คเงื่อนไขปลดล็อก
        List<StoryStage> storyStages = GameContentDatabase.Instance.GetStoryStagesByStoryID(selectedStoryId);
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
            Transform StarTransform = newButton.transform.Find("Star box");
            Image LockImage = null;
            if (lockTransform != null) LockImage = lockTransform.GetComponent<Image>();
            if (StarTransform != null)
            {
                StarTransform.gameObject.SetActive(false);
            }
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
            // 7. ตั้งค่าปุ่ม และ ดาว
            if (buttonComponent != null)
            {
                if (isUnlocked)
                {
                    buttonComponent.interactable = true;
                    int capturedId = chap.chapter_id; // capture loop variable
                    buttonComponent.onClick.AddListener(() => SelectChapter(capturedId));
                    //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
                    if (LockImage != null) LockImage.gameObject.SetActive(false);
                    if (StarTransform != null)
                    {
                        StarTransform.gameObject.SetActive(true); // เปิดกล่องใส่ดาว

                        // ดึงข้อมูลความคืบหน้าของด่านนี้
                        PlayerChapterProgress currentProgress = chapterProgress.Find(p => p.chapter_id == chap.chapter_id);
                        int starsEarned = 0;

                        if (currentProgress != null)
                        {
                            starsEarned = currentProgress.stars_earned;
                        }

                        // วนลูปเช็คดาวทีละดวง
                        for (int i = 0; i < StarTransform.childCount; i++)
                        {
                            GameObject starObj = StarTransform.GetChild(i).gameObject;

                            // หลักการ:
                            // i = 0 (ดาวดวงที่ 1): เปิดเมื่อ starsEarned >= 1 (หรือ i < starsEarned คือ 0 < 1 เป็นจริง)
                            // i = 1 (ดาวดวงที่ 2): เปิดเมื่อ starsEarned >= 2
                            // i = 2 (ดาวดวงที่ 3): เปิดเมื่อ starsEarned >= 3

                            if (i < starsEarned)
                            {
                                starObj.SetActive(true); // เปิดดาว
                            }
                            else
                            {
                                starObj.SetActive(false); // ปิดดาว
                            }
                        }
                    }
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

        // 8. วนลูปสร้างปุ่มตามจำนวน StoryStage ที่ดึงมาได้ เพื่อเช็คเงื่อนไขปลดล็อก (ต่อจากchapter)
        foreach (var storyStage in storyStages)
        {
            // สร้างปุ่มจาก Prefab แล้วยัดใส่ Box (แบบเดียวกับตอนสร้าง Chapter แต่ไม่ต้องตั้งค่าปุ่ม)
            if (currentPageBox == null || cardsOnCurrentPage >= cardsPerPage)
            {
                currentPageBox = Instantiate(BoxPrefab, buttonContainer);
                instantiatedPages.Add(currentPageBox); // เก็บหน้าไว้ใน List
                cardsOnCurrentPage = 0;
            }

            // ดึงข้อมูลความคืบหน้าของด่านนี้
            var stageProgress = GameManager.Instance.CurrentGameData.stageProgress.Find(sp => sp.stageID == storyStage.stageID);
            bool isStageCompleted = (stageProgress != null && stageProgress.isCompleted);

            // 5. ค้นหาส่วนประกอบใน Prefab ที่เพิ่งสร้าง
            // (GetComponentInChildren จะค้นหา Text, Button ที่อยู่ข้างใน)
            GameObject newButton = Instantiate(chapterButtonPrefab, currentPageBox.transform);
            TextMeshProUGUI Name = newButton.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonComponent = newButton.GetComponent<Button>();
            Image cardImage = newButton.GetComponent<Image>();
            Transform lockTransform = newButton.transform.Find("Lock");
            Transform StarTransform = newButton.transform.Find("Star box");
            Image LockImage = null;
            if (lockTransform != null) LockImage = lockTransform.GetComponent<Image>();
            if (StarTransform != null)
            {
                StarTransform.gameObject.SetActive(false);
            }

            // 2. เตรียมข้อมูล StageData เพื่อส่งให้ StageManager
            // เราจะสร้าง Object ใหม่ขึ้นมาเพื่อถือข้อมูลชั่วคราว
            StageManager.StageData stageData = new StageManager.StageData();
            stageData.stageID = storyStage.stageID;
            stageData.stageName = storyStage.stageName;
            stageData.stageName_th = storyStage.stageName_th;
            stageData.stageButton = newButton.GetComponent<Button>();

            // ค้นหา lockIcon ภายใน Prefab (สมมติว่าชื่อ "Lock")
            // Transform lockTr = newButton.transform.Find("Lock");
            // if (lockTr != null) stageData.lockIcon = lockTr.gameObject;

            stageData.botSprite = storyStage.stageImage;
            stageData.botLevel = storyStage.botLevel;
            stageData.deckDescription = storyStage.deckDescription;
            stageData.deckDescription_th = storyStage.deckDescription_th;
            stageData.starConditions = storyStage.starConditions;
            stageData.requiredChapters = storyStage.requiredChapters;
            stageData.botDecks = storyStage.botDecks;
            stageData.isStoryBattle = storyStage.isStoryBattle;
            stageData.YessButton_Recheck = YesButton_Recheck;

            // // ตั้งค่า flag ว่าเป็นด่านเนื้อเรื่อง เพื่อให้ StageManager ใช้ Logic ที่ถูกต้อง
            // stageData.isStoryBattle = true;


            // 6. ใส่ข้อมูล (สมมติ ChapterData มีตัวแปร 'chapterName')
            if (Name != null)
            {
                Name.text = storyStage.stageName;
            }
            UpdateStoryStageCompleteBadge(newButton, Name, isStageCompleted);
            if (cardImage != null && storyStage.stageImage != null)
            {
                // นำ Sprite จาก Database มาใส่ใน Image component
                cardImage.sprite = storyStage.stageImage;
            }

            bool isUnlocked = false;
            switch (selectedStoryId)
            {
                case "A01":
                    isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A01;
                    break;
                case "A02":
                    isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A02;
                    break;
                case "A03":
                    isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A03;
                    break;

            }
            if (buttonComponent != null)
            {
                if (isUnlocked)
                {
                    Debug.Log($"Stage {storyStage.stageName} is unlocked. Setting up button to open detail popup.");
                    buttonComponent.interactable = true;
                    // buttonComponent.onClick.AddListener();
                    //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
                    if (LockImage != null) LockImage.gameObject.SetActive(false);
                }
                else
                {
                    buttonComponent.interactable = false;
                    if (LockImage != null) LockImage.gameObject.SetActive(true);
                    //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
                }
            }

            // 3. ลงทะเบียนกับ StageManager (ถ้ามีใน Scene)
            if (StageManager.Instance != null)
            {
                StageManager.Instance.RegisterStage(stageData);
            }

            cardsOnCurrentPage++;
            chapterCounter++;
        }

        // --- แก้ไขในส่วนวนลูปสร้าง StoryStage (#8) ---
        // foreach (var storyStage in storyStages)
        // {
        //     // 1. Instantiate ปุ่มปกติของคุณ
        //     if (currentPageBox == null || cardsOnCurrentPage >= cardsPerPage)
        //     {
        //         currentPageBox = Instantiate(BoxPrefab, buttonContainer);
        //         instantiatedPages.Add(currentPageBox);
        //         cardsOnCurrentPage = 0;
        //     }

        //     GameObject newButton = Instantiate(chapterButtonPrefab, currentPageBox.transform);

        //     // 2. เตรียมข้อมูล StageData เพื่อส่งให้ StageManager
        //     // เราจะสร้าง Object ใหม่ขึ้นมาเพื่อถือข้อมูลชั่วคราว
        //     StageManager.StageData stageData = new StageManager.StageData();
        //     stageData.stageID = storyStage.stageID;
        //     stageData.stageName = storyStage.stageName;
        //     stageData.stageName_th = storyStage.stageName_th;
        //     stageData.stageButton = newButton.GetComponent<Button>();

        //     // ค้นหา lockIcon ภายใน Prefab (สมมติว่าชื่อ "Lock")
        //     Transform lockTr = newButton.transform.Find("Lock");
        //     if (lockTr != null) stageData.lockIcon = lockTr.gameObject;

        //     stageData.botSprite = storyStage.stageImage;
        //     stageData.botLevel = storyStage.botLevel;
        //     stageData.deckDescription = storyStage.deckDescription;
        //     stageData.deckDescription_th = storyStage.deckDescription_th;
        //     stageData.starConditions = storyStage.starConditions;
        //     stageData.requiredChapters = storyStage.requiredChapters;
        //     stageData.botDecks = storyStage.botDecks;
        //     stageData.isStoryBattle = storyStage.isStoryBattle;
        //     stageData.YessButton_Recheck = YesButton_Recheck;

        //     // // ตั้งค่า flag ว่าเป็นด่านเนื้อเรื่อง เพื่อให้ StageManager ใช้ Logic ที่ถูกต้อง
        //     // stageData.isStoryBattle = true;

        //     // 3. ลงทะเบียนกับ StageManager (ถ้ามีใน Scene)
        //     if (StageManager.Instance != null)
        //     {
        //         StageManager.Instance.RegisterStage(stageData);
        //     }

        //     TextMeshProUGUI Name = newButton.GetComponentInChildren<TextMeshProUGUI>();
        //     Button buttonComponent = newButton.GetComponent<Button>();
        //     Image cardImage = newButton.GetComponent<Image>();
        //     Transform lockTransform = newButton.transform.Find("Lock");
        //     Transform StarTransform = newButton.transform.Find("Star box");
        //     Image LockImage = null;
        //     if (lockTransform != null) LockImage = lockTransform.GetComponent<Image>();
        //     if (StarTransform != null)
        //     {
        //         StarTransform.gameObject.SetActive(false);
        //     }

        //     // 6. ใส่ข้อมูล (สมมติ ChapterData มีตัวแปร 'chapterName')
        //     if (Name != null)
        //     {
        //         Name.text = storyStage.stageName;
        //     }
        //     if (cardImage != null && storyStage.stageImage != null)
        //     {
        //         // นำ Sprite จาก Database มาใส่ใน Image component
        //         cardImage.sprite = storyStage.stageImage;
        //     }

        //     bool isUnlocked = false;
        //     switch (selectedStoryId)
        //     {
        //         case "A01":
        //             isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A01;
        //             break;
        //         case "A02":
        //             isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A02;
        //             break;
        //         case "A03":
        //             isUnlocked = GameManager.Instance.CurrentGameData.statusPostTest.hasSucessPost_A03;
        //             break;

        //     }
        //     if (buttonComponent != null)
        //     {
        //         if (isUnlocked)
        //         {
        //             Debug.Log($"Stage {storyStage.stageName} is unlocked. Setting up button to open detail popup.");
        //             buttonComponent.interactable = true;
        //             // buttonComponent.onClick.AddListener();
        //             //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
        //             if (LockImage != null) LockImage.gameObject.SetActive(false);
        //         }
        //         else
        //         {
        //             buttonComponent.interactable = false;
        //             if (LockImage != null) LockImage.gameObject.SetActive(true);
        //             //if (comingSoonTransform != null) comingSoonTransform.gameObject.SetActive(false);
        //         }
        //     }

        //     cardsOnCurrentPage++;
        //     chapterCounter++;
        // }
    }

    private void UpdateStoryStageCompleteBadge(GameObject stageButtonObject, TextMeshProUGUI sourceNameText, bool isCompleted)
    {
        if (stageButtonObject == null) return;

        const string badgeName = "CompleteBadge";
        Transform badgeTransform = stageButtonObject.transform.Find(badgeName);
        GameObject badgeObject;
        TextMeshProUGUI badgeText;

        if (badgeTransform == null)
        {
            badgeObject = new GameObject(badgeName, typeof(RectTransform));
            badgeObject.transform.SetParent(stageButtonObject.transform, false);

            RectTransform rect = badgeObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-22f, -14f);
            rect.sizeDelta = new Vector2(250f, 56f);

            badgeText = badgeObject.AddComponent<TextMeshProUGUI>();
            badgeText.alignment = TextAlignmentOptions.TopRight;
            badgeText.text = "COMPLETE";
            badgeText.raycastTarget = false;
            badgeText.color = new Color32(53, 255, 141, 255);
        }
        else
        {
            badgeObject = badgeTransform.gameObject;
            badgeText = badgeObject.GetComponent<TextMeshProUGUI>();

            if (badgeText == null)
            {
                badgeText = badgeObject.AddComponent<TextMeshProUGUI>();
                badgeText.alignment = TextAlignmentOptions.TopRight;
                badgeText.raycastTarget = false;
                badgeText.color = new Color32(53, 255, 141, 255);
            }
        }

        if (badgeText != null)
        {
            if (sourceNameText != null && sourceNameText.font != null)
            {
                badgeText.font = sourceNameText.font;
            }

            badgeText.fontSize = sourceNameText != null ? Mathf.Max(24f, sourceNameText.fontSize * 0.85f) : 28f;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.text = "COMPLETE";
        }

        badgeObject.SetActive(isCompleted);
    }

    void SetupPagination()
    {
        currentPageIndex = 0;

        // ถ้ามีหน้าเดียว (การ์ด 1-4 ใบ)
        if (instantiatedPages.Count <= 1)
        {
            if (buttonNext != null) buttonNext.gameObject.SetActive(false);
            if (buttonPrev != null) buttonPrev.gameObject.SetActive(false);
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
