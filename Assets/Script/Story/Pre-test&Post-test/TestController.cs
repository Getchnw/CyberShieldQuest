using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class TestController : MonoBehaviour
{
    [Header("Main Container")]
    [SerializeField] private GameObject mainTestBackground;

    [Header("UI Prefabs")]
    [SerializeField] private GameObject matchingPanelPrefab;
    [SerializeField] private GameObject fillInBlankPanelPrefab;
    [SerializeField] private GameObject trueFalsePanelPrefab;

    [Header("UI Containers")]
    private Transform questionContainer; // Panel กลางที่จะยัด Prefab ใส่

    [Header("Feedback UI")]
    [Tooltip("ลาก Image ที่เป็นแถบเวลา (Fill Method = Horizontal) มาใส่")]
    [SerializeField] private Image delayProgressBar; // แถบเวลา
    [SerializeField] private Button prevButton; // (จะถูกปิดการใช้งาน)

    [Header("Test Pages")] // (แก้ไข Gameobject เป็น GameObject)
    [SerializeField] public GameObject PreTestPage;
    [SerializeField] public GameObject PostTest;

    [Header("Navigation Buttons (ลากปุ่มจาก Canvas มาใส่)")]
    [SerializeField] private Button btnPrev;
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnSubmit; // ปุ่มส่งครั้งสุดท้าย

    private List<BaseTestQuestion> allQuestions; // ลิสต์คำถามที่สุ่มมาแล้ว
    private List<GameObject> instantiatedPanels; // หน้าคำถามแต่ละข้อ
    private List<PlayerChapterProgress> allChapterProgress; // (ใหม่: สำหรับเช็ค Progress)

    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool isLoadingNextQuestion = false;
    // private bool isPreTest = false;
    // private bool isPostTest = false;
    private enum TestMode { None, PreTest, PostTest }
    private TestMode currentTestMode = TestMode.None;

    IEnumerator Start()
    {
        // 0. ตั้งค่าเริ่มต้น
        instantiatedPanels = new List<GameObject>();
        if (delayProgressBar != null) delayProgressBar.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false); // ปิดปุ่ม "ย้อนกลับ"
        isLoadingNextQuestion = false;

        // ผูกปุ่มกับฟังก์ชัน
        btnPrev.onClick.AddListener(OnPrevClicked);
        btnNext.onClick.AddListener(OnNextClicked);
        btnSubmit.onClick.AddListener(OnSubmitClicked);

        // 1. รอให้ Database (Singleton) พร้อม
        yield return null; // รอ 1 เฟรม

        allChapterProgress = GameManager.Instance.CurrentGameData.chapterProgress ?? new List<PlayerChapterProgress>();

        // 2. ตัดสินใจว่าจะเปิด Pre-Test หรือ Post-Test หรือ Block
        var StoryId_now = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        CheckPostOrPreTest(StoryId_now);

        // 3. ถ้าไม่มีคำถามเลย (ถูกบล็อก) ก็ไม่ต้องทำอะไรต่อ
        if (allQuestions == null || allQuestions.Count == 0)
        {
            Debug.Log("การทดสอบถูกบล็อก หรือไม่พบคำถามใน Database!");
            yield break;
        }

        // 4. สร้าง UI ทั้งหมดเก็บไว้ (แต่ซ่อนไว้)
        InstantiateAllQuestionPanels();

        // 5. แสดงคำถามข้อแรก
        ShowQuestion(0);
    }

    private bool GetStatusFirstPaly(string storyId)
    {
        // Note: ต้องมั่นใจว่า statusPreTest ถูกสร้างไว้ใน GameData
        if (GameManager.Instance.CurrentGameData.statusPreTest == null) return false;

        switch (storyId)
        {
            case "A01":
                Debug.Log($"Status Pre-A01: {GameManager.Instance.CurrentGameData.statusPreTest.hasSucessPre_A01}");
                return GameManager.Instance.CurrentGameData.statusPreTest.hasSucessPre_A01;
            case "A02":
                return GameManager.Instance.CurrentGameData.statusPreTest.hasSucessPre_A02;
            case "A03":
                return GameManager.Instance.CurrentGameData.statusPreTest.hasSucessPre_A03;
            default:
                return false; // Error C: เพิ่ม return path
        }
    }

    private bool checkProgressChapter(string StoryId_now)
    {
        // ใช้ allChapterProgress ที่ดึงมาแล้ว
        if (allChapterProgress == null || allChapterProgress.Count == 0) return false;

        // 1. หา Chapter ทั้งหมดของ Story
        List<ChapterData> AllStoryChapter = GameContentDatabase.Instance.GetChaptersByStoryID(StoryId_now);

        if (AllStoryChapter != null && AllStoryChapter.Count > 0)
        {
            // 2. หา Chapter "สุดท้าย"
            ChapterData lastChapterOfStory = AllStoryChapter.LastOrDefault(); // ใช้ LastOrDefault()

            // 3. ตรวจสอบ Progress ของ Chapter สุดท้ายนั้น
            PlayerChapterProgress progress = allChapterProgress.Find(
                p => p.chapter_id == lastChapterOfStory.chapter_id);

            // ถ้า Chapter สุดท้ายนั้น "is_completed" = true
            if (progress != null && progress.is_completed)
            {
                return true;
            }
        }
        return false;
    }

    private bool HasCompletedPostTest(string storyId)
    {
        if (GameManager.Instance.CurrentGameData.postTestResults == null)
            return false;

        // ใช้ Linq.Any() เพื่อเช็คว่ามี Story ID นี้อยู่ใน List แล้วหรือยัง
        return GameManager.Instance.CurrentGameData.postTestResults
            .Any(p => p.story_id == storyId);
    }

    private void CheckPostOrPreTest(string StoryId_now)
    {
        bool isClearAllChapter = checkProgressChapter(StoryId_now);
        bool statusFirstPlay = GetStatusFirstPaly(StoryId_now);
        bool hasDonePostTest = HasCompletedPostTest(StoryId_now);

        // --- 1. Logic การตัดสินใจ (ตามที่ผู้ใช้กำหนด) ---
        if (!isClearAllChapter && !statusFirstPlay)
        {
            // State: PRE-TEST (เล่นครั้งแรก และ ยังไม่ผ่าน Pre-Test)
            Debug.Log("Test Mode: PRE-TEST");
            currentTestMode = TestMode.PreTest;
            Debug.Log(currentTestMode);
            if (mainTestBackground != null)
            {
                mainTestBackground.SetActive(true);
            }
            // --- คำสั่งเปิด/ปิดหน้า ---
            if (PreTestPage != null) PreTestPage.SetActive(true);
            if (PostTest != null) PostTest.SetActive(false);

            // สร้างคำถาม
            BuildRandomTestSet(StoryId_now);
        }
        else if (isClearAllChapter && statusFirstPlay && !hasDonePostTest)
        {
            // State: POST-TEST (เล่นจบแล้ว และ ผ่าน Pre-Test แล้ว)
            Debug.Log("Test Mode: POST-TEST");
            currentTestMode = TestMode.PostTest;
            // --- คำสั่งเปิด/ปิดหน้า ---
            if (mainTestBackground != null)
            {
                mainTestBackground.SetActive(true);
            }
            if (PreTestPage != null) PreTestPage.SetActive(false);
            if (PostTest != null) PostTest.SetActive(true);

            // สร้างคำถาม
            BuildRandomTestSet(StoryId_now);
        }
        else if (hasDonePostTest)
        {
            // State: ALREADY DONE (ทำเสร็จแล้ว ไม่ต้องทำซ้ำ)
            Debug.Log("Test Blocked: Post-Test already completed for this story.");
            if (mainTestBackground != null)
            {
                mainTestBackground.SetActive(false);
            }
            if (PreTestPage != null) PreTestPage.SetActive(false);
            if (PostTest != null) PostTest.SetActive(false);
            allQuestions = new List<BaseTestQuestion>();
        }
        else
        {
            // State: BLOCKED / MID-PLAY (อยู่ระหว่างการเล่น หรือ เงื่อนไขไม่ตรง)
            Debug.Log("Test Blocked: Condition not met (Mid-play or already done).");
            if (mainTestBackground != null)
            {
                mainTestBackground.SetActive(false);
            }

            // --- คำสั่งเปิด/ปิดหน้า (ซ่อนทั้งหมด) ---
            if (PreTestPage != null) PreTestPage.SetActive(false);
            if (PostTest != null) PostTest.SetActive(false);

            // ปิดไม่ให้ Build คำถาม
            allQuestions = new List<BaseTestQuestion>();
        }
    }

    void BuildRandomTestSet(string StoryId_now)
    {
        allQuestions = new List<BaseTestQuestion>();

        // 1. สุ่ม Matching
        var matchingDb = GameContentDatabase.Instance.GetMatchingQuestionsByStoryId(StoryId_now);
        if (matchingDb != null && matchingDb.Count > 0)
        {
            allQuestions.Add(matchingDb[Random.Range(0, matchingDb.Count)]);
        }

        // 2. สุ่ม Fill in the Blank
        var fillInBlankDb = GameContentDatabase.Instance.GetFillInBlankQuestionsByStoryId(StoryId_now);
        if (fillInBlankDb != null && fillInBlankDb.Count > 0)
        {
            allQuestions.Add(fillInBlankDb[Random.Range(0, fillInBlankDb.Count)]);
        }

        // 3. สุ่ม True/False
        var trueFalseDb = GameContentDatabase.Instance.GetTrueFalseQuestionsByStoryId(StoryId_now);
        // (แก้ไข Error D: เพิ่ม if check)
        if (trueFalseDb != null && trueFalseDb.Count > 0)
        {
            allQuestions.Add(trueFalseDb[Random.Range(0, trueFalseDb.Count)]);
        }

        // 4. (สำคัญ) สับไพ่คำถาม 3 ข้อนี้ เพื่อไม่ให้เรียงลำดับเดิม
        Shuffle(allQuestions);
    }
    // สร้าง UI ทั้งหมดเก็บไว้ แต่ซ่อนไว้ก่อน
    void InstantiateAllQuestionPanels()
    {
        foreach (BaseTestQuestion question in allQuestions)
        {
            GameObject panelInstance = null;
            if (currentTestMode == TestMode.PreTest)
            {
                questionContainer = PreTestPage.transform.Find("QuestionContainer");
            }
            else if (currentTestMode == TestMode.PostTest)
            {
                questionContainer = PostTest.transform.Find("QuestionContainer");
            }
            if (question is MatchingQuestion mq)
            {
                panelInstance = Instantiate(matchingPanelPrefab, questionContainer);

                // (อัปเกรด) "เปิด" โค้ด และ "เชื่อม Event"
                var panelUI = panelInstance.GetComponent<MatchingPanelUI>();
                panelUI.Setup(mq);
                panelUI.OnAnswerUpdated += UpdateButtonStates;
            }
            else if (question is FillInBlankQuestion fbq)
            {
                panelInstance = Instantiate(fillInBlankPanelPrefab, questionContainer);

                var panelUI = panelInstance.GetComponent<FillInBlankPanelUI>();
                panelUI.Setup(fbq);
                panelUI.OnAnswerUpdated += UpdateButtonStates;
            }
            else if (question is TrueFalseQuestion tfq)
            {
                panelInstance = Instantiate(trueFalsePanelPrefab, questionContainer);

                var panelUI = panelInstance.GetComponent<TrueFalsePanelUI>();
                panelUI.Setup(tfq);
                panelUI.OnAnswerUpdated += UpdateButtonStates;
            }

            if (panelInstance != null)
            {
                panelInstance.SetActive(false);
                instantiatedPanels.Add(panelInstance);
            }
            // เปิดPretest
            // เปิดposttest

        }
    }

    void EndTestAndSaveResults(List<Qustion_Answer> answerList, int calculatedMaxScore)
    {
        Debug.Log("--- จบแบบทดสอบ ---");
        string storyId = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        // int totalQuestions = allQuestions.Count;
        // int score = correctAnswersCount;

        // ใช้ Enum เช็ค
        if (currentTestMode == TestMode.PreTest)
        {
            GameManager.Instance.SavePreTest_PostTest(true, storyId, correctAnswersCount, calculatedMaxScore, answerList);
        }
        else if (currentTestMode == TestMode.PostTest)
        {
            GameManager.Instance.SavePreTest_PostTest(false, storyId, correctAnswersCount, calculatedMaxScore, answerList);
        }

        if (mainTestBackground != null)
        {
            mainTestBackground.SetActive(false);
        }
    }

    IEnumerator LoadNextQuestionWithDelay(int targetIndex, bool isFinishing = false)
    {
        isLoadingNextQuestion = true;

        // --- แสดง Delay Bar ---
        if (delayProgressBar != null)
        {
            delayProgressBar.gameObject.SetActive(true);
            delayProgressBar.fillAmount = 0;
            float timer = 0f;
            while (timer < 1.0f)
            {
                timer += Time.deltaTime;
                delayProgressBar.fillAmount = timer / 1.0f;
                yield return null;
            }
            delayProgressBar.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        // --- เปลี่ยนหน้า หลังจาก Delay จบ ---
        if (isFinishing)
        {
            CalculateTotalScoreAndFinish(); // จบเกม
        }
        else
        {
            currentQuestionIndex = targetIndex; // อัปเดต Index
            ShowQuestion(currentQuestionIndex); // เปลี่ยนหน้า UI
        }

        isLoadingNextQuestion = false;
    }

    // แสดงคำถามที่ Index ที่กำหนด
    void ShowQuestion(int index)
    {
        // ซ่อนทุก Panel
        foreach (GameObject panel in instantiatedPanels)
        {
            panel.SetActive(false);
        }

        // แสดง Panel ที่ต้องการ
        if (index >= 0 && index < instantiatedPanels.Count)
        {
            instantiatedPanels[index].SetActive(true);
        }

        // (อัปเดตปุ่ม Next/Prev/Submit)
        UpdateButtonStates();
    }

    // --- ฟังก์ชันเช็คและเปิดปิดปุ่ม ---//
    void UpdateButtonStates()
    {
        // 1. ปุ่มย้อนกลับ (Prev): เปิดเมื่อไม่ใช่ข้อแรก (Index > 0)
        btnPrev.gameObject.SetActive(currentQuestionIndex > 0);

        // 2. เช็คว่า "ข้อปัจจุบัน" ตอบครบหรือยัง?
        bool isCurrentAnswered = CheckIfCurrentQuestionIsAnswered();

        // 3. เช็คว่าเป็น "ข้อสุดท้าย" หรือไม่?
        bool isLastQuestion = currentQuestionIndex == allQuestions.Count - 1;

        if (isLastQuestion)
        {
            // ถ้าข้อสุดท้าย: ซ่อน Next, โชว์ Submit
            btnNext.gameObject.SetActive(false);
            btnSubmit.gameObject.SetActive(true);

            // ปุ่ม Submit จะกดได้ก็ต่อเมื่อ "ตอบครบแล้ว"
            btnSubmit.interactable = isCurrentAnswered;
        }
        else
        {
            // ถ้าไม่ใช่ข้อสุดท้าย: โชว์ Next, ซ่อน Submit
            btnNext.gameObject.SetActive(true);
            btnSubmit.gameObject.SetActive(false);

            // ปุ่ม Next จะกดได้ก็ต่อเมื่อ "ตอบครบแล้ว"
            btnNext.interactable = isCurrentAnswered;
        }
    }

    // Function: ที่ไปถาม Panel ปัจจุบันว่า "เสร็จยัง?"
    bool CheckIfCurrentQuestionIsAnswered()
    {
        GameObject currentPanel = instantiatedPanels[currentQuestionIndex];

        // เช็คทีละประเภท
        if (currentPanel.GetComponent<MatchingPanelUI>() != null)
            return currentPanel.GetComponent<MatchingPanelUI>().IsAnswered();

        if (currentPanel.GetComponent<FillInBlankPanelUI>() != null)
            return currentPanel.GetComponent<FillInBlankPanelUI>().IsAnswered();

        if (currentPanel.GetComponent<TrueFalsePanelUI>() != null)
            return currentPanel.GetComponent<TrueFalsePanelUI>().IsAnswered();

        return false;
    }

    void OnNextClicked()
    {
        if (isLoadingNextQuestion) return; // กันกดรัว

        if (currentQuestionIndex < allQuestions.Count - 1)
        {
            // สั่งให้ Delay แล้วค่อยไปหน้าถัดไป (Index + 1)
            StartCoroutine(LoadNextQuestionWithDelay(currentQuestionIndex + 1));
        }
    }

    void OnPrevClicked()
    {
        if (isLoadingNextQuestion) return; // กันกดรัว

        if (currentQuestionIndex > 0)
        {
            // สั่งให้ Delay แล้วค่อยย้อนกลับ (Index - 1)
            StartCoroutine(LoadNextQuestionWithDelay(currentQuestionIndex - 1));
        }
    }

    void OnSubmitClicked()
    {
        if (isLoadingNextQuestion) return;

        // สั่งให้ Delay แล้วค่อยจบเกม (isFinishing = true)
        StartCoroutine(LoadNextQuestionWithDelay(0, true));
    }

    void CalculateTotalScoreAndFinish()
    {
        int totalScoreEarned = 0;
        int totalMaxScore = 0;

        List<Qustion_Answer> collectedAnswers = new List<Qustion_Answer>();

        foreach (var panel in instantiatedPanels)
        {
            Qustion_Answer qaData = null;
            int panelMaxScore = 0;
            // --- Matching ---
            if (panel.GetComponent<MatchingPanelUI>() != null)
            {
                var p = panel.GetComponent<MatchingPanelUI>();

                // ดึง List ข้อย่อย
                List<Qustion_Answer> subList = p.GetSplitQuestionAnswers();

                // รวมคะแนน Matching
                foreach (var sub in subList)
                {
                    sub.TypeQustion = TypeQustion.Matching;
                    totalScoreEarned += sub.score; // ใช้ตัวแปรที่ประกาศไว้ข้างบน
                }
                collectedAnswers.AddRange(subList);
                totalMaxScore += p.GetMaxScore();

                // Matching จัดการเสร็จแล้ว ข้ามไป Loop ถัดไปเลย เพื่อไม่ให้ชน Logic ข้างล่าง
                continue;
            }

            // --- Fill In Blank ---
            else if (panel.GetComponent<FillInBlankPanelUI>() != null)
            {
                var p = panel.GetComponent<FillInBlankPanelUI>();
                qaData = p.GetQuestionAnswerData();
                if (qaData != null) qaData.TypeQustion = TypeQustion.FillBlank;
                panelMaxScore = p.GetMaxScore();
            }
            // --- True / False ---
            else if (panel.GetComponent<TrueFalsePanelUI>() != null)
            {
                var p = panel.GetComponent<TrueFalsePanelUI>();
                qaData = p.GetQuestionAnswerData();
                if (qaData != null) qaData.TypeQustion = TypeQustion.TrueFalse;
                panelMaxScore = p.GetMaxScore();
            }

            // 3. รวมคะแนน (สำหรับ FillBlank และ TrueFalse)
            if (qaData != null)
            {
                totalScoreEarned += qaData.score;
                collectedAnswers.Add(qaData);
                totalMaxScore += panelMaxScore;
            }
        }

        if (collectedAnswers == null)
        {
            collectedAnswers = new List<Qustion_Answer>(); // กันตาย สร้าง List เปล่ารอไว้
        }

        correctAnswersCount = totalScoreEarned;
        // ส่งข้อมูลไปบันทึก
        EndTestAndSaveResults(collectedAnswers, totalMaxScore);

    }


    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}