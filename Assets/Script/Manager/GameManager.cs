using UnityEngine;
using System.Linq; //ใช้ .FirstOrDefault()
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Starter Cards")]
    [SerializeField] private int starterCardQuantity = 3;
    [SerializeField] private List<string> starterGeneralCardPrefixes = new List<string> { "M_Gen", "S_Gen", "E_Gen" };
    private const int StarterGrantDataVersion = 3;

    // สร้าง "ตัวแปรกลาง" ให้ทุกคนในเกมเรียกใช้ได้ง่ายๆ
    public static GameManager Instance { get; private set; }

    // ตัวแปรสำหรับเก็บข้อมูลผู้เล่นคนปัจจุบัน
    public GameData CurrentGameData { get; private set; }
    public event System.Action OnDataLoaded;
    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnExperienceChanged;
    public event System.Action<int> OnLevelChanged;
    public event System.Action OnInventoryChanged; // 🔥 เตือนเมื่อ inventory เปลี่ยน
    public event System.Action OnPostTestCompleted;
    [Header("Leveling System")]
    [Tooltip("กราฟกำหนดค่า EXP ที่ต้องใช้ในแต่ละเลเวล (แกน X=Level, แกน Y=Exp Required)")]
    public AnimationCurve experienceCurve;
    public int maxLevel = 99;

    private void Awake()
    {
        // --- โค้ดส่วน Singleton ---
        // เช็คว่าในเกมมี GameManager อยู่แล้วหรือยัง?
        if (Instance == null)
        {
            // ถ้ายังไม่มี... ให้ตัวเรานี่แหละเป็น GameManager หลัก!
            Instance = this;
            // สั่งให้ Unity "อย่าทำลาย" GameObject นี้ทิ้งเมื่อเปลี่ยนฉาก
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // แต่ถ้าในเกมมี GameManager ตัวอื่นอยู่แล้ว...
            // ให้ทำลายตัวเองทิ้งไปเลย เพื่อป้องกันไม่ให้มี GameManager ซ้ำซ้อน
            Destroy(gameObject);
        }
    }

    // Ensure we load existing save or create a new one when the GameManager starts
    private void Start()
    {
        // พยายามโหลดไฟล์
        bool loaded = LoadGame();
        if (!loaded)
        {
            // ถ้าไม่มีไฟล์ ให้สร้างข้อมูลเปล่าๆ ไว้ใน RAM "แต่ยังไม่ต้อง Save ลงเครื่อง"
            // เพื่อให้ฉาก Home ไม่พัง (NullReference) แต่ปุ่ม Continuous ยังล็อกอยู่
            InitializeNewGameData();
            Debug.Log("No save found. Initialized temporary data in RAM.");
        }
    }

    private void InitializeNewGameData()
    {
        CurrentGameData = new GameData();
        CurrentGameData.profile.level = 1;
        CurrentGameData.profile.experience = 0;
        EnsureStarterCardsForAllPlayers();
        // ไม่ต้องเรียก SaveSystem.SaveGameData(CurrentGameData); ที่นี่!
        Debug.Log("Initialized empty data in RAM (No file created yet).");
    }

    // เมธอดสำหรับ "เริ่มเกมใหม่"
    public void CreateNewGame()
    {
        // สร้างข้อมูลผู้เล่นชุดใหม่ขึ้นมา (จาก Constructor ใน GameData.cs)
        InitializeNewGameData();
        Debug.Log("After InitializeNewGameData");
        // 🔥 เสกการ์ดให้ผู้เล่นตอนเริ่มเกมใหม่
        // Dev_AddAllCards();

        // สั่งให้ SaveSystem บันทึกข้อมูลใหม่นี้ลงไฟล์ทันที
        SaveSystem.SaveGameData(CurrentGameData);
        Debug.Log("New game data created and saved.");
        OnDataLoaded?.Invoke();
    }

    // เมธอดสำหรับ "โหลดเกม"
    public bool LoadGame()
    {
        // สั่งให้ SaveSystem ไปอ่านไฟล์เซฟแล้วส่งข้อมูลกลับมา
        CurrentGameData = SaveSystem.LoadGameData();

        // ถ้าโหลดสำเร็จ (ข้อมูลที่ได้มาไม่เป็น null) ให้ส่งค่า true กลับไป
        if (CurrentGameData != null)
        {
            bool hasGrantedNewStarterCards = EnsureStarterCardsForAllPlayers();

            if (CurrentGameData.profile.level <= 0)
            {
                CurrentGameData.profile.level = 1;
            }

            if (hasGrantedNewStarterCards)
            {
                SaveSystem.SaveGameData(CurrentGameData);
            }

            Debug.Log("Game data loaded successfully.");
            OnDataLoaded?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("Failed to load game data from file.");
            return false;
        }
    }

    private bool EnsureStarterCardsForAllPlayers()
    {
        if (CurrentGameData == null) return false;

        if (CurrentGameData.cardInventory == null)
        {
            CurrentGameData.cardInventory = new List<PlayerCardInventoryItem>();
        }

        if (CurrentGameData.starterCardGrantedIds == null)
        {
            CurrentGameData.starterCardGrantedIds = new List<string>();
        }

        // กันเคสค่าใน Inspector ยังเป็น 1 จากค่าเก่าที่ถูก serialize ไว้
        if (starterCardQuantity < 3) starterCardQuantity = 3;

        CardData[] allCards = Resources.LoadAll<CardData>("GameContent/Cards");
        if (allCards == null || allCards.Length == 0)
        {
            Debug.LogWarning("Starter card sync skipped: no cards found at Resources/GameContent/Cards.");
            return false;
        }

        int grantedCount = 0;
        int topUpCount = 0;
        foreach (CardData card in allCards)
        {
            if (card == null || string.IsNullOrEmpty(card.card_id)) continue;
            if (!IsGeneralStarterCardId(card.card_id)) continue;

            PlayerCardInventoryItem existing = CurrentGameData.cardInventory.FirstOrDefault(x => x.card_id == card.card_id);
            bool alreadyGranted = CurrentGameData.starterCardGrantedIds.Contains(card.card_id);

            if (!alreadyGranted)
            {
                if (existing != null)
                {
                    existing.quantity += starterCardQuantity;
                }
                else
                {
                    CurrentGameData.cardInventory.Add(new PlayerCardInventoryItem(card.card_id, starterCardQuantity));
                }

                CurrentGameData.starterCardGrantedIds.Add(card.card_id);
                grantedCount++;
            }
            else
            {
                // รองรับเซฟเก่าที่เคยได้ไม่ครบ 3 ใบ/การ migrate กติกาเดิม
                if (CurrentGameData.starterGrantDataVersion < StarterGrantDataVersion)
                {
                    int currentQty = existing != null ? existing.quantity : 0;
                    int missingQty = starterCardQuantity - currentQty;
                    if (missingQty > 0)
                    {
                        if (existing != null)
                        {
                            existing.quantity += missingQty;
                        }
                        else
                        {
                            CurrentGameData.cardInventory.Add(new PlayerCardInventoryItem(card.card_id, missingQty));
                        }
                        topUpCount++;
                    }
                }
            }
        }

        if (CurrentGameData.starterGrantDataVersion < StarterGrantDataVersion)
        {
            CurrentGameData.starterGrantDataVersion = StarterGrantDataVersion;
        }

        if (grantedCount > 0 || topUpCount > 0)
        {
            Debug.Log($"Starter card sync: granted {grantedCount} general cards and topped up {topUpCount} cards to qty {starterCardQuantity}.");
            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    private bool IsGeneralStarterCardId(string cardId)
    {
        if (string.IsNullOrEmpty(cardId) || starterGeneralCardPrefixes == null || starterGeneralCardPrefixes.Count == 0)
        {
            return false;
        }

        foreach (string prefix in starterGeneralCardPrefixes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;
            if (cardId.StartsWith(prefix)) return true;
        }

        return false;
    }

    // เราสามารถเพิ่มเมธอดสำหรับเซฟเกมระหว่างเล่นได้ด้วย
    public void SaveCurrentGame()
    {
        if (CurrentGameData != null)
        {
            SaveSystem.SaveGameData(CurrentGameData);
            Debug.Log("Game progress saved!");
            // Notify UI listeners that data has been saved/updated
            OnDataLoaded?.Invoke();
        }
    }

    /// ฟังก์ชันคำนวณหา Max Exp ของเลเวลนั้นๆ จาก AnimationCurve
    public int GetMaxExpForLevel(int level)
    {
        if (experienceCurve == null || experienceCurve.length == 0) return 100; // กัน Error
        return Mathf.RoundToInt(experienceCurve.Evaluate(level));
    }

    // Add experience to player
    public void AddExperience(int amount)
    {
        if (CurrentGameData == null) return;

        // 1. เพิ่ม EXP
        CurrentGameData.profile.experience += amount;

        // 2. ดึงข้อมูลเลเวลปัจจุบัน
        int currentLevel = CurrentGameData.profile.level;
        int maxExp = GetMaxExpForLevel(currentLevel);

        // 3. วนลูปเช็คการอัปเลเวล (เผื่อได้ Exp เยอะจนอัปหลายเวลรวดเดียว)
        bool hasLeveledUp = false;
        while (CurrentGameData.profile.experience >= maxExp && currentLevel < maxLevel)
        {
            CurrentGameData.profile.experience -= maxExp; // หัก Exp ออก
            currentLevel++;                               // เพิ่มเลเวล
            CurrentGameData.profile.level = currentLevel; // บันทึกลง Data

            // คำนวณ Max Exp ของเลเวลใหม่ เพื่อใช้เช็คในรอบถัดไป (ถ้า exp ยังเหลือ)
            maxExp = GetMaxExpForLevel(currentLevel);

            hasLeveledUp = true;
            Debug.Log($"<color=green>Level Up! Now Level {currentLevel}</color>");
        }

        // 4. บันทึกและส่ง Event
        SaveCurrentGame();

        // ส่ง Event บอก UI ให้อัปเดตหลอด Exp
        OnExperienceChanged?.Invoke(CurrentGameData.profile.experience);

        // ถ้ามีการอัปเลเวล ให้ส่ง Event บอก UI ให้เปลี่ยนตัวเลขเลเวล
        if (hasLeveledUp)
        {
            OnLevelChanged?.Invoke(currentLevel);
        }
    }

    /// เพิ่ม/ลด ทอง (ใช้ค่าติดลบเพื่อลด)
    public void AddGold(int amount)
    {
        if (CurrentGameData == null) return;

        CurrentGameData.profile.gold += amount;

        // (Optional) สั่งเซฟอัตโนมัติ
        SaveCurrentGame();

        // (Optional) ส่ง Event บอก UI ให้อัปเดต
        OnGoldChanged?.Invoke(CurrentGameData.profile.gold);
    }
    /// ลด ทอง
    public void DecreaseGold(int amount)
    {
        if (CurrentGameData == null) return;

        CurrentGameData.profile.gold -= amount;

        // (Optional) สั่งเซฟอัตโนมัติ
        SaveCurrentGame();

        // (Optional) ส่ง Event บอก UI ให้อัปเดต
        OnGoldChanged?.Invoke(CurrentGameData.profile.gold);
    }
    /// เพิ่มการ์ดเข้าคลัง
    public void AddCardToInventory(string cardID, int quantity = 1)
    {
        if (CurrentGameData == null) return;

        // 1. ค้นหาว่าเคยมีการ์ด ID นี้ในคลังหรือยัง
        // (เราต้อง .ToList() ก่อนเพื่อความปลอดภัยในการแก้ไข List)
        var inventory = CurrentGameData.cardInventory;
        // คือ Findfirstใน.js
        PlayerCardInventoryItem existingCard = inventory.FirstOrDefault(card => card.card_id == cardID);

        if (existingCard != null)
        {
            // 2. ถ้ามีแล้ว: เพิ่มจำนวน
            existingCard.quantity += quantity;
        }
        else
        {
            // 3. ถ้ายังไม่มี: เพิ่มการ์ดใหม่เข้าไป
            inventory.Add(new PlayerCardInventoryItem(cardID, quantity));
        }

        Debug.Log($"Added card {cardID} (Qty: {quantity}) to inventory.");

        // 🔥 แจ้งเตือน UI ที่ฟังว่า inventory เปลี่ยน
        OnInventoryChanged?.Invoke();
    }

    //Save storyId
    public void SaveSelectedStory(string storyId)
    {
        if (CurrentGameData == null) return;

        CurrentGameData.selectedStory.lastSelectedStoryId = storyId;
        Debug.Log($"Selected Story ID {storyId} saved to player profile.");
        SaveCurrentGame();
        Debug.Log($"Selected Story ID {storyId} saved to player profile.");
    }

    public void SaveSelectedChapter(int chapterId)
    {
        if (CurrentGameData == null) return;

        CurrentGameData.selectedStory.lastSelectedchapterId = chapterId;
        SaveCurrentGame();
        Debug.Log($"Selected Chapter ID {chapterId} saved to player profile.");
    }

    /// (ใหม่) เช็คว่าเคยรับรางวัลนี้หรือยัง
    public bool HasClaimedReward(int rewardId)
    {
        if (CurrentGameData == null) return false;
        return CurrentGameData.claimedQuizRewardRuleIDs.Contains(rewardId); //เช็คใน GameData ที่ claimedQuizRewardRuleIDs ว่ามีรางวัลนี้หรือยัง
    }

    /// (ใหม่) บันทึกว่ารับรางวัลนี้ไปแล้ว (กันรับซ้ำ)
    public void ClaimReward(int rewardId)
    {
        if (CurrentGameData == null) return;
        if (!CurrentGameData.claimedQuizRewardRuleIDs.Contains(rewardId))
        {
            CurrentGameData.claimedQuizRewardRuleIDs.Add(rewardId);
            SaveCurrentGame();
        }
    }

    //บันทึกความคืบหน้าของQuiz
    public void UpdateQuizProgress(int quizID, int highestScore, bool isCompleted)
    {
        if (CurrentGameData == null) return;

        var progressList = CurrentGameData.quizProgress;
        PlayerQuizProgress quizProgress = progressList.FirstOrDefault(q => q.quiz_id == quizID);
        var stars_earned = highestScore == 5 ? 3 : highestScore == 4 ? 2 : highestScore == 3 ? 1 : 0;

        if (quizProgress != null)
        {

            // 1. ถ้าเคยเล่นแล้ว: อัปเดตคะแนนสูงสุดและสถานะการผ่าน
            if (highestScore > quizProgress.highest_score)
            {
                quizProgress.highest_score = highestScore;
            }

            quizProgress.is_completed = isCompleted || quizProgress.is_completed;
            quizProgress.stars_earned = stars_earned > quizProgress.stars_earned ? stars_earned : quizProgress.stars_earned;
        }
        else
        {
            // 2. ถ้าเล่นครั้งแรก: สร้างข้อมูลใหม่
            progressList.Add(new PlayerQuizProgress
            {
                quiz_id = quizID,
                highest_score = highestScore,
                is_completed = isCompleted,
                stars_earned = stars_earned
            });
        }

        int chapterId = GameContentDatabase.Instance.GetChapterIdByQuizId(quizID);
        if (chapterId >= 0)
        {
            // ส่งค่าคะแนนและดาวของ *รอบนี้* ไปเช็คกับ Chapter
            AdvanceChapterProgress(chapterId, stars_earned, highestScore, isCompleted);
        }

        SaveCurrentGame();
    }

    // ลิสต์รายชื่อการ์ดที่จะเสก (ใส่ ID การ์ดที่นี่)
    [ContextMenu("DEV: Add All Cards")] // <- นี่คือคำสั่งพิเศษ
    public void Dev_AddAllCards()
    {
        if (CurrentGameData == null) CurrentGameData = new GameData();

        // 🔥 กันการเสกการ์ดซ้ำ
        if (CurrentGameData.hasInitializedCards)
        {
            Debug.Log("⚠️ Cards already initialized! Skipping...");
            return;
        }

        // โหลดการ์ดทั้งหมดจาก Resources แล้วเสกให้ผู้เล่น
        CardData[] allCards = Resources.LoadAll<CardData>("GameContent/Cards");

        Debug.Log($"🔥 Loaded {allCards.Length} cards from resources");

        // เสกการ์ดทั้งหมด อย่างละ 10 ใบ
        foreach (CardData card in allCards)
        {
            AddCardToInventory(card.card_id, 3);
            Debug.Log($"✅ Added card: {card.card_id} ({card.cardName})");
        }

        // 🔥 ตั้ง flag เพื่อกันการเสกซ้ำ
        CurrentGameData.hasInitializedCards = true;

        SaveCurrentGame();
        Debug.Log($"✨ เสกการ์ด {allCards.Length} ใบเรียบร้อย! (Cheat Mode Activated)");
    }

    // 🔥 DEV: ลบ Save File
    [ContextMenu("DEV: Clear Save")]
    public void Dev_ClearSave()
    {
        SaveSystem.DeleteSaveData();
        Debug.Log("✨ Save file deleted! Restart the game to create a new one.");
    }
    /// บันทึกความคืบหน้าของ Story Chapter
    public void AdvanceChapterProgress(int chapterID, int stars_earned, int score, bool is_completed)
    {
        if (CurrentGameData == null) return;

        var chapter = CurrentGameData.chapterProgress;
        PlayerChapterProgress chapterProgress = chapter.FirstOrDefault(c => c.chapter_id == chapterID);

        // 1. ถ้าเคยเล่นแล้ว: อัปเดตความคืบหน้า
        if (chapterProgress != null)
        {
            // chapterProgress.is_completed = true;
            // อัปเดต Event ล่าสุดที่ผ่าน
            if (stars_earned > chapterProgress.stars_earned)
            {
                chapterProgress.stars_earned = stars_earned;
            }
            if (score > chapterProgress.high_score)
            {
                chapterProgress.high_score = score;
            }
            // ถ้าผ่านด่านนี้แล้ว
            chapterProgress.is_completed = is_completed || chapterProgress.is_completed;

        }
        else
        {
            // เริ่ม Chapter นี้เป็นครั้งแรก
            CurrentGameData.chapterProgress.Add(new PlayerChapterProgress
            {
                chapter_id = chapterID,
                is_completed = is_completed,
                stars_earned = stars_earned,
                high_score = score
            });
        }
        SaveCurrentGame();
    }

    public void SaveStatusPreTest_PostTest(bool isPreOrPost, string story_id)
    {
        if (CurrentGameData == null) return;
        var statusPreTest = CurrentGameData.statusPreTest;
        var statusPostTest = CurrentGameData.statusPostTest;
        if (isPreOrPost)
        {
            switch (story_id)
            {
                case "A01":
                    {
                        statusPreTest.hasSucessPre_A01 = true;
                        break;
                    }
                case "A02":
                    {
                        statusPreTest.hasSucessPre_A02 = true;
                        break;
                    }
                case "A03":
                    {
                        statusPreTest.hasSucessPre_A03 = true;
                        break;
                    }
                default:
                    {
                        Debug.Log("Not found in All case ");
                        break;
                    }
            }
        }
        // ของPostTest
        else
        {
            switch (story_id)
            {
                case "A01":
                    {
                        statusPostTest.hasSucessPost_A01 = true;
                        break;
                    }
                case "A02":
                    {
                        statusPostTest.hasSucessPost_A02 = true;
                        break;
                    }
                case "A03":
                    {
                        statusPostTest.hasSucessPost_A03 = true;
                        break;
                    }
                default:
                    {
                        Debug.Log("Not found in All case ");
                        break;
                    }
            }
        }
        SaveCurrentGame();
    }


    public void SavePreTest_PostTest(
        bool isPreOrPost, string story_id
        , int score, int Maxscore
        , List<Qustion_Answer> answersList
    )
    {
        if (CurrentGameData == null) return;
        {
            //คะแนนของPretest
            if (isPreOrPost)
            {
                CurrentGameData.preTestResults.Add(new PlayerPreTestScore
                {
                    story_id = story_id,
                    TotalScore = score,
                    MaxScore = Maxscore,
                    Qustion_Answers = new List<Qustion_Answer>(answersList)
                });
                SaveStatusPreTest_PostTest(isPreOrPost, story_id);
            }
            // ของPostTest
            else
            {
                CurrentGameData.postTestResults.Add(new PlayerPostTestScore
                {
                    story_id = story_id,
                    TotalScore = score,
                    MaxScore = Maxscore,
                    Qustion_Answers = new List<Qustion_Answer>(answersList)
                });
                SaveStatusPreTest_PostTest(isPreOrPost, story_id);
                OnPostTestCompleted?.Invoke();
            }
            SaveCurrentGame();
        }
    }
    /// <summary>
    /// บันทึกว่าผ่านด่าน Stage แล้ว และเก็บสถิติ
    /// </summary>
    public void CompleteStage(string stageID, int starsEarned, BattleStatistics stats = null, List<bool> missionResults = null)
    {
        if (CurrentGameData == null) return;

        var progressList = CurrentGameData.stageProgress;
        PlayerStageProgress stageProgress = progressList.FirstOrDefault(s => s.stageID == stageID);

        int CalculateStarCountFromMissions(List<bool> completedMissions, int fallbackStars)
        {
            if (completedMissions == null || completedMissions.Count == 0)
                return Mathf.Clamp(fallbackStars, 0, 3);

            int trueCount = completedMissions.Count(done => done);
            return Mathf.Clamp(trueCount, 0, 3);
        }

        List<bool> MergeMissionResults(List<bool> existingMissions, List<bool> newMissions)
        {
            if ((existingMissions == null || existingMissions.Count == 0)
                && (newMissions == null || newMissions.Count == 0))
            {
                return new List<bool>();
            }

            if (existingMissions == null || existingMissions.Count == 0)
            {
                return new List<bool>(newMissions);
            }

            if (newMissions == null || newMissions.Count == 0)
            {
                return new List<bool>(existingMissions);
            }

            int mergedCount = Mathf.Max(existingMissions.Count, newMissions.Count);
            List<bool> merged = new List<bool>(mergedCount);

            for (int i = 0; i < mergedCount; i++)
            {
                bool oldDone = i < existingMissions.Count && existingMissions[i];
                bool newDone = i < newMissions.Count && newMissions[i];
                merged.Add(oldDone || newDone);
            }

            return merged;
        }

        if (stageProgress != null)
        {
            // 1. ถ้าเคยเล่นแล้ว: อัปเดตดาว (ถ้าทำได้ดีขึ้น)
            stageProgress.isCompleted = true;
            stageProgress.playCount++;
            stageProgress.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // เก็บผล mission แบบสะสม: ถ้าเคยผ่าน mission นั้นแล้ว จะคง true ตลอด
            stageProgress.completedStarMissions = MergeMissionResults(stageProgress.completedStarMissions, missionResults);
            int fallbackStars = Mathf.Max(stageProgress.starsEarned, starsEarned);
            stageProgress.starsEarned = CalculateStarCountFromMissions(stageProgress.completedStarMissions, fallbackStars);

            // อัปเดต Records
            if (stats != null)
            {
                if (stageProgress.bestTurns == 0 || stats.turnsPlayed < stageProgress.bestTurns)
                    stageProgress.bestTurns = stats.turnsPlayed;

                if (stats.totalDamageDealt > stageProgress.highestDamage)
                    stageProgress.highestDamage = stats.totalDamageDealt;
            }
        }
        else
        {
            // 2. ถ้าเล่นครั้งแรก: สร้างข้อมูลใหม่
            progressList.Add(new PlayerStageProgress
            {
                stageID = stageID,
                isCompleted = true,
                starsEarned = 0,
                completedStarMissions = missionResults != null ? new List<bool>(missionResults) : new List<bool>(),
                playCount = 1,
                lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                bestTurns = stats != null ? stats.turnsPlayed : 0,
                highestDamage = stats != null ? stats.totalDamageDealt : 0
            });

            PlayerStageProgress created = progressList[progressList.Count - 1];
            created.starsEarned = CalculateStarCountFromMissions(created.completedStarMissions, starsEarned);
        }

        SaveCurrentGame(); // บันทึกทันที
    }

    /// <summary>
    /// เช็คว่าด่านนี้ผ่านแล้วหรือยัง
    /// </summary>
    public bool IsStageCleared(string stageID)
    {
        if (CurrentGameData == null) return false;

        var stage = CurrentGameData.stageProgress.FirstOrDefault(s => s.stageID == stageID);
        return stage != null && stage.isCompleted;
    }

    /// <summary>
    /// ดึงข้อมูลความคืบหน้าของ Stage (สำหรับแสดง UI หรือ debug)
    /// </summary>
    public PlayerStageProgress GetStageProgress(string stageID)
    {
        if (CurrentGameData == null) return null;
        return CurrentGameData.stageProgress.FirstOrDefault(s => s.stageID == stageID);
    }

    /// <summary>
    /// ดึงจำนวนดาวรวมของผู้เล่น (ทุกด่าน)
    /// </summary>
    public int GetTotalStarsEarned()
    {
        if (CurrentGameData == null) return 0;
        return CurrentGameData.stageProgress.Sum(s => s.starsEarned);
    }

    // คลิกขวาที่ชื่อสคริปต์ใน Inspector -> เลือก "DEV: Add 5000 Gold"
    [ContextMenu("DEV: Add 5000 Gold")]
    public void Dev_AddGold()
    {
        if (CurrentGameData == null) CurrentGameData = new GameData();

        CurrentGameData.profile.gold += 5000; // เสกเงิน
        SaveCurrentGame(); // บันทึก

        Debug.Log($"เสกเงินเรียบร้อย! ตอนนี้มี: {CurrentGameData.profile.gold} Gold");

        // แจ้งเตือน UI ให้รู้ด้วย
        OnGoldChanged?.Invoke(CurrentGameData.profile.gold);
        OnDataLoaded?.Invoke();
    }
    // ------------------------------------------------------------
    // 🔥 ฟังก์ชันเช็คจำนวนการ์ด (GetCardAmount)
    // ------------------------------------------------------------
    public int GetCardAmount(string cardID)
    {
        // ถ้ายังไม่โหลดข้อมูล ให้ตอบว่ามี 0 ใบ
        if (CurrentGameData == null)
        {
            Debug.LogWarning($"❌ GetCardAmount({cardID}): CurrentGameData is NULL!");
            return 0;
        }

        // ค้นหาการ์ดในกระเป๋า (Inventory) ที่มี ID ตรงกัน
        // (ใช้ FirstOrDefault จำเป็นต้องมี using System.Linq; ด้านบนไฟล์)
        var item = CurrentGameData.cardInventory.FirstOrDefault(x => x.card_id == cardID);
        int result = item != null ? item.quantity : 0;

        // Debug log to see what's returned for crafted cards
        if (cardID.Contains("Fire") || cardID.Contains("Ice"))
        {
            Debug.Log($"📊 GetCardAmount({cardID}): {result} (Total inventory items: {CurrentGameData.cardInventory.Count})");
        }

        // ถ้าเจอ -> ส่งจำนวนกลับไป (item.quantity)
        // ถ้าไม่เจอ -> ส่ง 0 กลับไป
        return result;
    }
}