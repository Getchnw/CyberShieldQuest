using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GuidedLineController : MonoBehaviour
{
    // 1. กำหนดประเภทของไกด์ไลน์ (เพิ่มเผื่อไว้ให้ครบ)
    public enum TutorialType
    {
        Home,
        Story,
        Stage,
        Deck,
        Shop
    }

    [Header("Settings")]
    [Tooltip("เลือกประเภท: หน้าแรกเลือก Home / ปุ่ม Story เลือก Story")]
    [SerializeField] private TutorialType tutorialType = TutorialType.Home;

    // [Tooltip("ชื่อ Scene ที่จะไปต่อ (สำหรับ Story/Stage/Deck/Shop)")]
    // [SerializeField] private string nextSceneName;

    [Header("UI Objects (ห้ามลืมลากใส่!)")]
    [SerializeField] private GameObject tutorialPanel; // ตัวแม่ของหน้าต่างสอน
    [SerializeField] private List<GameObject> tutorialPages; // รูปภาพแต่ละหน้า

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Text Data Thai")]
    [TextArea(1, 3)][SerializeField] private string[] pageTitles_Thai;
    [TextArea(3, 5)][SerializeField] private string[] pageDescriptions_Thai;

    [Header("Text Data English")]
    [TextArea(1, 3)][SerializeField] private string[] pageTitles_English;
    [TextArea(3, 5)][SerializeField] private string[] pageDescriptions_English;

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;

    private int currentIndex = 0;

    void Start()
    {
        // ถ้าเป็นหน้า Home: ให้เช็กและเด้งเองอัตโนมัติ
        if (tutorialType == TutorialType.Home)
        {
            // เช็กว่ามี GameManager และยังไม่เคยดู Home Tutorial ใช่ไหม
            if (GameManager.Instance != null && !GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home)
            {
                StartTutorial();
            }
            else
            {
                // ถ้าเคยดูแล้ว หรือไม่ใช่ครั้งแรก ให้ปิดทิ้ง
                if (tutorialPanel != null) tutorialPanel.SetActive(false);
            }
        }
        else if (tutorialType == TutorialType.Story || tutorialType == TutorialType.Stage ||
                 tutorialType == TutorialType.Deck || tutorialType == TutorialType.Shop)
        {
            SelectMode();
        }
        else
        {
            // ถ้าเป็นโหมดอื่น (Story, Stage, etc.) ให้ปิดรอไว้ก่อน (รอคนกดปุ่ม)
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }


        // เชื่อมปุ่ม Next/Back ให้ทำงาน
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClick);
        if (backButton != null) backButton.onClick.AddListener(OnBackClick);
    }

    // --- ฟังก์ชันสำหรับปุ่มกด (เช่น ปุ่ม Story) ---
    // ลากฟังก์ชันนี้ไปใส่ใน On Click ของปุ่ม Story
    public void SelectMode()
    {
        bool hasSeen = false;

        // 1. ตรวจสอบข้อมูลจาก Save ว่าเคยดูหรือยัง
        if (GameManager.Instance != null)
        {
            var data = GameManager.Instance.CurrentGameData.tutorialData;
            switch (tutorialType)
            {
                case TutorialType.Story: hasSeen = data.hasSeenTutorial_Story; break;
                case TutorialType.Shop: hasSeen = data.hasSeenTutorial_Shop; break;
                case TutorialType.Deck: hasSeen = data.hasSeenTutorial_Deck; break;
                    // case TutorialType.Stage: hasSeen = data.hasSeenTutorial_Stage; break;
            }
        }

        // 2. ตัดสินใจ
        if (hasSeen)
        {
            Debug.Log("เคยดูแล้ว -> เข้าเกมเลย");
            // GoToNextScene();
        }
        else
        {
            Debug.Log("ยังไม่เคยดู -> เปิด Tutorial");
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        else Debug.LogError("ลืมลาก Tutorial Panel ใส่ใน Inspector!");

        currentIndex = 0;
        UpdatePageDisplay();
    }

    void OnNextClick()
    {
        if (currentIndex < tutorialPages.Count - 1)
        {
            currentIndex++;
            UpdatePageDisplay();
        }
        else
        {
            CompleteTutorial();
        }
    }

    void OnBackClick()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePageDisplay();
        }
    }

    void UpdatePageDisplay()
    {
        // เปิดรูปที่ตรงกับหน้าปัจจุบัน ปิดรูปอื่น
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            if (tutorialPages[i] != null) tutorialPages[i].SetActive(i == currentIndex);
        }

        // เปลี่ยนข้อความ
        if (GameManager.Instance.CurrentGameData.isTranstale)
        {
            if (headerText != null && pageTitles_English.Length > currentIndex)
            {
                // English
                headerText.text = pageTitles_English[currentIndex];
            }

            if (descriptionText != null && pageDescriptions_English.Length > currentIndex)
            {
                // English
                descriptionText.text = pageDescriptions_English[currentIndex];
            }
        }
        else
        {
            if (headerText != null && pageTitles_Thai.Length > currentIndex)
            {
                // Thai
                headerText.text = pageTitles_Thai[currentIndex];
            }

            if (descriptionText != null && pageDescriptions_Thai.Length > currentIndex)
            {
                // Thai
                descriptionText.text = pageDescriptions_Thai[currentIndex];
            }
        }

        // จัดการสถานะปุ่ม
        if (backButton != null) backButton.interactable = (currentIndex > 0);

        if (nextButtonText != null)
        {
            if (currentIndex == tutorialPages.Count - 1) nextButtonText.text = "GO!"; // หรือ FINISH
            else nextButtonText.text = "NEXT";
        }
    }

    void CompleteTutorial()
    {
        // 1. บันทึกว่าดูจบแล้ว
        if (GameManager.Instance != null)
        {
            var data = GameManager.Instance.CurrentGameData.tutorialData;

            if (tutorialType == TutorialType.Home) data.hasSeenTutorial_Home = true;
            else if (tutorialType == TutorialType.Story) data.hasSeenTutorial_Story = true;
            else if (tutorialType == TutorialType.Shop) data.hasSeenTutorial_Shop = true;
            else if (tutorialType == TutorialType.Deck) data.hasSeenTutorial_Deck = true;
            // else if (tutorialType == TutorialType.Stage) data.hasSeenTutorial_Stage = true;

            GameManager.Instance.SaveCurrentGame();
        }

        // 2. จบแล้วทำไงต่อ?
        if (tutorialPanel != null) tutorialPanel.SetActive(false);


        // if (tutorialType == TutorialType.Home)
        // {
        //     // หน้าแรกแค่ปิดหน้าต่าง
        //     if (tutorialPanel != null) tutorialPanel.SetActive(false);
        // }
        // else
        // {
        //     // โหมดอื่น (Story) จบแล้วให้เปลี่ยนฉาก
        //     GoToNextScene();
        // }
    }

    // void GoToNextScene()
    // {
    //     if (!string.IsNullOrEmpty(nextSceneName))
    //     {
    //         SceneManager.LoadScene(nextSceneName);
    //     }
    //     else
    //     {
    //         Debug.LogError("Error: คุณลืมใส่ชื่อ Scene ในช่อง Next Scene Name!");
    //     }
    // }
}