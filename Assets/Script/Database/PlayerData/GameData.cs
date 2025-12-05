using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // 1. PlayerProfile
    public PlayerProfile profile;

    // 2. PlayerCardInventory
    // เราไม่เก็บ string[] cards แล้ว แต่จะเก็บ List ที่ซับซ้อนกว่า
    public List<PlayerCardInventoryItem> cardInventory;

    // 3. Decks & DeckCards
    public List<DeckData> decks; // เก็บข้อมูล Deck และการ์ดข้างใน

    // 4. PlayerStageProgress
    public List<PlayerStageProgress> stageProgress;

    // 5. PlayerChapterProgress
    public List<PlayerChapterProgress> chapterProgress;

    // 6. PlayerQuizProgress
    public List<PlayerQuizProgress> quizProgress;

    // 7. PlayerClaimedQuizRewards
    public List<int> claimedQuizRewardRuleIDs; // เก็บแค่ ID ของรางวัลที่รับไปแล้ว

    public PlayerTutorialData tutorialData; // ข้อมูลการดู Tutorial

    public PlayerSelectedStory selectedStory; // ข้อมูลการเลือก Story ล่าสุด

    public PlayerPreTest statusPreTest;
    public PlayerPostTest statusPostTest;

    public List<PlayerPostTestScore> postTestResults;
    public List<PlayerPreTestScore> preTestResults;
    // เก็บข้อมูลผู้เล่น
    [System.Serializable]
    public class DeckData 
    {
        public int deck_id; 
        public string deck_name;
        
        // ⚠️ แก้จาก List<int> เป็น List<string>
        public List<string> card_ids_in_deck; 

        public DeckData(int id, string name)
        {
            deck_id = id;
            deck_name = name;
            card_ids_in_deck = new List<string>();
        }
    }
    public GameData()
    {
        profile = new PlayerProfile(); // สร้างโปรไฟล์เริ่มต้น
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
    }
}

[System.Serializable]
public class PlayerProfile
{
    public string playerName = "Sentinel";
    public int level = 1;
    public int experience = 0;
    public int gold = 0;
    //public bool hasSeenTutorial = false;
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

[System.Serializable]
public class PlayerCardInventoryItem
{
    public string card_id; // "Foreign Key" ไปหา ScriptableObject
    public int quantity;

    public PlayerCardInventoryItem(string id, int qty)
    {
        card_id = id;
        quantity = qty;
    }
}

[System.Serializable]

public class DeckData 
{
    public int deck_id; 
    public string deck_name;
    
    // ⚠️ แก้จาก List<int> เป็น List<string>
    public List<string> card_ids_in_deck; 

    public DeckData(int id, string name)
    {
        deck_id = id;
        deck_name = name;
        card_ids_in_deck = new List<string>();
    }
}

[System.Serializable]
public class PlayerStageProgress
{
    public int stage_id; // ID ของด่านที่ผ่าน
    public bool is_completed;
    public int stars_earned;
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
    public int score;
    public int Maxscore;
}

[System.Serializable]
public class PlayerPostTestScore
{
    public string story_id;
    public int score;
    public int Maxscore;
}