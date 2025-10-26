using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuidedTutoriol : MonoBehaviour
{
    [Header("UI หลักของ Tutorial")]
    [SerializeField] private GameObject tutorialPanel; // ลาก GuidedTutorial_Panel มาใส่
    
    [Header("ส่วนประกอบ Tutorial")]
    [SerializeField] private RectTransform highlightBorder; // กรอบไฮไลต์
    [SerializeField] private RectTransform descriptionBox;  // กล่องข้อความ
    [SerializeField] private TextMeshProUGUI descriptionText; // Text ที่ใช้อธิบาย
    [SerializeField] private Button nextButton; // ปุ่ม "ถัดไป"

    [Header("เป้าหมายที่จะสอน (ลากปุ่มมาใส่ตามลำดับ)")]
    [SerializeField] private RectTransform[] tutorialTargets; // ปุ่ม Stage, Story, Deck, Shop

    [Header("คำอธิบาย (พิมพ์ให้ตรงกับลำดับปุ่ม)")]
    [TextArea(3, 5)]
    [SerializeField] private string[] tutorialDescriptions;

    private int currentStep = 0; // ตัวนับว่าสอนถึงขั้นไหนแล้ว

    void Start()
    {
        // 1. เช็กเหมือนเดิม: ถ้ายังไม่เคยดู Tutorial
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentGameData != null &&
            !GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home)
        {
            StartTutorial(); // ให้เริ่มการสอน
        }
        else
        {
            tutorialPanel.SetActive(false); // ถ้าเคยดูแล้ว ก็ปิดไว้
        }
    }

    private void StartTutorial()
    {
        currentStep = 0;
        tutorialPanel.SetActive(true);
        // สั่งให้ปุ่ม "Next" เมื่อถูกคลิก ให้ไปรันฟังก์ชัน OnNextButtonClick
        nextButton.onClick.RemoveAllListeners(); // ล้างของเก่าทิ้งก่อน
        nextButton.onClick.AddListener(OnNextButtonClick);
        ShowStep(currentStep); // แสดงขั้นตอนแรก
    }

    private void OnNextButtonClick()
    {
        currentStep++; // ไปขั้นตอนถัดไป
        if (currentStep < tutorialTargets.Length)
        {
            ShowStep(currentStep); // ถ้ายังมีสอนต่อ ก็แสดงขั้นต่อไป
        }
        else
        {
            EndTutorial(); // ถ้าสอนครบแล้ว ก็จบการสอน
        }
    }
    // นี่คือหัวใจสำคัญ: ฟังก์ชันย้ายไฮไลต์และเปลี่ยนข้อความ
    private void ShowStep(int stepIndex)
    {
        // 1. ย้าย "กรอบไฮไลต์" ไปที่ตำแหน่งของปุ่มเป้าหมาย
        RectTransform target = tutorialTargets[stepIndex];
        
        if (stepIndex == 0) {
            highlightBorder.position = target.position;
            highlightBorder.sizeDelta = new Vector2(
            500,
            200
            );
            descriptionText.text = tutorialDescriptions[stepIndex];
            descriptionBox.position = target.position;
        } else {
        // 2. ขยาย "กรอบไฮไลต์" ให้ใหญ่กว่าปุ่มนิดหน่อย (เช่น 20-40 pixels)
        highlightBorder.position = target.position - new Vector3(0, 5, 0);
        float padding_w = 55f;
        float padding_h = 35f;
        
        highlightBorder.sizeDelta = new Vector2(
            target.rect.width * 1.2f,
            target.rect.height *1.35f
        );
        // 3. เปลี่ยน "ข้อความอธิบาย"
        descriptionText.text = tutorialDescriptions[stepIndex];
        // 4. (ขั้นสูง) ย้าย "กล่องข้อความ" ให้อยู่ใกล้ๆ ปุ่ม (เช่น อยู่เหนือปุ่ม)
        // if (target.position.y > 0)
        // {
        //     // ถ้าปุ่มอยู่ด้านบนของจอ ให้ย้ายกล่องข้อความไปด้านล่าง
        //     descriptionBox.position = target.position - new Vector3(0, 50, 0);
        // }
        // else
        // {
        //     // ถ้าปุ่มอยู่ด้านล่างของจอ ให้ย้ายกล่องข้อความไปด้านบน
        //     descriptionBox.position = target.position + new Vector3(0, 50, 0);
        // }
        descriptionBox.position = target.position + new Vector3(0, 30, 0);
        }
        
        // }
        // 5. ปรับตำแหน่งกล่องข้อความไม่ให้เลยขอบจอ
        // Vector3 boxPos = descriptionBox.position;
        // float boxWidth = descriptionBox.rect.width;
        // float screenWidth = Screen.width;
    }

    private void EndTutorial()
    {
        // 1. ปิดหน้าต่างสอน
        tutorialPanel.SetActive(false);
        // 2. "บันทึก" ว่าดูจบแล้ว (สำคัญมาก!)
        GameManager.Instance.CurrentGameData.tutorialData.hasSeenTutorial_Home = true;
        GameManager.Instance.SaveCurrentGame();
    }
}
