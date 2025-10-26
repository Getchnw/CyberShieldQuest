using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameContentDatabase : MonoBehaviour
{
    public static GameContentDatabase Instance { get; private set; }

    // 1. "ย้าย" คลังข้อมูลทั้งหมดมาไว้ที่นี่
    [Header("Game Content Databases")]
    //ข้อมูลการ์ด
    [SerializeField] private List<CardData> cardDatabase = new List<CardData>();
    // ข้อมูลเนื้อเรื่อง
    [SerializeField] private List<StoryData> storyDatabase = new List<StoryData>();
    [SerializeField] private List<ChapterData> chapterDatabase = new List<ChapterData>();
    [SerializeField] private List<ChapterEventsData> chapterEventsDatabase = new List<ChapterEventsData>();
    [SerializeField] private List<DialogsceneData> dialogsceneDatabase = new List<DialogsceneData>();
    [SerializeField] private List<DialogueLinesData> dialogueLinesDatabase = new List<DialogueLinesData>();
    // ข้อมูล Quiz
    [SerializeField] private List<QuizData> quizDatabase = new List<QuizData>();
    [SerializeField] private List<QuestionData> questionDatabase = new List<QuestionData>();
    //[SerializeField] private List<AnswerData> answerDatabase = new List<AnswerData>();
    [SerializeField] private List<RewardData> rewardDatabase = new List<RewardData>();
    // ข้อมูลด่าน
    //[SerializeField] private List<StageData> stageDatabase = new List<StageData>();

    void Awake()
    {
        // --- โค้ด Singleton ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //ตัวโหลด
            AutoLoadGameContentIfEmpty();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 2. เมธอด AutoLoad LoadData จากโฟลเดอร์ Resources/GameContent/
    private void AutoLoadGameContentIfEmpty()
    {
        // โหลด cardDatabase
        if (cardDatabase == null || cardDatabase.Count == 0)
        {
            var loadedCards = Resources.LoadAll<CardData>("GameContent/Cards");
            if (loadedCards != null && loadedCards.Length > 0)
            {
                cardDatabase = new List<CardData>(loadedCards);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    cardDatabase = new List<CardData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);
                        if (asset != null) cardDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {cardDatabase.Count} CardData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load CardData failed: {e.Message}");
            }
            #endif
        }
        // โหลด storyDatabase
        if (storyDatabase == null || storyDatabase.Count == 0)
        {
            var loadedStories = Resources.LoadAll<StoryData>("GameContent/Stories");
            if (loadedStories != null && loadedStories.Length > 0)
            {
                storyDatabase = new List<StoryData>(loadedStories);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StoryData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    storyDatabase = new List<StoryData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StoryData>(path);
                        if (asset != null) storyDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {storyDatabase.Count} StoryData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load StoryData failed: {e.Message}");
            }
            #endif
        }
        // โหลด chapterDatabase
        if (chapterDatabase == null || chapterDatabase.Count == 0)
        {
            var loadedChapters = Resources.LoadAll<ChapterData>("GameContent/Chapters");
            if (loadedChapters != null && loadedChapters.Length > 0)
            {
                chapterDatabase = new List<ChapterData>(loadedChapters);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ChapterData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    chapterDatabase = new List<ChapterData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ChapterData>(path);
                        if (asset != null) chapterDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {chapterDatabase.Count} ChapterData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load ChapterData failed: {e.Message}");
            }
            #endif
        }
        // โหลด dialogsceneDatabase
        if (dialogsceneDatabase == null || dialogsceneDatabase.Count == 0)
        {
            var loadedDialogscenes = Resources.LoadAll<DialogsceneData>("GameContent/Dialogscenes");
            if (loadedDialogscenes != null && loadedDialogscenes.Length > 0)
            {
                dialogsceneDatabase = new List<DialogsceneData>(loadedDialogscenes);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DialogsceneData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    dialogsceneDatabase = new List<DialogsceneData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogsceneData>(path);
                        if (asset != null) dialogsceneDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {dialogsceneDatabase.Count} DialogsceneData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load DialogsceneData failed: {e.Message}");
            }
            #endif
        }
        // โหลด dialogueLinesDatabase
        if (dialogueLinesDatabase == null || dialogueLinesDatabase.Count == 0)
        {
            var loadedDialogueLines = Resources.LoadAll<DialogueLinesData>("GameContent/DialogueLines");
            if (loadedDialogueLines != null && loadedDialogueLines.Length > 0)
            {
                dialogueLinesDatabase = new List<DialogueLinesData>(loadedDialogueLines);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DialogueLinesData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    dialogueLinesDatabase = new List<DialogueLinesData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogueLinesData>(path);
                        if (asset != null) dialogueLinesDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {dialogueLinesDatabase.Count} DialogueLinesData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load DialogueLinesData failed: {e.Message}");
            }
            #endif
        }
        // โหลด chapterEventsDatabase
        if (chapterEventsDatabase == null || chapterEventsDatabase.Count == 0)
        {
            var loadedEvents = Resources.LoadAll<ChapterEventsData>(""); 
            if (loadedEvents != null && loadedEvents.Length > 0)
            {
                chapterEventsDatabase = new List<ChapterEventsData>(loadedEvents);
            }
            // (ใส่ #if UNITY_EDITOR... เหมือนเดิม)
             #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ChapterEventsData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    chapterEventsDatabase = new List<ChapterEventsData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ChapterEventsData>(path);
                        if (asset != null) chapterEventsDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {chapterEventsDatabase.Count} ChapterEventsData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load ChapterEventsData failed: {e.Message}");
            }
            #endif
        }
        
        // โหลด quizDatabase
        if (quizDatabase == null || quizDatabase.Count == 0)
        {
            var loadedQuizzes = Resources.LoadAll<QuizData>("GameContent/Quizzes");
            if (loadedQuizzes != null && loadedQuizzes.Length > 0)
            {
                quizDatabase = new List<QuizData>(loadedQuizzes);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:QuizData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    quizDatabase = new List<QuizData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<QuizData>(path);
                        if (asset != null) quizDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {quizDatabase.Count} QuizData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load QuizData failed: {e.Message}");
            }
            #endif
        }

        // โหลด questionDatabase
        if (questionDatabase == null || questionDatabase.Count == 0)
        {
            var loadedQuestions = Resources.LoadAll<QuestionData>("GameContent/Questions");
            if (loadedQuestions != null && loadedQuestions.Length > 0)
            {
                questionDatabase = new List<QuestionData>(loadedQuestions);
            }
            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:QuestionData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    questionDatabase = new List<QuestionData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<QuestionData>(path);
                        if (asset != null) questionDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {questionDatabase.Count} QuestionData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load QuestionData failed: {e.Message}");
            }
            #endif
        }

        // โหลด rewardDatabase
        if (rewardDatabase == null || rewardDatabase.Count == 0)
        {
            var loadedRewards = Resources.LoadAll<RewardData>("GameContent/Rewards");
            if (loadedRewards != null && loadedRewards.Length > 0)
            {
                rewardDatabase = new List<RewardData>(loadedRewards);
            }

            #if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RewardData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    rewardDatabase = new List<RewardData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<RewardData>(path);
                        if (asset != null) rewardDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {rewardDatabase.Count} RewardData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load RewardData failed: {e.Message}");
            }
            #endif
        }

        //stageDatabase
    }

    //ฟังก์ชันดึงข้อมูลตาม ID ต่างๆ
    public CardData GetCardByID(string card_id)
    {
        if (cardDatabase == null) return null;
        return cardDatabase.FirstOrDefault(card => card.card_id == card_id);
    }

    public QuizData GetQuizByID(int quiz_id)
    {
        if (quizDatabase == null) return null;
        return quizDatabase.FirstOrDefault(quiz => quiz.quiz_id == quiz_id);
    }

    public StoryData GetStoryByID(string story_id)
    {
        if (storyDatabase == null) return null;
        return storyDatabase.FirstOrDefault(story => story.story_id == story_id);
    }
    
    // public StageData GetStageByID(int id)
    // {
    //     if (stageDatabase == null) return null;หห
    //     return stageDatabase.FirstOrDefault(stage => stage.stage_id == id);
    // }

    public int GetChapterIdByQuizId(int quiz_id)
    {
        if (chapterEventsDatabase == null || chapterEventsDatabase.Count == 0) return -1;

        foreach (var ev in chapterEventsDatabase)
        {
            if (ev == null) continue;
            // ตรวจเฉพาะ event ที่เป็น Quiz และมีการอ้างอิง quiz
            if (ev.type == ChapterEventsData.EventType.Quiz && ev.quizReference != null)
            {
                if (ev.quizReference.quiz_id == quiz_id)
                {
                    return (ev.chapter_id != null) ? ev.chapter_id.chapter_id : -1;
                }
            }
        }
        return -1;
    }

    public List<StoryData> GetStoryAll()
    {
        if (storyDatabase == null || storyDatabase.Count == 0) return null;
        return storyDatabase;
    }

    // ค้นหา Chapter ทั้งหมดที่อยู่ใน Story ID ที่กำหนด
    public List<ChapterData> GetChaptersByStoryID(string storyId)
    {
        if (chapterDatabase == null) 
            return new List<ChapterData>(); // คืนค่าลิสต์ว่าง (กัน Error)

        // ค้นหา Chapter ทั้งหมดที่ parentStory.story_id ตรงกับ storyId ที่ส่งมา
        return chapterDatabase
            .Where(chap => chap.story != null && chap.story.story_id == storyId)
            .ToList();
    }
}

