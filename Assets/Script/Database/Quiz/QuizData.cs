using UnityEngine;

[CreateAssetMenu(fileName = "New Quiz" ,menuName = "Game Content/Quiz")]

public class QuizData : ScriptableObject
{
    public int quiz_id;
    // sprite -> Sprite, fix typo in name
    public Sprite backgroundQuiz;
    // use camelCase and plural for clarity
    public int totalQuestions;
}


