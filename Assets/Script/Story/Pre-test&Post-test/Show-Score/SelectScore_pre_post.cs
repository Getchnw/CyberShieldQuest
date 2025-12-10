using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class SelectScore_pre_post : MonoBehaviour
{
    [Header("Pretest")]
    [SerializeField] private TextMeshProUGUI scoreText_TrueFalse;
    [SerializeField] private TextMeshProUGUI scoreText_FillBlank;
    [SerializeField] private TextMeshProUGUI scoreText_Matching;
    [SerializeField] private TextMeshProUGUI scoreText_TotalScore_Pretest;
    [SerializeField] private Transform contenier_Pretest;

    [Header("Posttest")]
    [SerializeField] private Transform contenier_Posttest;
    [SerializeField] private TextMeshProUGUI scoreText_TotalScore_Posttest;
    [SerializeField] private TextMeshProUGUI scoreText_TrueFalse_post;
    [SerializeField] private TextMeshProUGUI scoreText_FillBlank_post;
    [SerializeField] private TextMeshProUGUI scoreText_Matching_post;

    [Header("Question")]
    [SerializeField] private TextMeshProUGUI QustionText_truefalse;
    [SerializeField] private TextMeshProUGUI QustionText_fillblank;
    // [SerializeField] private TextMeshProUGUI QustionText_matching;
    [Header("Answer")]
    [SerializeField] private TextMeshProUGUI AnswerText_truefalse;
    [SerializeField] private TextMeshProUGUI AnswerText_fillblank;
    // [SerializeField] private TextMeshProUGUI AnswerText_matching;
    [Header("Description")]
    [SerializeField] private TextMeshProUGUI DescriptionText_truefalse;
    [SerializeField] private TextMeshProUGUI DescriptionText_fillblank;
    // [SerializeField] private TextMeshProUGUI DescriptionText_matching;

    [SerializeField] private List<GameObject> posttestMatchingSlots;
    [SerializeField] private List<TextMeshProUGUI> posttestMatchingQustion;
    [SerializeField] private List<TextMeshProUGUI> posttestMatchingAnswer;
    // [SerializeField] private List
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    // private void OnEnable()
    // {
    //     LoadPretestScore();
    //     LoadPosttestScore();
    // }

    public void LoadAll()
    {
        LoadPretestScore();
        LoadPosttestScore();
        Debug.Log("Load All Scores");
    }

    private void UpdateMatchingSlots(List<Qustion_Answer> matchingList, List<GameObject> uiSlots,
     List<TextMeshProUGUI> uiQuestions, List<TextMeshProUGUI> uiAnswers) // เปลี่ยนชื่อ Parameter ให้สั้นลงจะได้ไม่งง
    {
        // 1. เช็ค Null กันเหนียว
        if (uiSlots == null || uiQuestions == null || uiAnswers == null) return;

        // 2. วนลูป
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < matchingList.Count)
            {
                // เปิดกล่อง
                uiSlots[i].gameObject.SetActive(false);

                // ดึงข้อมูล
                var item = matchingList[i];

                // ✅ แก้ไขตรงนี้: เรียกผ่าน List ที่ส่งเข้ามา (และใช้ .Replace เพื่อตัดคำว่า Matching ออก)
                uiQuestions[i].text = item.QustionText.Replace("Matching: ", string.Empty);
                uiAnswers[i].text = item.AnswerText;

                // เปลี่ยนสี
                bool isCorrect = item.score > 0;
                uiAnswers[i].color = isCorrect ? correctColor : wrongColor;
            }
            else
            {
                // ปิดกล่องที่เหลือ
                uiSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void LoadPretestScore()
    {
        var preTestScore = GameManager.Instance.CurrentGameData.preTestResults
            .FirstOrDefault(score => score.story_id == GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);

        // เพิ่มคะแนนTotalscore
        if (preTestScore != null)
        {
            Debug.Log("in preTestScore != null");
            contenier_Pretest.gameObject.SetActive(true);
            scoreText_TotalScore_Pretest.text = $"{preTestScore.TotalScore.ToString()} / {preTestScore.MaxScore.ToString()}";

            if (preTestScore.Qustion_Answers != null)
            {
                Debug.Log("in preTestScore.Qustion_Answers  != null");
                var answers = preTestScore.Qustion_Answers;
                var trueFalse_pre = answers.Where(qa => qa.TypeQustion == TypeQustion.TrueFalse).ToList();
                var fillBlank_pre = answers.Where(qa => qa.TypeQustion == TypeQustion.FillBlank).ToList();
                var matching_pre = answers.Where(qa => qa.TypeQustion == TypeQustion.Matching).ToList();

                // ✅ ลบ ?? 0 ออก เพราะ .Sum() ไม่คืนค่า null
                scoreText_TrueFalse.text = $"{trueFalse_pre.Sum(qa => qa.score)} / {trueFalse_pre.Count}";
                scoreText_FillBlank.text = $"{fillBlank_pre.Sum(qa => qa.score)} / {fillBlank_pre.Count}";
                scoreText_Matching.text = $"{matching_pre.Sum(qa => qa.score)} / {matching_pre.Count}";
            }
            else
            {
                Debug.Log("preTestScore.Qustion_Answers is null");
                Debug.Log(preTestScore.TotalScore);
                Debug.Log(preTestScore.MaxScore);
                Debug.Log(preTestScore.Qustion_Answers);

            }
        }
        else
        {
            scoreText_TotalScore_Pretest.text = "<color=red>ยังไม่ได้ทำแบบทดสอบ</color>";
            contenier_Pretest.gameObject.SetActive(false);
        }

        // // ดึงคะแนนแยกประเภทคำถาม
        // var trueFalse_pre = preTestScore?.Qustion_Answersss
        //     .Where(qa => qa.TypeQustion == TypeQustion.TrueFalse);
        // var fillBlank_pre = preTestScore?.Qustion_Answers
        //     .Where(qa => qa.TypeQustion == TypeQustion.FillBlank);
        // var matching_pre = preTestScore?.Qustion_Answers
        //     .Where(qa => qa.TypeQustion == TypeQustion.Matching);
        // // เพิ่มคะแนนแยกประเภทคำถาม
        // var trueFalseScore = trueFalse_pre.Sum(qa => qa.score);
        // var fillBlankScore = fillBlank_pre.Sum(qa => qa.score);
        // var matchingScore = matching_pre.Sum(qa => qa.score);

        // scoreText_TrueFalse.text = $"{trueFalseScore} / {preTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.TrueFalse).Count()}";
        // scoreText_FillBlank.text = $"{fillBlankScore} / {preTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.FillBlank).Count()}";
        // scoreText_Matching.text = $"{matchingScore} / {preTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.Matching).Count()}";
    }

    // private void LoadPosttestScore()
    // {
    //     var postTestScore = GameManager.Instance.CurrentGameData.postTestResults
    //         .FirstOrDefault(score => score.story_id == GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);
    //     // เพิ่มคะแนนTotalscore
    //     if (postTestScore != null)
    //     {
    //         scoreText_TotalScore_Posttest.text = $"{postTestScore.TotalScore.ToString()} / {postTestScore.Maxscore.ToString()}";
    //     }
    //     else
    //     {
    //         scoreText_TotalScore_Posttest.text = "<color=red>ยังไม่ได้ทำแบบทดสอบ</color>";
    //         contenier_Posttest.gameObject.SetActive(false);
    //     }

    //     // ดึงคะแนนแยกประเภทคำถาม
    //     var trueFalse_post = postTestScore?.Qustion_Answers
    //         .Where(qa => qa.TypeQustion == TypeQustion.TrueFalse);
    //     var fillBlank_post = postTestScore?.Qustion_Answers
    //         .Where(qa => qa.TypeQustion == TypeQustion.FillBlank);
    //     var matching_post = postTestScore?.Qustion_Answers
    //         .Where(qa => qa.TypeQustion == TypeQustion.Matching);
    //     // เพิ่มคะแนนแยกประเภทคำถาม
    //     var trueFalseScore_post = trueFalse_post.Sum(qa => qa.score) ?? 0;
    //     var fillBlankScore_post = pfillBlank_post.Sum(qa => qa.score) ?? 0;
    //     var matchingScore_post = matching_post.Sum(qa => qa.score) ?? 0;

    //     scoreText_TrueFalse_post.text = $"{trueFalseScore_post} / {postTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.TrueFalse).Count()}";
    //     scoreText_FillBlank_post.text = $"{fillBlankScore_post} / {postTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.FillBlank).Count()}";
    //     scoreText_Matching_post.text = $"{matchingScore_post} / {postTestScore?.Qustion_Answers.Where(qa => qa.TypeQustion == TypeQustion.Matching).Count()}";

    //     // แสดงคำถามและคำตอบของแต่ละประเภทคำถาม
    //     QustionText_truefalse.text = string.Join("\n", trueFalse_post.Select(qa => qa.QustionText));
    //     AnswerText_truefalse.text = string.Join("\n", trueFalse_post.Select(qa => qa.AnswerText));
    //     // DescriptionText_truefalse.text = string.Join("\n", trueFalse_post.Select(qa => qa.Description));

    //     QustionText_fillblank.text = string.Join("\n", fillBlank_post.Select(qa => qa.QustionText));
    //     AnswerText_fillblank.text = string.Join("\n", fillBlank_post.Select(qa => qa.AnswerText));
    //     // DescriptionText_fillblank.text = string.Join("\n", fillBlank_post.Select(qa => qa.Description));

    //     QustionText_matching.text = string.Join("\n", matching_post.Select(qa => qa.QustionText));
    //     AnswerText_matching.text = string.Join("\n", matching_post.Select(qa => qa.AnsAnswerText));
    //     // DescriptionText_matching.text = string.Join("\n", matching_post.Select(qa => qa.Description));

    // }

    private void LoadPosttestScore()
    {
        if (GameManager.Instance.CurrentGameData == null) return;

        var postTestScore = GameManager.Instance.CurrentGameData.postTestResults
            .FirstOrDefault(score => score.story_id == GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);

        if (postTestScore != null)
        {
            contenier_Posttest.gameObject.SetActive(true);
            scoreText_TotalScore_Posttest.text = $"{postTestScore.TotalScore} / {postTestScore.MaxScore}";

            if (postTestScore.Qustion_Answers != null)
            {
                var answers = postTestScore.Qustion_Answers;

                var trueFalse_post = answers.Where(qa => qa.TypeQustion == TypeQustion.TrueFalse).ToList();
                var fillBlank_post = answers.Where(qa => qa.TypeQustion == TypeQustion.FillBlank).ToList();
                var matching_post = answers.Where(qa => qa.TypeQustion == TypeQustion.Matching).ToList();

                scoreText_TrueFalse_post.text = $"{trueFalse_post.Sum(qa => qa.score)} / {trueFalse_post.Count}";
                scoreText_FillBlank_post.text = $"{fillBlank_post.Sum(qa => qa.score)} / {fillBlank_post.Count}";
                scoreText_Matching_post.text = $"{matching_post.Sum(qa => qa.score)} / {matching_post.Count}";

                // แสดงรายการคำถามและคำตอบ
                QustionText_truefalse.text = string.Join("\n\n", trueFalse_post.Select((qa, i) => $"{i + 1}. {qa.QustionText}"));
                AnswerText_truefalse.text = string.Join("\n\n", trueFalse_post.Select(qa =>
                $"<color={(qa.score > 0 ? "green" : "red")}>{qa.AnswerText}</color>"
                ));

                QustionText_fillblank.text = string.Join("\n\n", fillBlank_post.Select((qa, i) => $"{i + 1}. {qa.QustionText}"));
                AnswerText_fillblank.text = string.Join("\n\n", fillBlank_post.Select(qa =>
                $"<color={(qa.score > 0 ? "green" : "red")}>{qa.AnswerText}</color>"
                ));

                // ถ้ามี Matching
                // QustionText_matching.text = string.Join("\n\n", matching_post.Select((qa, i) => $"{i + 1}. {qa.QustionText}"));
                // AnswerText_matching.text = string.Join("\n\n", matching_post.Select(qa => qa.AnswerText));
                UpdateMatchingSlots(matching_post, posttestMatchingSlots, posttestMatchingQustion, posttestMatchingAnswer);
            }
        }
        else
        {
            scoreText_TotalScore_Posttest.text = "<color=red>ยังไม่ได้ทำแบบทดสอบ</color>";
            contenier_Posttest.gameObject.SetActive(false);
        }
    }

    // private void SelectDescription()
    // {

    // }
}