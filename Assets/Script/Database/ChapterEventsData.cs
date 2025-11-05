using UnityEngine;

[CreateAssetMenu(fileName = "New ChapterEvent" ,menuName = "Game Content/ChapterEvent")]

public class ChapterEventsData : ScriptableObject
{
   public enum EventType
   {
      Dialogue,
      Quiz
   }

   [Header("Identifiers")]
   public int event_id;
   public int eventOrder;
   public EventType type;
   public ChapterData chapter;

   [Header("References")]
   // Assign the appropriate reference depending on the EventType
   public DialogsceneData dialogueReference;
   public QuizData quizReference;

   /// <summary>
   /// Returns the active reference object based on the selected EventType.
   /// </summary>
   public ScriptableObject GetActiveReference()
   {
      return (type == EventType.Dialogue) ? (ScriptableObject)dialogueReference : (ScriptableObject)quizReference;
   }
}


