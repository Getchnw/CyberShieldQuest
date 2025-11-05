using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_QuizController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject questionPanel; // (Panel ที่มีคำถาม+ปุ่มตอบ)
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<Button> answerButtons; 
    [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;
    
    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private Transform rewardListContainer; // (ที่ใส่แถวของรางวัล)
    [SerializeField] private GameObject rewardRowPrefab; // (Prefab ของแถวรางวัล)
    [SerializeField] private Button nextEventButton;

    // "โทรโข่ง" 📢 บอก StoryEventController ว่า "Quiz จบแล้ว"
    public event System.Action OnQuizCompleted;

    private QuizData currentQuiz;
    private List<QuestionData> allQuestions;
    private List<Color> colorButton = new List<Color>();
    private List<Outline> outlineButton = new List<Outline>();
    private int currentQuestionIndex;
    private int correctAnswersCount;
    private int experienceAll;
    private int GoldAll;

    void Awake()
    {
        gameObject.SetActive(false); 
        CollectColersButton();
        //CollectOutlineButton();
    }
    
    void Start()
    {
        nextEventButton.onClick.AddListener(FinishQuiz); 
    }

    public void StartQuiz(QuizData quizData)
    {
        currentQuiz = quizData;
        gameObject.SetActive(true);
        Debug.Log("now ittttt");
        questionPanel.SetActive(true);
        resultPanel.SetActive(false); 

        allQuestions = GameContentDatabase.Instance.GetQuestionsByQuizID(currentQuiz.quiz_id); //
        Debug.Log(allQuestions);
        currentQuestionIndex = 0;
        correctAnswersCount = 0;
        experienceAll = 0;
        GoldAll = 0;

        ShowQuestion();
    }

    private void ShowQuestion()
    {
        ResetColors();
        // เช็คว่าคำถามหมดหรือยัง
        Debug.Log(allQuestions.Count);
        if (currentQuestionIndex >= allQuestions.Count)
        {
            ShowQuizResults();
            return;
        }

        QuestionData q = allQuestions[currentQuestionIndex]; //ดึงคำถามปัจจุบัน
        Debug.Log(q);
        questionText.text = q.questionText; //ใส่โจทย์
        Debug.Log(q.questionText);

        for (int i = 0; i < answerButtons.Count && i < answerButtonTexts.Count; i++)
        {
            if (i < q.answerOptions.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtonTexts[i].text = q.answerOptions[i]; 
                int answerIndex = i; 
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ฟังชันตรวจคำตอบ
    private void OnAnswerSelected(int selectedIndex)
    {
        QuestionData q = allQuestions[currentQuestionIndex];
        if (selectedIndex == q.correctAnswerIndex) //
        {
            correctAnswersCount++;
            // เน้นปุ่มสีเขียวถ้าตอบถูก
            answerButtons[selectedIndex].GetComponent<Image>().color = new Color32(0, 255, 0, 128);
            //เปลี่ยนขอบเขตปุ่มเป็นสีเขียว
            //answerButtons[selectedIndex].GetComponent<Outline>().effectColor = Color.green;
        }
        else
        {
            // เน้นปุ่มสีแดงถ้าตอบผิด
            answerButtons[selectedIndex].GetComponent<Image>().color = new Color32(255, 0, 0, 128);
            //เปลี่ยนขอบเขตปุ่มเป็นสีแดง
            //answerButtons[selectedIndex].GetComponent<Outline>().effectColor = Color.red;
        }
        currentQuestionIndex++;
        // สั่งให้รอ 3 วินาที แล้วค่อยไปเรียกฟังก์ชัน ShowQuestion()
        Invoke("ShowQuestion", 3f);
    }

    private void CollectColersButton()
    {
        foreach (Button btn in answerButtons)
        {
            Color btnColor = btn.GetComponent<Image>().color;
            colorButton.Add(btnColor);
        }
    }

    // private void CollectOutlineButton()
    // {
    //     foreach (Button btn in answerButtons)
    //     {
    //         Outline outline = btn.GetComponent<Outline>();
    //         outlineButton.Add(outline);
    //     }
    // }

    private void ResetColors()
    {
        foreach (Button btn in answerButtons)
        {
            btn.GetComponent<Image>().color = colorButton[answerButtons.IndexOf(btn)];
            //btn.GetComponent<Outline>().effectColor = outlineButton[answerButtons.IndexOf(btn)].effectColor;
        }
    }

    private void ShowQuizResults()
    {
        questionPanel.SetActive(false);
        resultPanel.SetActive(true); 

        // 1. คำนวณดาว (ตามที่คุณเคยบอกว่ามี 5 ข้อ)
        int stars = 0;
        if (correctAnswersCount >= 5) stars = 3;
        else if (correctAnswersCount == 4) stars = 2;
        else if (correctAnswersCount == 3) stars = 1;
        
        resultScoreText.text = $"You got {correctAnswersCount} / {allQuestions.Count} correct!\nStars: {stars}";

        // 2. บันทึกผลลัพธ์ลง GameManager
        GameManager.Instance.UpdateQuizProgress(currentQuiz.quiz_id, correctAnswersCount, true);

        // 3. (สำคัญ) แสดง "ของรางวัล"
        DisplayRewards(stars);
    }

    private void DisplayRewards(int starsAchieved)
    {
        // ล้างแถวรางวัลเก่า
        foreach (Transform child in rewardListContainer)
        {
            Destroy(child.gameObject);
        }

        // ดึง "ของรางวัล" ทั้งหมดของ Quiz นี้
        List<RewardData> rewards = GameContentDatabase.Instance.GetRewardByQuizID(currentQuiz.quiz_id);

        foreach (RewardData reward in rewards)
        {
            //ผ่านเกณฑ์ดาวหรือไม่ เช่น ทำได้สามดาว ก็จะได้รางวัลของทั้งหมดตัตั้งแต่ 0-3ดาว
            if (reward.starRequired <= starsAchieved && reward.starRequired > 0)
            {
                // ถ้าผ่านเกณฑ์: เช็คว่า "เคยรับ" หรือยัง
                if (GameManager.Instance.HasClaimedReward(reward.reward_id))
                {
                    // เคยรับแล้ว
                    GameObject row = Instantiate(rewardRowPrefab, rewardListContainer);
                    TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                    string rewardDesc = $"Star {reward.starRequired}: "; //
                    if (reward.rewardType == RewardType.Gold) rewardDesc += $"{reward.rewardValue} Gold"; //
                    else if (reward.rewardType == RewardType.Card) rewardDesc += $"Card: {reward.cardReference.cardName}"; //
                    rowText.text = $"<color=grey>{rewardDesc} (Claimed)</color>";
                }
                else
                {
                    // ยังไม่เคยรับ (ให้รางวัลเลย!)
                    GameManager.Instance.ClaimReward(reward.reward_id);
                    if (reward.rewardType == RewardType.Gold) 
                    {
                       GoldAll += reward.rewardValue;
                    }
                    else if (reward.rewardType == RewardType.Card) 
                    {
                        GameManager.Instance.AddCardToInventory(reward.cardReference.card_id, 1);
                    }
                    experienceAll += reward.experiencePoints;

                    GameObject row = Instantiate(rewardRowPrefab, rewardListContainer);
                    TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();
                    string rewardDesc = $"Star {reward.starRequired}: "; //
                    if (reward.rewardType == RewardType.Gold) rewardDesc += $"{reward.rewardValue} Gold"; //
                    else if (reward.rewardType == RewardType.Card) rewardDesc += $"Card: {reward.cardReference.cardName}"; //
                    rowText.text = $"<color=yellow>{rewardDesc} (Received!)</color>";
                }
            }
            else {
                // ดาวไม่ถึง(0 ดาว) ได้แค่ แค่Gold กับ Exp เป็นราสงวัลพื้นฐาน
                if (reward.rewardType == RewardType.Gold) 
                {
                   GoldAll += reward.rewardValue;
                }
                experienceAll += reward.experiencePoints;
            }
        }
        // เพิ่ม Gold กับ Exp ให้ผู้เล่น
        GameManager.Instance.AddExperience(experienceAll);
        GameManager.Instance.AddGold(GoldAll);
        // show UI All
        GameObject rowAll = Instantiate(rewardRowPrefab, rewardListContainer);
        TextMeshProUGUI rowTextAll = rowAll.GetComponentInChildren<TextMeshProUGUI>();
        rowTextAll.text = $"<color=green> Total Received: {GoldAll} Gold , {experienceAll} Exp </color>";

    }

    /// <summary>
    /// ถูกเรียกโดย "nextEventButton" (ปุ่มถัดไปบนหน้าผลลัพธ์)
    /// </summary>
    private void FinishQuiz()
    {
        gameObject.SetActive(false); // ซ่อน QuizPanel
        OnQuizCompleted?.Invoke(); // "ตะโกน" บอกว่าจบแล้ว
    }
}