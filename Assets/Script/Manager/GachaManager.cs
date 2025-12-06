using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class GachaManager : MonoBehaviour
{
    [Header("Settings")]
    public int costPerPull = 100;

    [Header("Drop Rates")]
    public int commonRate = 60;
    public int rareRate = 30;
    public int epicRate = 8;
    public int legendaryRate = 2;

    [Header("UI References")]
    public TextMeshProUGUI goldText;    
    public GameObject resultPanel;      
    public Transform resultGrid;        
    public GameObject cardDisplayPrefab;

    [Header("Banner UI")]
    public TextMeshProUGUI currentBannerNameText; 
    public Button pullOneButton;   
    public Button pullTenButton;
    [Header("Banner Images")]
    public Image bannerImageDisplay;  // ตัว Image บนหน้าจอที่จะให้เปลี่ยนรูป
    public Sprite[] bannerSprites;    // อาเรย์เก็บรูป 3 รูป (ลากใส่ใน Inspector)
    // ข้อมูลภายใน
    private List<CardData> allCards;
    private MainCategory currentTargetCategory = MainCategory.A01; 

    void Start()
    {
        allCards = Resources.LoadAll<CardData>("GameContent/Cards").ToList();
        
        // เริ่มต้นเลือกตู้ A01
        SelectBanner(1); 
        
        UpdateUI(); // ✅ มีฟังก์ชันรองรับแล้ว
        if(resultPanel != null) resultPanel.SetActive(false);
    }

    void Update()
    {
        UpdateUI(); // อัปเดตเงินตลอดเวลา
    }

    // 🔥 ฟังก์ชันที่เพิ่มเข้ามา
    void UpdateUI()
    {
        if(GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }

    // =========================================================
    // ระบบเลือกตู้ (Select Banner)
    // =========================================================
    public void SelectBanner(int categoryIndex)
    {
        if (categoryIndex == 1) currentTargetCategory = MainCategory.A01;
        else if (categoryIndex == 2) currentTargetCategory = MainCategory.A02;
        else if (categoryIndex == 3) currentTargetCategory = MainCategory.A03;
        else currentTargetCategory = MainCategory.General;

        if (currentBannerNameText != null)
            currentBannerNameText.text = $"Current Banner: {currentTargetCategory}";
        
        if (bannerImageDisplay != null && bannerSprites != null && bannerSprites.Length > 0)
        {
            // categoryIndex ส่งมาเป็น 1, 2, 3
            // แต่อาเรย์เริ่มที่ 0, 1, 2 -> เลยต้องลบ 1
            int spriteIndex = categoryIndex - 1;

            // เช็คกัน Error (เผื่อลืมใส่รูป)
            if (spriteIndex >= 0 && spriteIndex < bannerSprites.Length)
            {
                bannerImageDisplay.sprite = bannerSprites[spriteIndex];
            }
        }
        bool isUnlocked = CheckUnlockStatus(categoryIndex);
        
        if (pullOneButton != null) pullOneButton.interactable = isUnlocked;
        if (pullTenButton != null) pullTenButton.interactable = isUnlocked;

        if (!isUnlocked && currentBannerNameText != null)
            currentBannerNameText.text += " (LOCKED)";
    }

    bool CheckUnlockStatus(int categoryIndex)
    {
        if (GameManager.Instance == null) return true; // กัน Error ตอนเทส

        // 🔥 กำหนดเงื่อนไข: ตู้นี้ต้องผ่าน Chapter ไหนบ้าง? (ใส่เลข ID ของ Chapter ตามจริงของคุณ)
        List<int> requiredChapters = new List<int>();

        switch (categoryIndex)
        {
            case 1: // ตู้ A01 (Broken Access)
                // สมมติว่าเนื้อเรื่อง A01 คือ Chapter 1, 2, 3
                requiredChapters = new List<int> { 1, 2, 3 }; 
                break;

            case 2: // ตู้ A02 (Crypto)
                // สมมติว่าเนื้อเรื่อง A02 คือ Chapter 4, 5, 6, 7, 8
                requiredChapters = new List<int> { 4, 5,6,7,8 }; 
                break;

            case 3: // ตู้ A03 (Injection)
                // สมมติว่าเนื้อเรื่อง A03 คือ Chapter 9, 10, 11, 12
                requiredChapters = new List<int> { 9,10,11,12 }; 
                break;
            
            default:
                return true; // ตู้อื่นๆ เปิดตลอด
        }

        // 🔥 ลูปเช็ค: ต้องผ่าน "ครบทุกบท" ในลิสต์ข้างบน ถึงจะเปิดตู้ได้
        foreach (int chapID in requiredChapters)
        {
            // ดึงข้อมูล Chapter จาก GameManager
            var chapterData = GameManager.Instance.CurrentGameData.chapterProgress
                              .FirstOrDefault(c => c.chapter_id == chapID);

            // ถ้าหาไม่เจอ หรือ ยังเล่นไม่จบ (is_completed = false) -> ล็อคตู้ทันที 🔒
            if (chapterData == null || !chapterData.is_completed)
            {
                return false; 
            }
        }

        // ถ้าวนลูปจนจบแล้วไม่ติดขัดอะไร แปลว่าผ่านครบหมดแล้ว -> ปลดล็อค! 🔓
        return true; 
    }

    // =========================================================
    // ปุ่มกดสุ่ม
    // =========================================================
    public void PullOne()
    {
        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;
        if (currentGold >= costPerPull)
        {
            GameManager.Instance.DecreaseGold(costPerPull);
            
            CardData pulledCard = RandomCard(currentTargetCategory);
            
            GameManager.Instance.AddCardToInventory(pulledCard.card_id, 1);
            GameManager.Instance.SaveCurrentGame();
            ShowResult(new List<CardData> { pulledCard });
        }
        else Debug.Log("เงินไม่พอ!");
    }

    public void PullTen()
    {
        int totalCost = costPerPull * 10;
        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;

        if (currentGold >= totalCost)
        {
            GameManager.Instance.DecreaseGold(totalCost);
            List<CardData> pulledList = new List<CardData>();
            for (int i = 0; i < 10; i++)
            {
                CardData c = RandomCard(currentTargetCategory);
                pulledList.Add(c);
                GameManager.Instance.AddCardToInventory(c.card_id, 1);
            }
            GameManager.Instance.SaveCurrentGame();
            ShowResult(pulledList);
        }
        else Debug.Log("เงินไม่พอ!");
    }

    // =========================================================
    // Logic การสุ่ม (กรองตามตู้)
    // =========================================================
    CardData RandomCard(MainCategory targetCategory)
    {
        int rng = Random.Range(0, 100);
        Rarity targetRarity = Rarity.Common;

        if (rng < legendaryRate) targetRarity = Rarity.Legendary;
        else if (rng < legendaryRate + epicRate) targetRarity = Rarity.Epic;
        else if (rng < legendaryRate + epicRate + rareRate) targetRarity = Rarity.Rare;
        else targetRarity = Rarity.Common;

        // กรอง 2 ชั้น: Rarity + Category
        List<CardData> pool = allCards.FindAll(x => x.rarity == targetRarity && x.mainCategory == targetCategory);

        // Fallback: ถ้าไม่มีของระดับนั้นในตู้นี้ ให้สุ่ม Common ของตู้นี้แทน
        if (pool.Count == 0) 
        {
            pool = allCards.FindAll(x => x.rarity == Rarity.Common && x.mainCategory == targetCategory);
        }
        
        // Fallback สุดท้าย: สุ่มมั่วๆ จากทั้งหมด (กัน Error)
        if (pool.Count == 0) pool = allCards;

        return pool[Random.Range(0, pool.Count)];
    }

   

    void ShowResult(List<CardData> cards)
    {
        if(resultPanel != null) resultPanel.SetActive(true);
        
        if(resultGrid != null)
        {
            // ล้างของเก่า
            foreach(Transform child in resultGrid) Destroy(child.gameObject);
            
            // ใช้ Coroutine เพื่อหน่วงเวลา
            StartCoroutine(SpawnCardsRoutine(cards));
        }
    }

    // ฟังก์ชันสร้างการ์ดทีละใบแบบมีอนิเมชั่น
    IEnumerator SpawnCardsRoutine(List<CardData> cards)
    {
        foreach(var card in cards)
        {
            GameObject obj = Instantiate(cardDisplayPrefab, resultGrid);
            var slot = obj.GetComponent<CardUISlot>();
            if(slot != null) slot.Setup(card, -1, null, null); 

            // เริ่มต้นที่ขนาด 0 (ซ่อนอยู่)
            obj.transform.localScale = Vector3.zero;

            // สั่งให้ขยายขึ้นมา (Scale Up)
            float timer = 0;
            float duration = 0.3f; // ใช้เวลา 0.3 วิ
            while(timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                // สูตร BackEaseOut (ทำให้เด้งดึ๋งเกินนิดนึงแล้วหดกลับ)
                float ease = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
                
                obj.transform.localScale = Vector3.one * ease;
                yield return null;
            }
            obj.transform.localScale = Vector3.one; // จบที่ขนาดปกติ

            // รอแป๊บนึงค่อยเสกใบต่อไป
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void CloseResult() { if(resultPanel != null) resultPanel.SetActive(false); }
}