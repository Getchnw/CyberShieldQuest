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

    // เก็บข้อมูลผู้เล่น
    public GameData()
    {
        profile = new PlayerProfile(); // สร้างโปรไฟล์เริ่มต้น
        tutorialData = new PlayerTutorialData();
        selectedStory = new PlayerSelectedStory();
        cardInventory = new List<PlayerCardInventoryItem>();
        decks = new List<DeckData>();
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
public class PlayerCardInventoryItem
{
    public int card_id; // "Foreign Key" ไปหา ScriptableObject
    public int quantity;

    public PlayerCardInventoryItem(int id, int qty)
    {
        card_id = id;
        quantity = qty;
    }
}

[System.Serializable]
public class DeckData // รวมตาราง Decks และ DeckCards ไว้ด้วยกัน
{
    public int deck_id; // เช่น 1, 2, 3
    public string deck_name;
    public List<int> card_ids_in_deck; // เก็บ ID ของการ์ดที่อยู่ในเด็คนี้

    public DeckData(int id, string name)
    {
        deck_id = id;
        deck_name = name;
        card_ids_in_deck = new List<int>();
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