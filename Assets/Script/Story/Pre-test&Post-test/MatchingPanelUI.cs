using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MatchingPanelUI : MonoBehaviour
{
    [Header("Containers (ลากมาใส่)")]
    [SerializeField] private Transform promptContainer; // (ที่ใส่โจทย์)
    [SerializeField] private Transform optionContainer; // (ที่ใส่ตัวเลือก)

    [Header("Prefabs (ลากมาใส่)")]
    [SerializeField] private GameObject promptRowPrefab; // (Prefab "แถวโจทย์ + ช่องว่าง")
    [SerializeField] private GameObject optionRowPrefab; // (Prefab "ตัวเลือกที่ลากได้")

    // เก็บข้อมูลไว้ตรวจคำตอบ
    private List<DropZone> spawnedDropZones = new List<DropZone>();
    private MatchingQuestion questionData;

    public event System.Action OnAnswerUpdated;
    // TestController จะเรียกฟังก์ชันนี้

    void OnEnable()
    {
        // เริ่มฟัง
        DropZone.OnDropZoneChanged += NotifyAnswerUpdated;
        DraggableItem.OnItemBeginDrag += NotifyAnswerUpdated;
    }

    void OnDisable()
    {
        // หยุดฟัง
        DropZone.OnDropZoneChanged -= NotifyAnswerUpdated;
        DraggableItem.OnItemBeginDrag -= NotifyAnswerUpdated;
    }

    public void Setup(MatchingQuestion data)
    {
        questionData = data;
        spawnedDropZones.Clear();

        // 1. ล้างของเก่า
        foreach (Transform child in promptContainer) Destroy(child.gameObject);
        foreach (Transform child in optionContainer) Destroy(child.gameObject);

        // 2. สร้างแถว "โจทย์" (Prompts)
        for (int i = 0; i < data.prompts.Count; i++)
        {
            // สร้าง Prefab แถวโจทย์
            GameObject promptObj = Instantiate(promptRowPrefab, promptContainer);
            promptObj.transform.localScale = Vector3.one;
            // ใส่ข้อความโจทย์
            promptObj.GetComponentInChildren<TextMeshProUGUI>().text = data.prompts[i];
            // ตั้งค่า "ช่องว่าง"
            DropZone dropZone = promptObj.transform.Find("DropArea").GetComponent<DropZone>();
            dropZone.promptIndex = i; // บอกช่องว่างว่า "เธอคือคำถามข้อ 0, 1, 2..."

            spawnedDropZones.Add(dropZone); // เก็บไวตรวจ
        }

        // 3. สร้างแถว "ตัวเลือก" (Options)
        for (int i = 0; i < data.options.Count; i++)
        {
            // สร้าง Prefab ตัวเลือก
            GameObject optionObj = Instantiate(optionRowPrefab, optionContainer);
            optionObj.transform.localScale = Vector3.one;
            // ใส่ข้อความตัวเลือก
            optionObj.GetComponentInChildren<TextMeshProUGUI>().text = data.options[i];

            // ตั้งค่า "ตัวลาก"
            DraggableItem item = optionObj.GetComponent<DraggableItem>();
            item.answerIndex = i; // บอกตัวเลือกว่า "เธอคือคำตอบข้อ 0, 1, 2..."
        }
    }

    // (ฟังก์ชันนี้ TestController จะเรียกใช้ตอนกด "ส่งคำตอบ")
    public int CheckAnswers()
    {
        int correctCount = 0;

        // วนลูปเช็กทุก "ช่องว่าง" ที่เราสร้าง
        foreach (DropZone zone in spawnedDropZones)
        {
            // ดึงข้อมูล
            int promptIndex = zone.promptIndex; // นี่คือคำถามข้อที่ (เช่น 0)
            int droppedAnswerIndex = zone.currentDroppedAnswerIndex; // นี่คือคำตอบที่ผู้เล่นลากมา (เช่น 2)

            // (ถ้ายังไม่ตอบ droppedAnswerIndex จะเป็น -1)
            if (droppedAnswerIndex == -1) continue;

            // ดึง "เฉลย" จาก ScriptableObject
            int correctAnswerIndex = questionData.correctAnswers_OptionIndex[promptIndex];

            // ตรวจสอบ
            if (droppedAnswerIndex == correctAnswerIndex)
            {
                correctCount++;
                // (Optional: ทำให้ช่องเป็นสีเขียว)
            }
            // else
            // {
            //     // (Optional: ทำให้ช่องเป็นสีแดง)
            // }
        }

        Debug.Log($"Matching Correct: {correctCount} / {questionData.prompts.Count}");
        return correctCount;
    }

    public bool IsAnswerCorrect()
    {
        // ถือว่าถูกก็ต่อเมื่อ "จำนวนข้อที่ถูก" เท่ากับ "จำนวนโจทย์ทั้งหมด"
        return CheckAnswers() == questionData.prompts.Count;
    }

    public void NotifyAnswerUpdated()
    {
        OnAnswerUpdated?.Invoke();
    }
    public bool IsAnswered()
    {
        if (spawnedDropZones.Count == 0) return false;
        // เช็คว่ามีช่องไหนยังเป็น -1 ไหม
        return !spawnedDropZones.Any(z => z.currentDroppedAnswerIndex == -1);
    }

    public List<Qustion_Answer> GetSplitQuestionAnswers()
    {
        List<Qustion_Answer> subAnswers = new List<Qustion_Answer>();

        // วนลูปทุกคู่โจทย์ที่มี (เช่น 4 ข้อ)
        foreach (var zone in spawnedDropZones)
        {
            Qustion_Answer qa = new Qustion_Answer();

            // 1. ระบุโจทย์ข้อย่อยนั้นๆ
            // เช่น "Matching: SQL Injection"
            string promptText = questionData.prompts[zone.promptIndex];
            qa.QustionText = "Matching: " + promptText;

            // 2. ระบุคำตอบที่ผู้เล่นเลือก
            string answerText = "[No Answer]";
            int score = 0;

            if (zone.currentDroppedAnswerIndex != -1)
            {
                // ชื่อคำตอบที่ลากมาวาง
                answerText = questionData.options[zone.currentDroppedAnswerIndex];

                // ตรวจว่าคู่นี้ถูกไหม?
                int correctAnswerIndex = questionData.correctAnswers_OptionIndex[zone.promptIndex];
                if (zone.currentDroppedAnswerIndex == correctAnswerIndex)
                {
                    score = 1; // ถูกได้ 1 คะแนน
                }
            }

            qa.AnswerText = answerText;
            qa.score = score;

            // เพิ่มข้อย่อยนี้ลงใน List
            subAnswers.Add(qa);
        }

        return subAnswers;
    }

    public int GetMaxScore()
    {
        // คะแนนเต็ม = จำนวนข้อที่ต้องจับคู่
        if (questionData == null || questionData.prompts == null) return 0;
        return questionData.prompts.Count;
    }
}