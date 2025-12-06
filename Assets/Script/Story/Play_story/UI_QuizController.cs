using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class UI_QuizController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject questionPanel; // (Panel ที่มีคำถาม+ปุ่มตอบ)
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<Button> answerButtons;
    [SerializeField] private List<TextMeshProUGUI> answerButtonTexts;
    [SerializeField] private Image delayProgressBar; // แถบเวลา


    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [Header("Reward Containers")]
    // อันเดิมใช้สำหรับ Gold หรือ List ยาวๆ
    [SerializeField] private Transform rewardListContainer;
    // *เพิ่มอันนี้* สำหรับใส่การ์ดที่เป็นตาราง Grid
    [SerializeField] private Transform cardGridContainer; // (ที่ใส่แถวของรางวัล)

    [Header("Prefabs")]
    [SerializeField] private GameObject GoldRowPrefab; // Prefab แถบยาวสำหรับทอง
    [SerializeField] private GameObject CardSlotPrefab; // Prefab สี่เหลี่ยมสำหรับการ์ด
    [SerializeField] private Button nextEventButton;
    // [SerializeField] private TextMeshProUGUI GoldText;
    [SerializeField] private TextMeshProUGUI ExperienceText;
    [SerializeField] private TextMeshProUGUI Star_amount;
    [SerializeField] private TextMeshProUGUI StarText;

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
    private bool isLoadingNextQuestion = false;

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
        questionPanel.SetActive(true);
        resultPanel.SetActive(false);

        allQuestions = GameContentDatabase.Instance.GetQuestionsByQuizID(currentQuiz.quiz_id); //
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
        if (currentQuestionIndex >= allQuestions.Count)
        {
            ShowQuizResults();
            return;
        }

        QuestionData q = allQuestions[currentQuestionIndex]; //ดึงคำถามปัจจุบัน
        questionText.text = q.questionText; //ใส่โจทย

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
        if (isLoadingNextQuestion) return;
        QuestionData q = allQuestions[currentQuestionIndex];
        // เปลี่ยนสีปุ่มตามคำตอบถูกผิด
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

        // Delay
        StartCoroutine(LoadNextQuestionWithDelay(currentQuestionIndex));
    }

    IEnumerator LoadNextQuestionWithDelay(int targetIndex, bool isFinishing = false)
    {
        isLoadingNextQuestion = true;

        // --- แสดง Delay Bar ---
        if (delayProgressBar != null)
        {
            delayProgressBar.gameObject.SetActive(true);
            delayProgressBar.fillAmount = 0;

            float duration = 1.5f; // เก็บเป็นตัวแปรเผื่อแก้ภายหลัง
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                delayProgressBar.fillAmount = timer / duration;
                yield return null; // รอเฟรมถัดไป
            }

            // (Optional) บังคับให้เต็ม 100% ก่อนปิด เผื่อ Frame rate ตกแล้วมันจบที่ 0.99
            delayProgressBar.fillAmount = 1.0f;

            // หน่วงอีกนิดนึง (0.1วิ) ให้คนเห็นว่าเต็มแล้วค่อยปิด (แล้วแต่ชอบ)
            yield return new WaitForSeconds(0.1f);

            delayProgressBar.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        ShowQuestion();

        // ย้ายมาไว้ท้ายสุด เพื่อเปิดรับ input ใหม่
        isLoadingNextQuestion = false;
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

        Star_amount.text = $"{stars}";
        StarText.text = $"Star earned : {stars}";
        resultScoreText.text = $"Score : {correctAnswersCount} / {allQuestions.Count}";

        // 2. บันทึกผลลัพธ์ลง GameManager
        GameManager.Instance.UpdateQuizProgress(currentQuiz.quiz_id, correctAnswersCount, true);

        // 3. (สำคัญ) แสดง "ของรางวัล"
        DisplayRewards(stars);
    }

    // private void DisplayRewards(int starsAchieved)
    // {
    //     // ล้างแถวรางวัลเก่า
    //     foreach (Transform child in rewardListContainer)
    //     {
    //         Destroy(child.gameObject);
    //     }

    //     // ดึง "ของรางวัล" ทั้งหมดของ Quiz นี้
    //     List<RewardData> rewards = GameContentDatabase.Instance.GetRewardByQuizID(currentQuiz.quiz_id);

    //     foreach (RewardData reward in rewards)
    //     {

    //         //ผ่านเกณฑ์ดาวหรือไม่ เช่น ทำได้สามดาว ก็จะได้รางวัลของทั้งหมดตัตั้งแต่ 0-3ดาว
    //         if (reward.starRequired <= starsAchieved && reward.starRequired > 0)
    //         {
    //             // ถ้าผ่านเกณฑ์: เช็คว่า "เคยรับ" หรือยัง
    //             if (GameManager.Instance.HasClaimedReward(reward.reward_id))
    //             {
    //                 // เคยรับแล้ว
    //                 TextMeshProUGUI card_Name = CardPrefab.GetComponentInChildren<TextMeshProUGUI>();
    //                 if (reward.rewardType == RewardType.Gold)
    //                 {
    //                     GoldAll += reward.rewardValue;
    //                     GoldText.text = $"{reward.rewardValue} Gold";
    //                 }
    //                 else if (reward.rewardType == RewardType.Card)
    //                 {
    //                     foreach (var cardItem in reward.cardReference)
    //                     {
    //                         card_Name.text = $"<color=grey>{cardItem.card.cardName} x{cardItem.amount} (Claimed)</color>";
    //                     }
    //                 }
    //             }
    //             else
    //             {
    //                 // ยังไม่เคยรับ (ให้รางวัลเลย!)
    //                 GameManager.Instance.ClaimReward(reward.reward_id);
    //                 if (reward.rewardType == RewardType.Gold)
    //                 {
    //                     GoldAll += reward.rewardValue;
    //                 }
    //                 else if (reward.rewardType == RewardType.Card)
    //                 {
    //                     foreach (var cardItem in reward.cardReference)
    //                     {
    //                         GameManager.Instance.AddCardToInventory(cardItem.card.card_id, cardItem.amount);
    //                     }
    //                 }
    //                 experienceAll += reward.experiencePoints;

    //                 // GameObject row = Instantiate(rewardRowPrefab, rewardListContainer);
    //                 // TextMeshProUGUI rowText = row.GetComponentInChildren<TextMeshProUGUI>();

    //                 TextMeshProUGUI card_Name = CardPrefab.GetComponentInChildren<TextMeshProUGUI>();
    //                 if (reward.rewardType == RewardType.Gold)
    //                 {
    //                     GoldAll += reward.rewardValue;
    //                     GoldText.text = $"{reward.rewardValue} Gold";
    //                 }
    //                 else if (reward.rewardType == RewardType.Card)
    //                 {
    //                     foreach (var cardItem in reward.cardReference)
    //                     {
    //                         card_Name.text = $"<color=yellow>{cardItem.card.cardName} x{cardItem.amount}</color>";
    //                     }
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             // ดาวไม่ถึง(0 ดาว) ได้แค่ แค่Gold กับ Exp เป็นราสงวัลพื้นฐาน
    //             if (reward.rewardType == RewardType.Gold)
    //             {
    //                 GoldAll += reward.rewardValue;
    //                 GoldText.text = $"{reward.rewardValue} Gold";
    //             }
    //             experienceAll += reward.experiencePoints;
    //             ExperienceText.text = $"{reward.experiencePoints}";
    //         }
    //     }
    //     // เพิ่ม Gold กับ Exp ให้ผู้เล่น
    //     GameManager.Instance.AddExperience(experienceAll);
    //     GameManager.Instance.AddGold(GoldAll);
    //     // // show UI All
    //     // GameObject rowAll = Instantiate(rewardRowPrefab, rewardListContainer);
    //     // TextMeshProUGUI rowTextAll = rowAll.GetComponentInChildren<TextMeshProUGUI>();
    //     // rowTextAll.text = $"<color=green> Total Received: {GoldAll} Gold , {experienceAll} Exp </color>";

    // }

    private void DisplayRewards(int starsAchieved)
    {
        // 1. ล้างรายการเก่าทิ้งก่อน
        foreach (Transform child in rewardListContainer) Destroy(child.gameObject);
        foreach (Transform child in cardGridContainer) Destroy(child.gameObject);

        // 2. ดึงของรางวัลมา
        List<RewardData> rewards = GameContentDatabase.Instance.GetRewardByQuizID(currentQuiz.quiz_id);

        //set Prefeb gold และ card
        //Gold
        GameObject newRow = Instantiate(GoldRowPrefab, rewardListContainer);
        TextMeshProUGUI goldText = newRow.GetComponentInChildren<TextMeshProUGUI>();

        foreach (RewardData reward in rewards)
        {
            // ตรวจสอบเงื่อนไขดาว (ถ้า 0 ดาว คือรางวัลปลอบใจที่ได้เสมอ)
            bool isEligible = (reward.starRequired == 0) || (reward.starRequired <= starsAchieved);

            if (!isEligible) continue; // ถ้าไม่ได้รางวัลนี้ ก็ข้ามไป


            // --- ตรวจสอบสถานะการรับ ---
            bool isClaimed = GameManager.Instance.HasClaimedReward(reward.reward_id);
            string statusText = isClaimed ? "<color=grey>(Received)</color>" : "<color=yellow>(New!)</color>";

            // --- กรณีเป็น GOLD ---
            if (reward.rewardType == RewardType.Gold)
            {
                // GameObject newRow = Instantiate(GoldRowPrefab, rewardListContainer);
                // TextMeshProUGUI goldText = newRow.GetComponentInChildren<TextMeshProUGUI>();

                goldText.text = $"Gold : {reward.rewardValue} {statusText}";

                // ซ่อนรูปการ์ด (เพราะเป็นทอง) -> พื้นหลังจะหดเล็กลงเองเพราะ ContentSizeFitter
                // if (cardImageDisplay != null) cardImageDisplay.gameObject.SetActive(false);

                // แจกรางวัล (Logic)
                if (!isClaimed)
                {
                    GameManager.Instance.ClaimReward(reward.reward_id);
                    GoldAll += reward.rewardValue;
                }
            }
            // --- กรณีเป็น CARD ---
            else if (reward.rewardType == RewardType.Card)
            {
                if (reward.cardReference != null)
                {
                    foreach (var cardItem in reward.cardReference)
                    {
                        // สร้าง Slot การ์ดลงใน Grid Container
                        GameObject newCard = Instantiate(CardSlotPrefab, cardGridContainer);

                        // ดึงรูปมาแสดง
                        Image icon = newCard.transform.Find("cardPic")?.GetComponent<Image>();
                        TextMeshProUGUI nameText = newCard.GetComponentInChildren<TextMeshProUGUI>();
                        if (icon != null) icon.sprite = cardItem.card.artwork; // ใส่รูปการ์ด
                        if (nameText != null) nameText.text = $"{cardItem.card.cardName} x{cardItem.amount} {statusText}"; else nameText.text = "";
                        if (!isClaimed)
                        {
                            // (ตัวอย่าง) สั่ง Add การ์ดเข้ากระเป๋า
                            GameManager.Instance.AddCardToInventory(cardItem.card.card_id, cardItem.amount);
                        }
                    }

                    // Mark ว่ารับรางวัลชิ้นใหญ่นี้ไปแล้ว
                    if (!isClaimed) GameManager.Instance.ClaimReward(reward.reward_id);
                }

                // // แจกรางวัล (Logic)
                // if (!isClaimed && reward.cardReference != null)
                // {
                //     GameManager.Instance.ClaimReward(reward.reward_id);
                // }
            }

            // เพิ่ม EXP เสมอ
            if (!isClaimed)
            {
                experienceAll += reward.experiencePoints;
            }
        }

        // อัปเดต Text สรุปผลรวมด้านนอก
        if (goldText != null) goldText.text = $"Gold :{GoldAll}";
        if (ExperienceText != null) ExperienceText.text = $"Experience :{experienceAll}";

        // บันทึกผลรวม
        GameManager.Instance.AddExperience(experienceAll);
        GameManager.Instance.AddGold(GoldAll);
    }

    /// ถูกเรียกโดย "nextEventButton" (ปุ่มถัดไปบนหน้าผลลัพธ์)
    private void FinishQuiz()
    {
        gameObject.SetActive(false); // ซ่อน QuizPanel
        OnQuizCompleted?.Invoke(); // "ตะโกน" บอกว่าจบแล้ว
    }
}