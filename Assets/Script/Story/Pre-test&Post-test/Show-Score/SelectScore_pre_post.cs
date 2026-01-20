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

    [SerializeField] private List<GameObject> posttestMatchingSlots;
    [SerializeField] private List<TextMeshProUGUI> posttestMatchingQustion;
    [SerializeField] private List<TextMeshProUGUI> posttestMatchingAnswer;
    [SerializeField] private List<TextMeshProUGUI> posttestMatchingDescriptionText;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    public void LoadAll()
    {
        LoadPretestScore();
        LoadPosttestScore();
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

        // ดึงผลคะแนน Post-test ของเรื่องทีในหน้านั้น
        var postTestScore = GameManager.Instance.CurrentGameData.postTestResults
            .FirstOrDefault(score => score.story_id == GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId);

        if (postTestScore != null)
        {
            contenier_Posttest.gameObject.SetActive(true);
            scoreText_TotalScore_Posttest.text = $"{postTestScore.TotalScore} / {postTestScore.MaxScore}";

            if (postTestScore.Qustion_Answers != null)
            {
                // ดึงคำถาม คำตอบ และ คะแนน
                var answers = postTestScore.Qustion_Answers;

                // แยกคำถามตามประเภท
                var trueFalse_post = answers.Where(qa => qa.TypeQustion == TypeQustion.TrueFalse).ToList();
                var fillBlank_post = answers.Where(qa => qa.TypeQustion == TypeQustion.FillBlank).ToList();
                var matching_post = answers.Where(qa => qa.TypeQustion == TypeQustion.Matching).ToList();

                // แสดงคะแนน
                scoreText_TrueFalse_post.text = $"{trueFalse_post.Sum(qa => qa.score)} / {trueFalse_post.Count}";
                scoreText_FillBlank_post.text = $"{fillBlank_post.Sum(qa => qa.score)} / {fillBlank_post.Count}";
                scoreText_Matching_post.text = $"{matching_post.Sum(qa => qa.score)} / {matching_post.Count}";

                // แสดงรายการคำถามและคำตอบ
                QustionText_truefalse.text = string.Join("\n\n", trueFalse_post.Select((qa, i) => $"{qa.QustionText}"));
                AnswerText_truefalse.text = string.Join("\n\n", trueFalse_post.Select(qa =>
                $"<color={(qa.score > 0 ? "green" : "red")}>{qa.AnswerText}</color>"
                ));

                QustionText_fillblank.text = string.Join("\n\n", fillBlank_post.Select((qa, i) => $"{qa.QustionText}"));
                AnswerText_fillblank.text = string.Join("\n\n", fillBlank_post.Select(qa =>
                $"<color={(qa.score > 0 ? "green" : "red")}>{qa.AnswerText}</color>"
                ));

                // ถ้ามี Matching
                // QustionText_matching.text = string.Join("\n\n", matching_post.Select((qa, i) => $"{i + 1}. {qa.QustionText}"));
                // AnswerText_matching.text = string.Join("\n\n", matching_post.Select(qa => qa.AnswerText));
                UpdateMatchingSlots(matching_post, posttestMatchingSlots, posttestMatchingQustion, posttestMatchingAnswer);

                // อัปเดตคำอธิบาย
                SelectDescription_TrueFalse_FillInBlank(trueFalse_post, TypeQustion.TrueFalse, DescriptionText_truefalse);
                SelectDescription_TrueFalse_FillInBlank(fillBlank_post, TypeQustion.FillBlank, DescriptionText_fillblank);
                SelectDescription_Matching(matching_post, TypeQustion.Matching, posttestMatchingDescriptionText);
            }
        }
        else
        {
            scoreText_TotalScore_Posttest.text = "<color=red>ยังไม่ได้ทำแบบทดสอบ</color>";
            contenier_Posttest.gameObject.SetActive(false);
        }
    }

    private void SelectDescription_TrueFalse_FillInBlank(List<Qustion_Answer> Qustion_Answers, TypeQustion type, TextMeshProUGUI descriptionText)
    {
        // คำอธิบาย TrueFalse
        if (type == TypeQustion.TrueFalse)
        {
            // ดึงคำอธิบายทั้งหมดของ TrueFalse ในเstoryมา
            var story_id = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
            var trueFalseDescriptions = GameContentDatabase.Instance.GetTrueFalseDescriptionByID(story_id);

            //  หาคำอธิบายที่ตรงกับคำถาม
            var trueFalseDes_Question = trueFalseDescriptions
                .Where(desc => Qustion_Answers
                .Any(qa => qa.QustionText == desc.TrueFalse.statement))
                .ToList();

            // แสดงคำอธิบาย ที่ตรงกับคำตอบของผู้เล่น
            // var UserAnswer = Qustion_Answers.AnsAnswerText == "True" ? true : false;
            for (int i = 0, len = trueFalseDes_Question.Count; i < len; i++)
            {
                var description = trueFalseDes_Question[i];
                for (int j = 0, len2 = Qustion_Answers.Count; j < len2; j++)
                {
                    var Qustion = Qustion_Answers[j];
                    if (Qustion.QustionText == description.TrueFalse.statement &&
                     Qustion.AnswerText == description.UserAnswertext)
                    {
                        descriptionText.text = $"{description.DescriptionContext}";
                    }
                }
            }
        }

        // คำอธิบาย FillInBlank
        if (type == TypeQustion.FillBlank)
        {
            // ดึงคำอธิบายทั้งหมดของ FillBlank ในเstoryมา
            var story_id = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
            var fillBlankDescriptions = GameContentDatabase.Instance.GetFillInBlankDescriptionByID(story_id);

            //  หาคำอธิบายที่ตรงกับคำถาม
            var fillBlankDes_Question = fillBlankDescriptions
                .Where(desc => Qustion_Answers.Any(qa =>
                {
                    // 1. สร้าง String จากฝั่ง Description ขึ้นมาใหม่ ให้ Format เหมือนกับ qa เป๊ะๆ
                    string generatedDescText = string.Join("\n", desc.FillInBlank.sentences.Select(s =>
                        string.IsNullOrEmpty(s.sentencePart2)
                            ? s.sentencePart1 + " _____ "                       // กรณีไม่มีส่วนหลัง
                            : s.sentencePart1 + " _____ " + s.sentencePart2     // กรณีมีส่วนหลัง
                    ));

                    // 2. เอามาเทียบกัน
                    return qa.QustionText == generatedDescText;
                }))
                .ToList();

            // แสดงคำอธิบาย ที่ตรงกับคำตอบของผู้เล่น
            for (int i = 0, len = fillBlankDes_Question.Count; i < len; i++)
            {
                var description = fillBlankDes_Question[i];
                string generatedStatement = string.Join("\n", description.FillInBlank.sentences.Select(s =>
                    string.IsNullOrEmpty(s.sentencePart2)
                        ? s.sentencePart1 + " _____ "
                        : s.sentencePart1 + " _____ " + s.sentencePart2
                ));
                for (int j = 0, len2 = Qustion_Answers.Count; j < len2; j++)
                {
                    var Qustion = Qustion_Answers[j];
                    if (Qustion.QustionText == generatedStatement &&
                     Qustion.AnswerText == description.UserAnswertext)
                    {
                        descriptionText.text = $"{description.DescriptionContext}";
                    }
                }
            }
        }
    }

    private void SelectDescription_Matching(List<Qustion_Answer> Qustion_Answers, TypeQustion type, List<TextMeshProUGUI> descriptionTextList)
    {
        if (type != TypeQustion.Matching) return;

        // 1. ดึงคำอธิบายทั้งหมดของ Matching ใน Story มาเตรียมไว้
        var story_id = GameManager.Instance.CurrentGameData.selectedStory.lastSelectedStoryId;
        var allMatchingDescriptions = GameContentDatabase.Instance.GetMatchingDescriptionByID(story_id);

        // 2. วนลูปตาม "รายการคำถามที่ส่งมา" (เพื่อให้ Index ตรงกับ UI Slot)
        for (int i = 0; i < Qustion_Answers.Count; i++)
        {
            // กันเหนียว: ถ้าจำนวนคำถาม มากกว่าจำนวนช่อง UI Text ที่มี ให้หยุด
            if (i >= descriptionTextList.Count) break;

            var currentQA = Qustion_Answers[i];

            // เคลียร์ข้อความเก่าออกก่อน
            descriptionTextList[i].text = "";

            // 3. จัดการเรื่อง String (ตัดคำว่า Matching: ออก เพื่อให้เทียบกับ Database ได้)
            // ต้องทำให้ format เหมือนกับที่เก็บใน Database เป๊ะๆ
            string cleanQuestionText = currentQA.QustionText.Replace("Matching: ", "").Trim();

            // 4. ค้นหาว่าคำถามนี้ + คำตอบนี้ มี Description ไหม
            var targetDesc = allMatchingDescriptions.FirstOrDefault(desc =>
                desc.Question_of_Matching == cleanQuestionText &&   // เทียบโจทย์ (ที่ตัดคำนำหน้าออกแล้ว)
                desc.UserAnswertext == currentQA.AnswerText         // เทียบคำตอบ
            );

            // 5. ถ้าเจอ ให้แสดงผลในช่องที่ i
            if (targetDesc != null)
            {
                descriptionTextList[i].text = $"{targetDesc.DescriptionContext}\n\n";
            }
        }
    }
}