using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

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
    public Button closeResultButton; 

    [Header("Banner UI")]
    public TextMeshProUGUI currentBannerNameText; 
    public Button pullOneButton;   
    public Button pullTenButton;
    public Image bannerImageDisplay;  
    public Sprite[] bannerSprites;    

    [Header("Banner Selection Buttons")]
    public Button[] bannerSelectButtons; 

    private List<CardData> allCards;
    private MainCategory currentTargetCategory = MainCategory.A01; 

    void Start()
    {
        allCards = Resources.LoadAll<CardData>("GameContent/Cards").ToList();
        
        if (closeResultButton != null) closeResultButton.onClick.AddListener(CloseResult);

        UpdateBannerButtonsVisual();
        SelectBanner(1); 
        UpdateUI();
        
        if(resultPanel != null) resultPanel.SetActive(false);
    }

    void Update() { UpdateUI(); }

    void UpdateUI()
    {
        if(GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }
    }

    // =========================================================
    // 🔥 แก้ไขการเช็ค Unlock (อิงจาก statusPostTest ใน GameData)
    // =========================================================
    // =========================================================
    // 🔥 แก้ไขเงื่อนไข: เรียนจบบทไหน ถึงจะสุ่มบทนั้นได้
    // =========================================================
    bool CheckUnlockStatus(int categoryIndex)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            // ดึงข้อมูลการสอบผ่าน (Post-Test)
            var statusPost = GameManager.Instance.CurrentGameData.statusPostTest;

            switch (categoryIndex)
            {
                case 1: // ตู้ A01 -> ต้องจบ A01 ก่อน
                    return statusPost.hasSucessPost_A01;

                case 2: // ตู้ A02 -> ต้องจบ A02 ก่อน
                    return statusPost.hasSucessPost_A02;

                case 3: // ตู้ A03 -> ต้องจบ A03 ก่อน
                    return statusPost.hasSucessPost_A03;
            }
        }

        // กรณี Test Mode (หรือหาข้อมูลไม่เจอ) ให้ล็อคไว้ก่อนเพื่อความปลอดภัย
        return false; 
    }

    // =========================================================
    // ระบบเลือกตู้ & UI
    // =========================================================
    public void SelectBanner(int categoryIndex)
    {
        if (categoryIndex == 1) currentTargetCategory = MainCategory.A01;
        else if (categoryIndex == 2) currentTargetCategory = MainCategory.A02;
        else if (categoryIndex == 3) currentTargetCategory = MainCategory.A03;
        else currentTargetCategory = MainCategory.General;

        // เปลี่ยนรูปแบนเนอร์
        if (bannerImageDisplay != null && bannerSprites != null && bannerSprites.Length > 0)
        {
            int spriteIndex = categoryIndex - 1;
            if (spriteIndex >= 0 && spriteIndex < bannerSprites.Length)
                bannerImageDisplay.sprite = bannerSprites[spriteIndex];
        }

        // เช็คล็อค
        bool isUnlocked = CheckUnlockStatus(categoryIndex);
        
        // คุมปุ่มสุ่ม (ซ่อนปุ่มถ้ายังไม่ปลดล็อค)
        if (pullOneButton != null) pullOneButton.gameObject.SetActive(isUnlocked);
        if (pullTenButton != null) pullTenButton.gameObject.SetActive(isUnlocked);

        // อัปเดตข้อความ
        if (currentBannerNameText != null)
        {
            currentBannerNameText.text = $"Banner: {currentTargetCategory}";
            
            if (!isUnlocked)
            {
                currentBannerNameText.text += " <color=red> ( LOCKED )</color>";
                if(bannerImageDisplay != null) bannerImageDisplay.color = Color.gray; 
            }
            else
            {
                if(bannerImageDisplay != null) bannerImageDisplay.color = Color.white;
            }
        }
    }

    void UpdateBannerButtonsVisual()
    {
        if (bannerSelectButtons == null) return;

        for (int i = 0; i < bannerSelectButtons.Length; i++)
        {
            int categoryIndex = i + 1; 
            bool isUnlocked = CheckUnlockStatus(categoryIndex);
            
            Button btn = bannerSelectButtons[i];
            if (btn == null) continue;

            ColorBlock colors = btn.colors;
            if (isUnlocked)
            {
                colors.normalColor = Color.white;
            }
            else
            {
                colors.normalColor = new Color(0.4f, 0.4f, 0.4f, 1f); // สีเทาเข้ม
            }
            btn.colors = colors;
            
            // บังคับเปลี่ยนสีรูปปุ่มทันที
            if(btn.image != null) btn.image.color = colors.normalColor;
        }
    }

    // Helper: แปลง MainCategory เป็น index สำหรับเช็ค Unlock
    int GetCategoryIndex(MainCategory cat)
    {
        if (cat == MainCategory.A01) return 1;
        if (cat == MainCategory.A02) return 2;
        if (cat == MainCategory.A03) return 3;
        return 0; // General
    }

    // =========================================================
    // ระบบสุ่ม (Gacha Logic)
    // =========================================================
    public void PullOne()
    {
        Debug.Log("🔵 PullOne() called");
        
        // เช็คเงื่อนไขอีกรอบกันเหนียว (เผื่อแฮกปุ่ม)
        int catIndex = GetCategoryIndex(currentTargetCategory);
        if (!CheckUnlockStatus(catIndex))
        {
            Debug.LogWarning("❌ Banner ยังไม่ปลดล็อค!");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            Debug.LogError("❌ GameManager is NULL!");
            return;
        }

        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;
        Debug.Log($"💰 Current Gold: {currentGold}, Cost: {costPerPull}");
        
        if (currentGold >= costPerPull)
        {
            GameManager.Instance.DecreaseGold(costPerPull);
            CardData pulledCard = RandomCard(currentTargetCategory);
            GameManager.Instance.AddCardToInventory(pulledCard.card_id, 1);
            GameManager.Instance.SaveCurrentGame();
            Debug.Log($"✅ Pulled: {pulledCard.cardName}");
            ShowResult(new List<CardData> { pulledCard });
        }
        else Debug.LogWarning($"❌ เงินไม่พอ! มี {currentGold} ต้องการ {costPerPull}");
    }

    public void PullTen()
    {
        Debug.Log("🔵 PullTen() called");
        
        int catIndex = GetCategoryIndex(currentTargetCategory);
        if (!CheckUnlockStatus(catIndex))
        {
            Debug.LogWarning("❌ Banner ยังไม่ปลดล็อค!");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            Debug.LogError("❌ GameManager is NULL!");
            return;
        }

        int totalCost = costPerPull * 10;
        int currentGold = GameManager.Instance.CurrentGameData.profile.gold;
        Debug.Log($"💰 Current Gold: {currentGold}, Total Cost: {totalCost}");

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
            Debug.Log($"✅ Pulled {pulledList.Count} cards");
            ShowResult(pulledList);
        }
        else Debug.Log("เงินไม่พอ!");
    }

    CardData RandomCard(MainCategory targetCategory)
    {
        int rng = Random.Range(0, 100);
        Rarity targetRarity = Rarity.Common;

        if (rng < legendaryRate) targetRarity = Rarity.Legendary;
        else if (rng < legendaryRate + epicRate) targetRarity = Rarity.Epic;
        else if (rng < legendaryRate + epicRate + rareRate) targetRarity = Rarity.Rare;
        else targetRarity = Rarity.Common;

        List<CardData> pool = allCards.FindAll(x => x.rarity == targetRarity && x.mainCategory == targetCategory);

        if (pool.Count == 0) pool = allCards.FindAll(x => x.rarity == Rarity.Common && x.mainCategory == targetCategory);
        if (pool.Count == 0) pool = allCards; // Fallback สุดท้าย

        return pool[Random.Range(0, pool.Count)];
    }

    // =========================================================
    // แสดงผล
    // =========================================================
    void ShowResult(List<CardData> cards)
    {
        if(resultPanel != null) resultPanel.SetActive(true);
        if(resultGrid != null)
        {
            foreach(Transform child in resultGrid) Destroy(child.gameObject);
            StartCoroutine(SpawnCardsRoutine(cards));
        }
    }

    IEnumerator SpawnCardsRoutine(List<CardData> cards)
    {
        yield return null; 
        foreach(var card in cards)
        {
            GameObject obj = Instantiate(cardDisplayPrefab, resultGrid);
            var slot = obj.GetComponent<CardUISlot>();
            if(slot != null) slot.Setup(card, -1, null, null); 

            // Animation
            obj.transform.localScale = Vector3.zero;
            float timer = 0;
            while(timer < 0.3f)
            {
                timer += Time.deltaTime;
                float t = timer / 0.3f;
                float ease = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
                obj.transform.localScale = Vector3.one * ease;
                yield return null;
            }
            obj.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void CloseResult() 
    { 
        if(resultPanel != null) resultPanel.SetActive(false); 
    }
}