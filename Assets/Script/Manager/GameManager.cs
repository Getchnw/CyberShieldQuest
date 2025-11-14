using UnityEngine;
using System.Linq; //ใช้ .FirstOrDefault()
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // สร้าง "ตัวแปรกลาง" ให้ทุกคนในเกมเรียกใช้ได้ง่ายๆ
    // เราเรียกเทคนิคนี้ว่า Singleton Pattern
    public static GameManager Instance { get; private set; }

    // ตัวแปรสำหรับเก็บข้อมูลผู้เล่นคนปัจจุบัน
    public GameData CurrentGameData { get; private set; }
    public event System.Action OnDataLoaded;
    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnExperienceChanged;

    // Awake() จะทำงานเป็นฟังก์ชันแรกสุดตอนที่ Object นี้ถูกสร้างขึ้น
    private void Awake()
    {
        // --- โค้ดส่วน Singleton ---
        // เช็คว่าในเกมมี GameManager อยู่แล้วหรือยัง?
        if (Instance == null)
        {
            // ถ้ายังไม่มี... ให้ตัวเรานี่แหละเป็น GameManager หลัก!
            Instance = this;

            // *** คำสั่งที่สำคัญที่สุด ***
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
        // Try to load saved data; if none exists create a new one
        bool loaded = LoadGame();
        if (!loaded)
        {
            CreateNewGame();
        }
    }

    // เมธอดสำหรับ "เริ่มเกมใหม่"
    public void CreateNewGame()
    {
        // สร้างข้อมูลผู้เล่นชุดใหม่ขึ้นมา (จาก Constructor ใน GameData.cs)
        CurrentGameData = new GameData();
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
            Debug.Log("Game data loaded successfully.");
            OnDataLoaded?.Invoke();
            return true;
        }
        else
        {
            Debug.LogError("Failed to load game data from file.");
            return false;
        }
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

    // Add experience to player
    public void AddExperience(int amount)
    {
        if (CurrentGameData == null) return;

        CurrentGameData.profile.experience += amount;

        // (Optional) สั่งเซฟอัตโนมัติ
        SaveCurrentGame();

        // (Optional) ส่ง Event บอก UI ให้อัปเดต
        OnExperienceChanged?.Invoke(CurrentGameData.profile.experience);
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
    public void UpdateQuizProgress(int quizID, int highestScore, bool isCompleted )
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
            // หา chapter id จาก quiz id โดยใช้ฐานข้อมูล ChapterEventsData
            int chapterId = GameContentDatabase.Instance.GetChapterIdByQuizId(quizID);
            if (chapterId >= 0)
            {
                AdvanceChapterProgress(chapterId, stars_earned, highestScore);
            }
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
            // หา chapter id จาก quiz id โดยใช้ฐานข้อมูล ChapterEventsData
            int chapterId = GameContentDatabase.Instance.GetChapterIdByQuizId(quizID);
            if (chapterId >= 0)
            {
                AdvanceChapterProgress(chapterId, stars_earned, highestScore);
            }
        }
        SaveCurrentGame();
    }


    /// บันทึกความคืบหน้าของ Story Chapter
    public void AdvanceChapterProgress(int chapterID , int stars_earned ,int score)
    {
         if (CurrentGameData == null) return;
         
         var chapter = CurrentGameData.chapterProgress;
         PlayerChapterProgress chapterProgress = chapter.FirstOrDefault(c => c.chapter_id == chapterID);
         
        // 1. ถ้าเคยเล่นแล้ว: อัปเดตความคืบหน้า
         if (chapterProgress != null)
         {
            chapterProgress.is_completed = true;
             // อัปเดต Event ล่าสุดที่ผ่าน
             if(stars_earned > chapterProgress.stars_earned)
             {
                 chapterProgress.stars_earned = stars_earned;
             }
            if(score > chapterProgress.high_score)
            {
                chapterProgress.high_score = score;
            }
         }
         else
         {
             // เริ่ม Chapter นี้เป็นครั้งแรก
             CurrentGameData.chapterProgress.Add(new PlayerChapterProgress
             {
                chapter_id = chapterID,
                is_completed = true,
                stars_earned = stars_earned,
                high_score = score
             });
         }
         SaveCurrentGame();
    }


    /// บันทึกว่าผ่านด่าน Stage แล้ว
    public void CompleteStage(int stageID, int starsEarned)
    {
        if (CurrentGameData == null) return;

        var progressList = CurrentGameData.stageProgress;
        PlayerStageProgress stageProgress = progressList.FirstOrDefault(s => s.stage_id == stageID);

        if (stageProgress != null)
        {
            // 1. ถ้าเคยเล่นแล้ว: อัปเดตดาว (ถ้าทำได้ดีขึ้น)
            stageProgress.is_completed = true;
            if (starsEarned > stageProgress.stars_earned)
            {
                stageProgress.stars_earned = starsEarned;
            }
        }
        else
        {
            // 2. ถ้าเล่นครั้งแรก: สร้างข้อมูลใหม่
            progressList.Add(new PlayerStageProgress 
            {
                stage_id = stageID,
                is_completed = true,
                stars_earned = starsEarned
            });
        }
    }


}