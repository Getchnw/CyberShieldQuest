using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GuidedTutorialController : MonoBehaviour
{
    [Header("UI Objects (Images)")]
    [Tooltip("ลาก Panel ตัวแม่ของ Tutorial มาใส่")]
    [SerializeField] private GameObject tutorialPanel;

    [Tooltip("ลากหน้า Home, Stage, Story, Deck, Shop มาใส่เรียงตามลำดับ 5 หน้า")]
    [SerializeField] private List<GameObject> tutorialPages;

    [Header("UI Text (To be updated)")]
    [SerializeField] private TextMeshProUGUI headerText;      // ลากตัว Name ข้างบนมาใส่
    [SerializeField] private TextMeshProUGUI descriptionText; // ลากตัว Text (TMP) ข้างล่างมาใส่

    [Header("Text Data")]
    [TextArea(1, 3)]
    [SerializeField] private string[] pageTitles;       // พิมพ์ชื่อหัวข้อ 5 อันเรียงกัน
    [TextArea(3, 5)]
    [SerializeField] private string[] pageDescriptions; // พิมพ์คำอธิบาย 5 อันเรียงกัน

    [Header("Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene"; // (เก็บไว้เผื่ออนาคตอยากเปลี่ยนฉาก)

    private int currentIndex = 0;

    void Start()
    {
        // 1. ตรวจสอบข้อมูลและเริ่มทำงาน
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            // ถ้ายังไม่เคยดู Tutorial ให้เริ่มแสดงผล
            if (!GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home)
            {
                StartTutorial();
            }
            else
            {
                // ถ้าเคยดูแล้ว ให้ปิด Panel ทิ้งไปเลย
                if (tutorialPanel != null) tutorialPanel.SetActive(false);
            }
        }
        else
        {
            // ถ้าไม่มี GameManager ให้ปิดไว้ก่อน (กัน Error)
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }

        // 2. ผูกฟังก์ชันปุ่ม (Listener)
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClick);
        if (backButton != null) backButton.onClick.AddListener(OnBackClick);
    }

    public void StartTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        currentIndex = 0;
        UpdatePageDisplay();
    }

    void OnNextClick()
    {
        // ถ้ายังไม่ใช่หน้าสุดท้าย ให้ไปหน้าถัดไป
        if (currentIndex < tutorialPages.Count - 1)
        {
            currentIndex++;
            UpdatePageDisplay();
        }
        else
        {
            // ถ้าอยู่หน้าสุดท้ายแล้วกด Next -> จบการสอน
            CompleteTutorial();
        }
    }

    void OnBackClick()
    {
        // ถ้าย้อนกลับได้ ให้ย้อน
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePageDisplay();
        }
    }

    void UpdatePageDisplay()
    {
        // 1. อัปเดตการแสดงผลรูปภาพ
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            if (tutorialPages[i] != null)
                tutorialPages[i].SetActive(i == currentIndex);
        }

        // 2. อัปเดตข้อความ (Header และ Description)
        if (headerText != null && pageTitles.Length > currentIndex)
        {
            headerText.text = pageTitles[currentIndex];
        }

        if (descriptionText != null && pageDescriptions.Length > currentIndex)
        {
            descriptionText.text = pageDescriptions[currentIndex];
        }

        // 3. อัปเดตสถานะปุ่ม
        // ปุ่ม Back กดได้เมื่อไม่ใช่หน้าแรก
        if (backButton != null)
            backButton.interactable = (currentIndex > 0);

        // เปลี่ยนข้อความปุ่ม Next เป็น START หรือ FINISH เมื่อถึงหน้าสุดท้าย
        if (nextButtonText != null)
        {
            if (currentIndex == tutorialPages.Count - 1)
                nextButtonText.text = "FINISH"; // หรือใช้คำว่า EXIT ก็ได้
            else
                nextButtonText.text = "NEXT";
        }
    }

    void CompleteTutorial()
    {
        // 1. บันทึกข้อมูลว่าดูจบแล้ว
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home = true;
            // GameManager.Instance.CurrentGameData.isNewGameStarted = true;
            GameManager.Instance.SaveCurrentGame();
        }

        // 2. สั่งปิดหน้านี้ (ออกจากหน้า Tutorial)
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        Debug.Log("Tutorial Completed. Panel Closed.");

        // หมายเหตุ: ถ้าในอนาคตอยากให้เข้าเกมทันทีหลังจากดูจบ ให้เปิดบรรทัดล่างนี้แทนครับ
        // SceneManager.LoadScene(gameSceneName);
    }
}