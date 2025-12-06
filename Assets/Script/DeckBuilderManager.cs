using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class DeckBuilderManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform leftContent;  
    public Transform rightContent; 
    public TextMeshProUGUI countText; 
    public TMP_Dropdown deckDropdown;
    public TMP_InputField newDeckInput;

    [Header("Search & Filter")]
    public TMP_InputField searchInput; 
    public TMP_Dropdown filterDropdown;

    [Header("Statistics UI")]
    public TextMeshProUGUI typeStatText;
    public TextMeshProUGUI costStatText;

    [Header("Popup & Prefab")]
    public CardDetailView detailPopup;
    public GameObject cardPrefab;

    // ข้อมูล (ดึงจาก GameDataManager)
    private List<CardData> allCardsLibrary = new List<CardData>(); 
    private int currentDeckIndex = 0; 

    void Start()
    {
        Debug.Log("🔵 DeckBuilderManager: Start() called");
        
        if (searchInput != null) searchInput.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });
        if (filterDropdown != null) filterDropdown.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });

        LoadCardLibrary(); 
        
        // รอให้ GameManager โหลดเสร็จก่อน
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            Debug.Log("🟢 GameManager และ CurrentGameData พบแล้ว");
            var data = GameManager.Instance.CurrentGameData;
            
            // ✅ Initialize สิ่งที่อาจเป็น null
            if (data.decks == null) data.decks = new List<DeckData>();
            if (data.cardInventory == null) data.cardInventory = new List<PlayerCardInventoryItem>();
            
            Debug.Log($"🟢 CardInventory items: {data.cardInventory.Count}");
            
            // ถ้าไม่มีเด็คเลย สร้างให้ 1 อัน
            if (data.decks.Count == 0)
            {
                Debug.Log("🟢 Creating first deck...");
                CreateNewDeck("First Deck");
            }
            
            currentDeckIndex = 0; // ✅ Reset index
            RefreshDropdown(); 
            RefreshUI();
            RefreshLeftPanel(); // เพิ่มเรียกใหม่
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance หรือ CurrentGameData เป็น null!");
        }
    }

    void LoadCardLibrary()
    {
        CardData[] loaded = Resources.LoadAll<CardData>("GameContent/Cards");
        
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning("⚠️ No cards found at 'GameContent/Cards'. Trying alternative paths...");
            // พยายามหาเส้นทางอื่น
            loaded = Resources.LoadAll<CardData>("Cards");
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogError("❌ Card library not found! Check your Resources folder structure.");
                return;
            }
        }
        
        allCardsLibrary = loaded.OrderBy(x => x.cost).ThenBy(x => x.card_id).ToList();
        Debug.Log($"✅ Loaded {allCardsLibrary.Count} cards from library");
    }

    void ShowDetail(CardData card) { if (detailPopup != null) detailPopup.Open(card); }

    // --- ส่วนแสดงผลคลังการ์ด (ซ้าย) ---
    void RefreshLeftPanel()
    {
        Debug.Log("🔵 RefreshLeftPanel() called");
        
        foreach (Transform child in leftContent) Destroy(child.gameObject);

        // ✅ เช็ค UI references
        if (leftContent == null)
        {
            Debug.LogError("❌ leftContent is NULL!");
            return;
        }
        
        if (cardPrefab == null)
        {
            Debug.LogError("❌ cardPrefab is NULL!");
            return;
        }

        // ✅ เช็ค GameManager ก่อนใช้ข้อมูล
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            Debug.LogError("❌ GameManager or CurrentGameData is NULL!");
            return;
        }

        string searchText = (searchInput != null) ? searchInput.text.ToLower() : "";
        int categoryIndex = (filterDropdown != null) ? filterDropdown.value : 0;
        
        // ดึงข้อมูลจาก GameManager
        var data = GameManager.Instance.CurrentGameData;
        
        // ✅ เช็ค null และ index validity
        if (data.decks == null || data.decks.Count == 0 || currentDeckIndex >= data.decks.Count)
        {
            Debug.LogError($"❌ Decks invalid: decks={data.decks}, count={data.decks?.Count ?? -1}, index={currentDeckIndex}");
            return;
        }
            
        DeckData currentDeck = data.decks[currentDeckIndex];

        // ✅ เช็ค cardInventory null
        if (data.cardInventory == null)
        {
            data.cardInventory = new List<PlayerCardInventoryItem>();
            Debug.LogWarning("⚠️ CardInventory was null, initialized it");
        }

        Debug.Log($"📊 CardInventory count: {data.cardInventory.Count}");
        Debug.Log($"📊 AllCardsLibrary count: {allCardsLibrary.Count}");

        int displayedCards = 0;
        int skippedCards = 0;
        foreach (var card in allCardsLibrary)
        {
            // 1. เช็คจำนวนที่มีใน Inventory (จาก GameData)
            int ownedAmount = 0;
            var item = data.cardInventory.FirstOrDefault(x => x.card_id == card.card_id);
            if (item != null) ownedAmount = item.quantity;

            if (ownedAmount <= 0)
            {
                skippedCards++;
                continue;
            }

            // 2. Filter & Search
            bool matchName = string.IsNullOrEmpty(searchText) || 
                             card.cardName.ToLower().Contains(searchText) ||
                             card.abilityText.ToLower().Contains(searchText);

            bool matchCategory = true;
            if (categoryIndex == 1 && card.mainCategory != MainCategory.A01) matchCategory = false;
            if (categoryIndex == 2 && card.mainCategory != MainCategory.A02) matchCategory = false;
            if (categoryIndex == 3 && card.mainCategory != MainCategory.A03) matchCategory = false;

            if (matchName && matchCategory)
            {
                GameObject obj = Instantiate(cardPrefab, leftContent);
                
                // คำนวณจำนวนที่เหลือ (Owned - Used)
                int usedInDeck = 0;
                if (currentDeck != null)
                {
                    usedInDeck = currentDeck.card_ids_in_deck.Count(id => id == card.card_id);
                }
                int remainAmount = ownedAmount - usedInDeck;

                Button btn = obj.GetComponent<Button>();
                if(remainAmount <= 0) btn.interactable = false;

                obj.GetComponent<CardUISlot>().Setup(card, remainAmount, AddToDeck, ShowDetail);
                displayedCards++;
            }
        }

        Debug.Log($"✅ Displayed {displayedCards} cards in left panel (skipped {skippedCards} cards with 0 quantity)");
    }

    // --- จัดการ Deck (Create/Delete) ---
    public void CreateNewDeckButton() {
        string name = newDeckInput.text;
        int count = GameManager.Instance.CurrentGameData.decks.Count;
        if (string.IsNullOrEmpty(name)) name = "Deck " + (count + 1);
        CreateNewDeck(name);
        newDeckInput.text = "";
    }

    void CreateNewDeck(string name) {
        // สร้าง DeckData ใหม่ (ใช้ ID รันตามจำนวน)
        int newId = GameManager.Instance.CurrentGameData.decks.Count + 1;
        DeckData newDeck = new DeckData(newId, name);
        
        GameManager.Instance.CurrentGameData.decks.Add(newDeck);
        GameManager.Instance.SaveCurrentGame(); // เซฟทันที

        currentDeckIndex = GameManager.Instance.CurrentGameData.decks.Count - 1;
        RefreshDropdown(); RefreshUI();
    }

    public void DeleteCurrentDeck() {
        var decks = GameManager.Instance.CurrentGameData.decks;
        if (decks.Count <= 1) return;

        decks.RemoveAt(currentDeckIndex);
        GameManager.Instance.SaveCurrentGame();

        currentDeckIndex = 0;
        RefreshDropdown(); RefreshUI();
    }

    public void OnDropdownChanged(int index) { currentDeckIndex = index; RefreshUI(); }

    // --- ย้ายการ์ด ---
    void AddToDeck(CardData card) {
        var decks = GameManager.Instance.CurrentGameData.decks;
        DeckData current = decks[currentDeckIndex];

        // 1. เช็ค Limit 30 ใบ
        if (current.card_ids_in_deck.Count >= 30) return;

        // 2. เช็คจำนวนที่ผู้เล่นมี (Inventory) vs จำนวนที่ใส่ไปแล้ว
        int owned = 0;
        var item = GameManager.Instance.CurrentGameData.cardInventory.FirstOrDefault(x => x.card_id == card.card_id);
        if (item != null) owned = item.quantity;

        int used = current.card_ids_in_deck.Count(id => id == card.card_id);

        if (used >= 3) return; // กฏห้ามเกิน 3 ใบ
        if (used >= owned) return; // ห้ามเกินจำนวนที่มีจริง

        current.card_ids_in_deck.Add(card.card_id);
        
        GameManager.Instance.SaveCurrentGame();
        RefreshUI(); RefreshLeftPanel();
    }

    void RemoveFromDeck(CardData card) {
        var decks = GameManager.Instance.CurrentGameData.decks;
        DeckData current = decks[currentDeckIndex];

        if (current.card_ids_in_deck.Contains(card.card_id)) {
            current.card_ids_in_deck.Remove(card.card_id);
            GameManager.Instance.SaveCurrentGame();
            RefreshUI(); RefreshLeftPanel();
        }
    }

    // --- UI Update ---
    void RefreshDropdown() {
        if (deckDropdown == null) return;
        
        deckDropdown.ClearOptions();
        List<string> names = new List<string>();
        
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null)
        {
            foreach (var deck in GameManager.Instance.CurrentGameData.decks) 
                names.Add(deck.deck_name);
        }
        
        deckDropdown.AddOptions(names);
        if (names.Count > 0 && currentDeckIndex < names.Count)
            deckDropdown.value = currentDeckIndex;
    }

    void RefreshUI() {
        foreach (Transform child in rightContent) Destroy(child.gameObject);
        
        // ✅ เช็ค null ก่อน
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
            return;
            
        var decks = GameManager.Instance.CurrentGameData.decks;
        if (decks == null || decks.Count == 0 || currentDeckIndex >= decks.Count) 
            return;

        DeckData current = decks[currentDeckIndex];
        List<CardData> cardsInDeck = new List<CardData>();

        // แปลง ID string -> CardData Object
        foreach (string id in current.card_ids_in_deck) {
            CardData found = allCardsLibrary.Find(x => x.card_id == id);
            if (found != null) cardsInDeck.Add(found);
        }
        
        cardsInDeck = cardsInDeck.OrderBy(x => x.cost).ToList();

        foreach (var card in cardsInDeck) {
            GameObject obj = Instantiate(cardPrefab, rightContent);
            // ฝั่งขวาไม่ต้องโชว์จำนวน (-1)
            obj.GetComponent<CardUISlot>().Setup(card, -1, RemoveFromDeck, ShowDetail);
        }

        if (countText != null) countText.text = $"Deck: {current.card_ids_in_deck.Count} / 30";
        UpdateDeckStats(cardsInDeck);
    }

    void UpdateDeckStats(List<CardData> deck) {
        // (ฟังก์ชันเดิม ไม่ต้องแก้)
        if (typeStatText != null) {
            int mon = deck.Count(x => x.type == CardType.Monster);
            int spl = deck.Count(x => x.type == CardType.Spell);
            int eqp = deck.Count(x => x.type == CardType.EquipSpell);
            typeStatText.text = $"Type: Mon {mon}   Spell {spl}   Equip {eqp}";
        }
        if (costStatText != null) {
            string s = "Cost: ";
            int max = deck.Count > 0 ? deck.Max(x => x.cost) : 0;
            for (int i = 0; i <= max; i++) {
                int c = deck.Count(x => x.cost == i);
                if (c > 0) s += $"[{i}]:{c} ";
            }
            costStatText.text = s;
        }
    }
}