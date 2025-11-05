using UnityEngine;

[CreateAssetMenu(fileName = "New Qustion" ,menuName = "Game Content/Question")]

public class QuestionData : ScriptableObject
{
    public int question_id;
    public string questionText;
    public QuizData quiz;
    public int questionOrder;
    public string[] answerOptions; // ตัวเลือกคำตอบ
    public int correctAnswerIndex;

}