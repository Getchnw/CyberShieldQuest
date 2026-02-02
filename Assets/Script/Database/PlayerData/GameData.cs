using System.Collections.Generic;

// ⚠️ สำคัญ: DeckData ต้องอยู่นอกวงเล็บของ GameData ก่อน!
[System.Serializable]
public class DeckData
{
    public int deck_id;
    public string deck_name;
    public List<string> card_ids_in_deck;

    public DeckData(int id, string name)
    {
        deck_id = id;
        deck_name = name;
        card_ids_in_deck = new List<string>();
    }
}

[System.Serializable]
public class GameData
{
    // 1. PlayerProfile
    public PlayerProfile profile;

    // 2. PlayerCardInventory
    public List<PlayerCardInventoryItem> cardInventory;

    // 3. Decks
    public List<DeckData> decks;

    // 4. Progress ต่างๆ
    public List<PlayerStageProgress> stageProgress;
    public List<PlayerChapterProgress> chapterProgress;
    public List<PlayerQuizProgress> quizProgress;
    public List<int> claimedQuizRewardRuleIDs;

    public PlayerTutorialData tutorialData;
    public PlayerSelectedStory selectedStory;
    public PlayerPreTest statusPreTest;
    public PlayerPostTest statusPostTest;
    public List<PlayerPostTestScore> postTestResults;
    public List<PlayerPreTestScore> preTestResults;

    // 🔥 Flag เพื่อกันการเสกการ์ดซ้ำ
    public bool hasInitializedCards = false;

    public bool isNewGameStarted = false;
    // Daily Login
    public PlayerDailyLogin dailyLoginData = new PlayerDailyLogin();
    // Daily Quest
    public PlayerDailyQuestSystem dailyQuestData = new PlayerDailyQuestSystem();
    public List<PlayerAchievementData> achievements = new List<PlayerAchievementData>();
    public string currentTitle = "Novice";

    // Constructor (ตัวสร้างข้อมูลเริ่มต้น)
    public GameData()
    {
        profile = new PlayerProfile();
        tutorialData = new PlayerTutorialData();
        selectedStory = new PlayerSelectedStory();

        cardInventory = new List<PlayerCardInventoryItem>();
        decks = new List<DeckData>();

        statusPostTest = new PlayerPostTest();
        statusPreTest = new PlayerPreTest();
        preTestResults = new List<PlayerPreTestScore>();
        postTestResults = new List<PlayerPostTestScore>();
        stageProgress = new List<PlayerStageProgress>();
        chapterProgress = new List<PlayerChapterProgress>();
        quizProgress = new List<PlayerQuizProgress>();
        claimedQuizRewardRuleIDs = new List<int>();

        hasInitializedCards = false; // 🔥 ยังไม่เสกการ์ด
        isNewGameStarted = false;
        // Daily Login
        dailyLoginData = new PlayerDailyLogin();
        dailyQuestData = new PlayerDailyQuestSystem();
        // Achievement Data
        achievements = new List<PlayerAchievementData>();
        currentTitle = "Novice";
    }
}

// ---------------------------------------------------------
// Class ย่อยต่างๆ
// ---------------------------------------------------------

[System.Serializable]
public class PlayerProfile
{
    public string playerName = "Sentinel";
    public int level = 1;
    public int experience = 0;
    public int gold = 0;

    public int scrap = 0;
}

[System.Serializable]
public class PlayerCardInventoryItem
{
    public string card_id;
    public int quantity;

    public PlayerCardInventoryItem(string id, int qty)
    {
        card_id = id;
        quantity = qty;
    }
}

[System.Serializable]
public class PlayerSelectedStory
{
    public string lastSelectedStoryId = "";
    public int lastSelectedchapterId = 0;
}

[System.Serializable]
public class PlayerTutorialData
{
    public bool hasSeenTutorial_Home = false;
    public bool hasSeenTutorial_Deck = false;
    public bool hasSeenTutorial_Stage = false;
    public bool hasSeenTutorial_Story = false;
    public bool hasSeenTutorial_Shop = false;
}

[System.Serializable]
public class PlayerPreTest
{
    public bool hasSucessPre_A01 = false;
    public bool hasSucessPre_A02 = false;
    public bool hasSucessPre_A03 = false;
}

[System.Serializable]
public class PlayerPostTest
{
    public bool hasSucessPost_A01 = false;
    public bool hasSucessPost_A02 = false;
    public bool hasSucessPost_A03 = false;
}

[System.Serializable
]
public class PlayerStageProgress
{
    public string stageID;        // เปลี่ยนเป็น string เพื่อให้ตรงกับ StageManager (L1_A01, L2_Mix1, ฯลฯ)
    public bool isCompleted;      // ชนะหรือยัง
    public int starsEarned;       // ได้กี่ดาว (0-3)
    public int bestTurns;         // Record เทิร์นที่น้อยที่สุด
    public int highestDamage;     // Record ดาเมจสูงสุด
    public int playCount;         // เล่นกี่รอบแล้ว
    public string lastPlayedDate; // วันที่เล่นล่าสุด
}

[System.Serializable]
public class PlayerChapterProgress
{
    public int chapter_id;
    public bool is_completed;
    public int stars_earned;
    public int high_score;
}

[System.Serializable]
public class PlayerQuizProgress
{
    public int quiz_id;
    public int highest_score;
    public bool is_completed;
    public int stars_earned;
}

[System.Serializable]
public class PlayerPreTestScore
{
    public string story_id;
    public int TotalScore;
    public int MaxScore;
    public List<Qustion_Answer> Qustion_Answers;
}

[System.Serializable]
public class PlayerPostTestScore
{
    public string story_id;
    public int TotalScore;
    public int MaxScore;
    public List<Qustion_Answer> Qustion_Answers;
}

[System.Serializable]
public enum TypeQustion
{
    TrueFalse,
    FillBlank,
    Matching
}
[System.Serializable]
public class Qustion_Answer
{
    public string QustionText;
    public string AnswerText;
    public int score;
    public TypeQustion TypeQustion;
}

[System.Serializable]
public class PlayerDailyLogin
{
    public string lastClaimedDate; // เก็บวันที่รับล่าสุด "yyyy-MM-dd"
    public int currentStreak;      // เก็บจำนวนวันต่อเนื่อง (1-7)

    // Constructor สำหรับค่าเริ่มต้น
    public PlayerDailyLogin()
    {
        lastClaimedDate = "";
        currentStreak = 0;
    }
}

[System.Serializable]
public class PlayerQuestData
{
    public string questID;       // เก็บแค่ ID เพื่อไปดึงข้อมูลจาก ScriptableObject
    public int currentAmount;    // สิ่งที่ต้องจำ (ทำไปเท่าไหร่แล้ว)
    public bool isClaimed;       // สิ่งที่ต้องจำ (รับไปยัง)
}

[System.Serializable]
public class PlayerDailyQuestSystem
{
    public string lastQuestDate; // วันที่ล่าสุดที่รีเซ็ตเควส (เช่น "2023-10-27")
    public List<PlayerQuestData> activeQuests; // รายการเควสของวันนี้

    public PlayerDailyQuestSystem()
    {
        lastQuestDate = "";
        activeQuests = new List<PlayerQuestData>();
    }
}

[System.Serializable]
public class PlayerAchievementData
{
    public string achievementID;
    public bool isUnlocked; // เงื่อนไขครบแล้ว (พร้อมให้รับรางวัล)
    public bool isClaimed;  // รับรางวัลไปแล้ว
}