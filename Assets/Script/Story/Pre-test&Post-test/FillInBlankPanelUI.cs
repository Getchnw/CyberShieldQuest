using UnityEngine;
using UnityEngine.UI; // ต้องมี
using TMPro; // ต้องมี
using System.Collections.Generic; // ต้องมี
using System.Linq;

public class FillInBlankPanelUI : MonoBehaviour
{
    [Header("Containers (ลากมาใส่)")]
    [SerializeField] private Transform wordBankContainer; //
    [SerializeField] private Transform sentenceContainer;

    [Header("Prefabs (ลากมาใส่)")]
    [SerializeField] private GameObject draggableWordPrefab; // Draggable_Word (Prefab)
    [SerializeField] private GameObject sentenceRowPrefab;   // Sentence_Row (Prefab)

    // (ใหม่) เก็บ DropZones เพื่อใช้ตรวจคำตอบ
    private List<DropZone> spawnedDropZones = new List<DropZone>();
    private FillInBlankQuestion questionData;
    public event System.Action OnAnswerUpdated;

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

    // TestController จะเรียกใช้ฟังก์ชันนี้
    public void Setup(FillInBlankQuestion data)
    {
        questionData = data;
        spawnedDropZones.Clear();

        // 1. ล้างของเก่า (ถ้ามี)
        foreach (Transform child in wordBankContainer) Destroy(child.gameObject);
        foreach (Transform child in sentenceContainer) Destroy(child.gameObject);

        // 2. สร้าง "คลังคำ"
        foreach (string word in data.wordBank)
        {
            // สร้าง Prefab "คำ"
            GameObject wordObj = Instantiate(draggableWordPrefab, wordBankContainer);

            // ใส่ข้อความ
            wordObj.GetComponentInChildren<TextMeshProUGUI>().text = word;

            // (สำคัญ) ตั้งค่า ID คำตอบ ให้ตรงกับข้อความ
            wordObj.GetComponent<DraggableItem>().answerID = word;
        }

        // 3. สร้าง "แถวประโยค"
        foreach (SentenceBlank sentence in data.sentences)
        {
            // สร้าง Prefab "แถวประโยค"
            GameObject sentenceObj = Instantiate(sentenceRowPrefab, sentenceContainer);

            GameObject beforeTextObj = sentenceObj.transform.Find("Before area text").gameObject;
            GameObject afterTextObj = sentenceObj.transform.Find("After area text").gameObject;

            // ใส่ข้อความ ส่วนที่ 1 และ 2
            if (string.IsNullOrEmpty(sentence.sentencePart1))
            {
                // ถ้า Text ว่าง: ให้ "ซ่อน" GameObject นี้
                beforeTextObj.SetActive(false);
            }
            else
            {
                // ถ้า Text ไม่ว่าง: "แสดง" GameObject นี้ และ "ใส่ข้อความ"
                beforeTextObj.SetActive(true);
                beforeTextObj.GetComponent<TextMeshProUGUI>().text = sentence.sentencePart1;
            }

            // 3. ตรวจสอบ "โจทย์ส่วนที่ 2" (After)
            if (string.IsNullOrEmpty(sentence.sentencePart2))
            {
                // ถ้า Text ว่าง: ให้ "ซ่อน" GameObject นี้
                afterTextObj.SetActive(false);
            }
            else
            {
                // ถ้า Text ไม่ว่าง: "แสดง" GameObject นี้ และ "ใส่ข้อความ"
                afterTextObj.SetActive(true);
                afterTextObj.GetComponent<TextMeshProUGUI>().text = sentence.sentencePart2;
            }

            // (สำคัญ) เก็บ DropZone ไว้อ้างอิง
            DropZone dropZone = sentenceObj.transform.Find("AreaDrop").GetComponent<DropZone>();
            spawnedDropZones.Add(dropZone);
        }
    }

    // (ฟังก์ชันนี้ TestController จะเรียกใช้ตอนกด "ส่งคำตอบ")
    public int CheckAnswers()
    {
        int correctCount = 0;

        // วนลูปตามจำนวนช่องว่าง (DropZones)
        for (int i = 0; i < spawnedDropZones.Count; i++)
        {
            DropZone zone = spawnedDropZones[i];

            // 1. ดึง Index เฉลยจากข้อมูล (ในรูป Inspector คุณใส่เลข "3")
            string correctIndexStr = questionData.sentences[i].correctAnswer;

            // 2. แปลงเลข "3" ให้กลายเป็นข้อความจาก WordBank (เช่น "เข้ารหัสซ้อน...")
            string correctWordText = "";
            if (int.TryParse(correctIndexStr, out int index))
            {
                // ตรวจสอบว่า Index ไม่เกินขนาดของ WordBank
                if (index >= 0 && index < questionData.wordBank.Count)
                {
                    correctWordText = questionData.wordBank[index];
                }
            }
            else
            {
                // กรณีที่คุณใส่เฉลยเป็นข้อความตรงๆ ไม่ใช่ตัวเลข
                correctWordText = correctIndexStr;
            }

            // 3. เทียบคำตอบ: สิ่งที่วาง (zone.currentDroppedAnswerID) == เฉลยที่เป็นข้อความ (correctWordText)
            // ใช้ Trim() เพื่อตัดช่องว่างหน้าหลังกันพลาด
            if (zone.currentDroppedAnswerID != null &&
                zone.currentDroppedAnswerID.Trim() == correctWordText.Trim())
            {
                correctCount++;
            }
        }
        return correctCount;
    }

    public bool IsAnswerCorrect()
    {
        // ถือว่า "ถูก" ก็ต่อเมื่อ "จำนวนข้อที่ถูก (correctCount)" เท่ากับ "จำนวนข้อทั้งหมด"
        return CheckAnswers() == questionData.sentences.Count;
    }

    // private void SubscribeToDropZones()
    // {
    //     // สมมติว่าคุณแก้ DropZone ให้มี Event ชื่อ OnAnyDrop แล้ว
    //     // หรือใช้วิธีง่ายๆ: ให้ DropZone เรียก FillInBlankPanelUI.NotifyUpdate()
    // }

    // ฟังก์ชันที่ DropZone จะเรียกเมื่อมีการวางของ
    public void NotifyAnswerUpdated()
    {
        OnAnswerUpdated?.Invoke();
    }

    // เช็คว่าเติมครบทุกช่องหรือยัง
    public bool IsAnswered()
    {
        if (spawnedDropZones.Count == 0) return false;
        // เช็คว่ามีช่องไหนว่างไหม
        return !spawnedDropZones.Any(z => string.IsNullOrEmpty(z.currentDroppedAnswerID));
    }

    // ... (Code เดิมด้านบน) ...

    // เพิ่มฟังก์ชันนี้ลงไปท้าย Class
    public Qustion_Answer GetQuestionAnswerData()
    {
        Qustion_Answer qa = new Qustion_Answer();

        // 1. สร้างโจทย์ (เอาประโยคมาต่อกันเพื่อเป็น Reference)
        // หรือจะใช้ชื่อหัวข้อก็ได้ถ้าประโยคยาวไป
        // qa.QustionText = string.Join(" ... ", questionData.sentences.Select(s => s.sentencePart1) +
        //     " _____ " +
        //     string.Join(" ", s.sentencePart2)).ToArray();
        // สมมติว่า sentencePart2 เป็น List หรือ Array
        string result = string.Join("\n", questionData.sentences.Select(s =>
            string.IsNullOrEmpty(s.sentencePart2)
            ? s.sentencePart1 + " _____ "                       // ถ้าไม่มีส่วนหลัง
            : s.sentencePart1 + " _____ " + s.sentencePart2     // ถ้ามีส่วนหลัง
        ));

        qa.QustionText = result;

        // 2. รวบรวมคำตอบที่ผู้เล่นใส่ในช่องว่าง
        List<string> playerAnswers = new List<string>();
        foreach (var zone in spawnedDropZones)
        {
            if (string.IsNullOrEmpty(zone.currentDroppedAnswerID))
                playerAnswers.Add("[Blank]");
            else
                playerAnswers.Add(zone.currentDroppedAnswerID);
        }
        qa.AnswerText = string.Join(", ", playerAnswers);

        // 3. คะแนนรวมของหน้านี้
        qa.score = CheckAnswers();

        return qa;
    }

    public int GetMaxScore()
    {
        // คะแนนเต็ม = จำนวนช่องว่างที่ต้องเติม
        if (questionData == null || questionData.sentences == null) return 0;
        return questionData.sentences.Count;
    }
}