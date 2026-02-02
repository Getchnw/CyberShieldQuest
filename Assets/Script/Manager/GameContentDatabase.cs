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
    // ข้อมูล Quiz Pretest&PostTest
    [SerializeField] private List<MatchingQuestion> matchingQuestionsDatabase = new List<MatchingQuestion>();
    [SerializeField] private List<FillInBlankQuestion> fillInBlankQuestionsDatabase = new List<FillInBlankQuestion>();
    [SerializeField] private List<TrueFalseQuestion> trueFalseQuestionsDatabase = new List<TrueFalseQuestion>();
    [SerializeField] private List<TrueFalseDescription> trueFalseDescriptionDatabase = new List<TrueFalseDescription>();
    [SerializeField] private List<MatchingDescription> matchingDescriptionDatabase = new List<MatchingDescription>();
    [SerializeField] private List<FillInBlankDescription> fillInBlankDescriptionDatabase = new List<FillInBlankDescription>();
    // ข้อมูล Daily Login & Daily Quest
    [SerializeField] private List<DailyRewardData> dailyRewardDatabase = new List<DailyRewardData>();
    [SerializeField] private List<DailyQuestsData> dailyQuestDatabase = new List<DailyQuestsData>();
    // ข้อมูล Achievement
    [SerializeField] private List<AchievementData> achievementDatabase = new List<AchievementData>();
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
            var loadedEvents = Resources.LoadAll<ChapterEventsData>("GameContent/ChapterEvents");
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
            var loadedQuizzes = Resources.LoadAll<QuizData>("GameContent/Quiz");
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
        // if( stageDatabase == null || stageDatabase.Count == 0)
        // {
        //     var loadedStages = Resources.LoadAll<StageData>("GameContent/Stages");
        //     if (loadedStages != null && loadedStages.Length > 0)
        //     {
        //         stageDatabase = new List<StageData>(loadedStages);
        //     }
        //     #if UNITY_EDITOR
        //     try
        //     {
        //         string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StageData", new[] { "Assets/Script/Database" });
        //         if (guids != null && guids.Length > 0)
        //         {
        //             stageDatabase = new List<StageData>();
        //             foreach (var g in guids)
        //             {
        //                 string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
        //                 var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<StageData>(path);
        //                 if (asset != null) stageDatabase.Add(asset);
        //             }
        //             Debug.Log($"Editor auto-loaded {stageDatabase.Count} StageData from Assets/Script/Database.");
        //         }
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogWarning($"Auto-load StageData failed: {e.Message}");
        //     }
        //     #endif
        // }

        // ดึงMatchingData
        if (matchingQuestionsDatabase == null || matchingQuestionsDatabase.Count == 0)
        {
            var loadMatchingQuestion = Resources.LoadAll<MatchingQuestion>("GameContent/QuizMatching");
            if (loadMatchingQuestion != null && loadMatchingQuestion.Length > 0)
            {
                matchingQuestionsDatabase = new List<MatchingQuestion>(loadMatchingQuestion);
            }

#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MatchingQuestion", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    matchingQuestionsDatabase = new List<MatchingQuestion>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<MatchingQuestion>(path);
                        if (asset != null) matchingQuestionsDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {matchingQuestionsDatabase.Count} MatchingQuestion from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load MatchingQuestion failed: {e.Message}");
            }
#endif
        }

        // ดึงFill Blank data
        if (fillInBlankQuestionsDatabase == null || fillInBlankQuestionsDatabase.Count == 0)
        {
            var loadFillInBlankQuestion = Resources.LoadAll<FillInBlankQuestion>("GameContent/QuizFillInBlank");
            if (loadFillInBlankQuestion != null && loadFillInBlankQuestion.Length > 0)
            {
                fillInBlankQuestionsDatabase = new List<FillInBlankQuestion>(loadFillInBlankQuestion);
            }

#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FillInBlankQuestion", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    fillInBlankQuestionsDatabase = new List<FillInBlankQuestion>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<FillInBlankQuestion>(path);
                        if (asset != null) fillInBlankQuestionsDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {fillInBlankQuestionsDatabase.Count} FillInBlankQuestion from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load FillInBlankQuestion failed: {e.Message}");
            }
#endif
        }

        // ดึงTrue/False Question
        if (trueFalseQuestionsDatabase == null || trueFalseQuestionsDatabase.Count == 0)
        {
            var loadTrueFalseQuestion = Resources.LoadAll<TrueFalseQuestion>("GameContent/QuizTrueFalse");
            if (loadTrueFalseQuestion != null && loadTrueFalseQuestion.Length > 0)
            {
                trueFalseQuestionsDatabase = new List<TrueFalseQuestion>(loadTrueFalseQuestion);
            }

#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TrueFalseQuestion", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    trueFalseQuestionsDatabase = new List<TrueFalseQuestion>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TrueFalseQuestion>(path);
                        if (asset != null) trueFalseQuestionsDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {trueFalseQuestionsDatabase.Count} TrueFalseQuestion from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load TrueFalseQuestion failed: {e.Message}");
            }
#endif
        }

        // ดึง Daily Reward Data
        if (dailyRewardDatabase == null || dailyRewardDatabase.Count == 0)
        {
            var loadDailyRewardData = Resources.LoadAll<DailyRewardData>("GameContent/DailyReward");
            if (loadDailyRewardData != null && loadDailyRewardData.Length > 0)
            {
                dailyRewardDatabase = new List<DailyRewardData>(loadDailyRewardData);
            }
#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DailyRewardData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    dailyRewardDatabase = new List<DailyRewardData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DailyRewardData>(path);
                        if (asset != null) dailyRewardDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {dailyRewardDatabase.Count} DailyRewardData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load DailyRewardData failed: {e.Message}");
            }
#endif
        }

        // ดึง trueFalse Description data
        if (trueFalseDescriptionDatabase == null || trueFalseDescriptionDatabase.Count == 0)
        {
            var loadTrueFalseDescription = Resources.LoadAll<TrueFalseDescription>("GameContent/DescriptionTrueFale");
            if (loadTrueFalseDescription != null && loadTrueFalseDescription.Length > 0)
            {
                trueFalseDescriptionDatabase = new List<TrueFalseDescription>(loadTrueFalseDescription);
            }
#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:TrueFalseDescription", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    trueFalseDescriptionDatabase = new List<TrueFalseDescription>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TrueFalseDescription>(path);
                        if (asset != null) trueFalseDescriptionDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {trueFalseDescriptionDatabase.Count} TrueFalseDescription from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load TrueFalseDescription failed: {e.Message}");
            }
#endif
        }

        // ดึง matching Description data
        if (matchingDescriptionDatabase == null || matchingDescriptionDatabase.Count == 0)
        {
            var loadMatchingDescription = Resources.LoadAll<MatchingDescription>("GameContent/DescriptionMatching");
            if (loadMatchingDescription != null && loadMatchingDescription.Length > 0)
            {
                matchingDescriptionDatabase = new List<MatchingDescription>(loadMatchingDescription);
            }
#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MatchingDescription", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    matchingDescriptionDatabase = new List<MatchingDescription>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<MatchingDescription>(path);
                        if (asset != null) matchingDescriptionDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {matchingDescriptionDatabase.Count} MatchingDescription from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load MatchingDescription failed: {e.Message}");
            }   
#endif
        }

        // ดึง Fill in Blank Description data
        if (fillInBlankDescriptionDatabase == null || fillInBlankDescriptionDatabase.Count == 0)
        {
            var loadFillInBlankDescription = Resources.LoadAll<FillInBlankDescription>("GameContent/DescriptionFillInBlank");
            if (loadFillInBlankDescription != null && loadFillInBlankDescription.Length > 0)
            {
                fillInBlankDescriptionDatabase = new List<FillInBlankDescription>(loadFillInBlankDescription);
            }
#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FillInBlankDescription", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    fillInBlankDescriptionDatabase = new List<FillInBlankDescription>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<FillInBlankDescription>(path);
                        if (asset != null) fillInBlankDescriptionDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {fillInBlankDescriptionDatabase.Count} FillInBlankDescription from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load FillInBlankDescription failed: {e.Message}");
            }
#endif
        }

        // ดึง Daily Quest Data
        if (dailyQuestDatabase == null || dailyQuestDatabase.Count == 0)
        {
            var loadDailyQuestData = Resources.LoadAll<DailyQuestsData>("GameContent/DailyQuest");
            if (loadDailyQuestData != null && loadDailyQuestData.Length > 0)
            {
                dailyQuestDatabase = new List<DailyQuestsData>(loadDailyQuestData);
            }
#if UNITY_EDITOR
            try
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DailyQuestsData", new[] { "Assets/Script/Database" });
                if (guids != null && guids.Length > 0)
                {
                    dailyQuestDatabase = new List<DailyQuestsData>();
                    foreach (var g in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<DailyQuestsData>(path);
                        if (asset != null) dailyQuestDatabase.Add(asset);
                    }
                    Debug.Log($"Editor auto-loaded {dailyQuestDatabase.Count} DailyQuestData from Assets/Script/Database.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-load DailyQuestData failed: {e.Message}");
            }
#endif
        }

        // ดึง Acheivement Data
        if (achievementDatabase == null || achievementDatabase.Count == 0)
        {
            var loadAchievementData = Resources.LoadAll<AchievementData>("GameContent/Achievement");
            if (loadAchievementData != null && loadAchievementData.Length > 0)
            {
                achievementDatabase = new List<AchievementData>(loadAchievementData);
            }
            // #if UNITY_EDITOR
            //             try
            //             {
            //                 string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AchievementData", new[] { "Assets/Script/Database" });
            //                 if (guids != null && guids.Length > 0)
            //                 {
            //                     achievementDatabase = new List<AchievementData>();
            //                     foreach (var g in guids)
            //                     {
            //                         string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
            //                         var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AchievementData>(path);
            //                         if (asset != null) achievementDatabase.Add(asset);
            //                     }
            //                     Debug.Log($"Editor auto-loaded {achievementDatabase.Count} AchievementData from Assets/Script/Database.");
            //                 }
            //             }
            //             catch (System.Exception e)
            //             {
            //                 Debug.LogWarning($"Auto-load AchievementData failed: {e.Message}");
            //             }
            // #endif
        }
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
                    return (ev.chapter != null) ? ev.chapter.chapter_id : -1;
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

    // ค้นหา Chapter ทั้งหมดที่อยู่ใน Story ID
    public List<ChapterData> GetChaptersByStoryID(string storyId)
    {
        if (chapterDatabase == null)
            return new List<ChapterData>(); // คืนค่าลิสต์ว่าง (กัน Error)

        // ค้นหา Chapter ทั้งหมดที่ parentStory.story_id ตรงกับ storyId ที่ส่งมา
        return chapterDatabase
            .Where(chap => chap.story != null && chap.story.story_id == storyId)
            .ToList();
    }

    public List<ChapterData> GetAllStoryChapters()
    {
        if (chapterDatabase == null)
            return new List<ChapterData>(); // คืนค่าลิสต์ว่าง (กัน Error)

        return chapterDatabase;
    }

    // ค้นหา ChapterEvents ทั้งหมดที่อยู่ใน Chapter ID
    public List<ChapterEventsData> GetChapterEventsByChapterID(int chapterId)
    {
        if (chapterEventsDatabase == null)
            return new List<ChapterEventsData>(); // คืนค่าลิสต์ว่าง ถ้าเป็น null

        // ค้นหา ChapterEvents ทั้งหมดที่ chapter.chapter_id ตรงกับ chapterId ที่ส่งมา
        return chapterEventsDatabase
            .Where(ev => ev.chapter != null && ev.chapter.chapter_id == chapterId)
            .OrderBy(ev => ev.eventOrder) // เรียงลำดับตาม eventOrder
            .ToList();
    }

    // ดึง "บทพูด" ทั้งหมดของ Scene (เรียงลำดับแล้ว) เอาidมาจากchapter events
    public List<DialogueLinesData> GetDialogueLinesByScene(int sceneId)
    {
        if (dialogueLinesDatabase == null) return new List<DialogueLinesData>();

        return dialogueLinesDatabase
            .Where(line => line.scene != null && line.scene.scene_id == sceneId) //
            .OrderBy(line => line.sequence_order) // เรียง 1, 2, 3...
            .ToList();
    }

    //ดึงQuestion ทั้งหมดของ Quiz
    public List<QuestionData> GetQuestionsByQuizID(int quizId)
    {
        if (questionDatabase == null) return new List<QuestionData>();

        return questionDatabase
            .Where(q => q.quiz != null && q.quiz.quiz_id == quizId)
            .OrderBy(q => q.questionOrder)
            .ToList();
    }

    //ดึง Reward ของ Quiz
    public List<RewardData> GetRewardByQuizID(int quizId)
    {
        if (rewardDatabase == null) return new List<RewardData>();

        return rewardDatabase
            .Where(r => r.quiz != null && r.quiz.quiz_id == quizId) //
            .OrderBy(r => r.starRequired) // เรียงรางวัล 1 ดาว, 2 ดาว, 3 ดาว
            .ToList();
    }

    public List<MatchingQuestion> GetMatchingQuestionsByStoryId(string storyId)
    {
        if (matchingQuestionsDatabase == null) return new List<MatchingQuestion>();

        return matchingQuestionsDatabase
            .Where(m => m.story != null && m.story.story_id == storyId)
            .ToList();
    }

    public List<FillInBlankQuestion> GetFillInBlankQuestionsByStoryId(string storyId)
    {
        if (fillInBlankQuestionsDatabase == null) return new List<FillInBlankQuestion>();

        return fillInBlankQuestionsDatabase
            .Where(f => f.story != null && f.story.story_id == storyId)
            .ToList();
    }

    public List<TrueFalseQuestion> GetTrueFalseQuestionsByStoryId(string storyId)
    {
        if (trueFalseQuestionsDatabase == null) return new List<TrueFalseQuestion>();

        return trueFalseQuestionsDatabase
            .Where(tf => tf.story != null && tf.story.story_id == storyId)
            .ToList();
    }

    // ดึง Daily Reward ทั้งหมด
    public List<DailyRewardData> GetAllDailyRewardData()
    {
        if (dailyRewardDatabase == null) return new List<DailyRewardData>();
        return dailyRewardDatabase;
    }

    // ดึง Daily Reward ตามลำดับ (ตัวแรก)
    public DailyRewardData GetDailyRewardData()
    {
        if (dailyRewardDatabase == null || dailyRewardDatabase.Count == 0) return null;
        return dailyRewardDatabase[0];
    }

    // ดึง Daily Quest ทั้งหมด
    public DailyQuestsData GetQuestByID(string id)
    {
        return dailyQuestDatabase.FirstOrDefault(q => q.questID == id);
    }

    // 2. สุ่มเควส (ใช้ตอนขึ้นวันใหม่)
    public List<DailyQuestsData> GetRandomQuests(int count)
    {
        // สับเปลี่ยนลำดับแล้วหยิบมาตามจำนวนที่ต้องการ (เช่น 3 อัน)
        var list = new List<DailyQuestsData>();
        int Num_random_Quest_Stage = 0;
        int Num_random_Quest_Story = 0;
        int Num_random_Quest_Gacha = 0;
        int Num_random_Quest_Card = 0;

        // แบ่งจำนวนสุ่มเควสให้เท่าๆกันในแต่ละประเภท
        int count_per_type = count / 4;
        while (list.Count < count)
        {
            // ข้อมูลเควสทั้งหมดในฐานข้อมูล
            var shuffledQuests = dailyQuestDatabase.OrderBy(x => Random.value).ToList();
            foreach (var quest in shuffledQuests)
            {
                if (list.Count >= count) break;

                if (quest.type == QuestType.Stage && Num_random_Quest_Stage < count_per_type)
                {
                    list.Add(quest);
                    Num_random_Quest_Stage++;
                }
                else if (quest.type == QuestType.Story && Num_random_Quest_Story < count_per_type)
                {
                    list.Add(quest);
                    Num_random_Quest_Story++;
                }
                else if (quest.type == QuestType.Gacha && Num_random_Quest_Gacha < count_per_type)
                {
                    list.Add(quest);
                    Num_random_Quest_Gacha++;
                }
                else if (quest.type == QuestType.Card && Num_random_Quest_Card < count_per_type)
                {
                    list.Add(quest);
                    Num_random_Quest_Card++;
                }
            }
        }

        return list;
    }

    // ดึงคำอธิบายของ True/False ตาม ID story
    public List<TrueFalseDescription> GetTrueFalseDescriptionByID(string story_id)
    {
        if (trueFalseDescriptionDatabase == null) return null;
        return trueFalseDescriptionDatabase.Where(TF => TF.TrueFalse.story.story_id == story_id).ToList();
    }

    // ดึงคำอธิบายของ Matching ตาม ID story
    public List<MatchingDescription> GetMatchingDescriptionByID(string story_id)
    {
        if (matchingDescriptionDatabase == null) return null;
        return matchingDescriptionDatabase.Where(M => M.Matching.story.story_id == story_id).ToList();
    }
    // ดึงคำอธิบายของ Fill in Blank ตาม ID story
    public List<FillInBlankDescription> GetFillInBlankDescriptionByID(string story_id)
    {
        if (fillInBlankDescriptionDatabase == null) return null;
        return fillInBlankDescriptionDatabase.Where(FB => FB.FillInBlank.story.story_id == story_id).ToList();
    }

    // ดึงข้อมูล Achievement ทั้งหมด
    public List<AchievementData> GetAllAchievementData()
    {
        if (achievementDatabase == null) return new List<AchievementData>();
        return achievementDatabase;
    }
}

