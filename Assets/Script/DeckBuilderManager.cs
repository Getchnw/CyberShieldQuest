using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class DeckBuilderManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform leftContent;  // ช่องการ์ดรวม (ซ้าย)
    public Transform rightContent; // ช่องเด็คปัจจุบัน (ขวา)
    public TextMeshProUGUI countText; 
    public TMP_Dropdown deckDropdown; // Dropdown เลือกเด็ค
    public TMP_InputField newDeckInput; // ช่องตั้งชื่อเด็คใหม่

    [Header("Search & Filter")]
    public TMP_InputField searchInput; 
    public TMP_Dropdown filterDropdown;

    [Header("Statistics UI")]
    public TextMeshProUGUI typeStatText;
    public TextMeshProUGUI costStatText;

    [Header("Popup Reference")]
    public CardDetailView detailPopup; // ลาก CardDetailPanel มาใส่ตรงนี้

    [Header("Prefab")]
    public GameObject cardPrefab;

    // --- ข้อมูล ---
    private List<CardData> allCardsLibrary = new List<CardData>(); 
    private List<DeckData> allDecks = new List<DeckData>(); 
    private int currentDeckIndex = 0; 

    // --- Class เซฟข้อมูลเด็ค ---
    [System.Serializable]
    public class DeckData {
        public string deckName;
        public List<string> cardIds = new List<string>();
    }

    [System.Serializable]
    public class SaveSystemWrapper {
        public List<DeckData> savedDecks;
    }

    void Start()
    {
        // ผูกฟังก์ชันค้นหาอัตโนมัติ
        if (searchInput != null) searchInput.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });
        if (filterDropdown != null) filterDropdown.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });

        LoadCardLibrary(); 
        LoadSavedDecks();  
        
        // ถ้าเปิดมาไม่มีเด็คเลย ให้สร้างอันแรกให้
        if (allDecks.Count == 0) CreateNewDeck("Starter Deck");

        RefreshDropdown(); 
        RefreshUI(); // วาดหน้าจอครั้งแรก
    }

    void LoadCardLibrary()
    {
        // โหลด Blueprint การ์ดทั้งหมดจากเกม
        CardData[] loaded = Resources.LoadAll<CardData>("GameContent/Cards");
        allCardsLibrary = loaded.OrderBy(x => x.cost).ThenBy(x => x.card_id).ToList();
    }

    // ฟังก์ชันเปิด Popup (ส่งไปให้ CardUISlot)
    void ShowDetail(CardData card)
    {
        if (detailPopup != null)
        {
            detailPopup.Open(card);
        }
    }

    // =================================================================
    // 🔥 ส่วนแสดงผลฝั่งซ้าย (Collection) - เชื่อมกับ Inventory
    // =================================================================
    void RefreshLeftPanel()
    {
        foreach (Transform child in leftContent) Destroy(child.gameObject);

        string searchText = "";
        if (searchInput != null) searchText = searchInput.text.ToLower();

        int categoryIndex = 0;
        if (filterDropdown != null) categoryIndex = filterDropdown.value;

        // ดึงข้อมูลเด็คปัจจุบัน (เพื่อเช็คว่าใส่ไปกี่ใบแล้ว)
        DeckData currentDeckData = (allDecks.Count > 0) ? allDecks[currentDeckIndex] : null;

        foreach (var card in allCardsLibrary)
        {
            // ---------------------------------------------------------
            // 1. เช็คจำนวนที่ผู้เล่นมี (Inventory Check)
            // ---------------------------------------------------------
            int ownedAmount = 0;
            if (PlayerSaveManager.Instance != null)
            {
                ownedAmount = PlayerSaveManager.Instance.GetCardAmount(card.card_id);
            }
            else
            {
                ownedAmount = 99; // โหมดทดสอบ (ถ้าไม่มี SaveManager)
            }

            // ถ้าไม่มีสักใบ ไม่ต้องโชว์
            if (ownedAmount <= 0) continue;

            // ---------------------------------------------------------
            // 2. เช็คเงื่อนไข Filter & Search
            // ---------------------------------------------------------
            bool matchName = string.IsNullOrEmpty(searchText) || 
                             card.cardName.ToLower().Contains(searchText) ||
                             card.abilityText.ToLower().Contains(searchText);

            bool matchCategory = true;
            // 0=All, 1=A01, 2=A02, 3=A03 (เรียงตาม Dropdown)
            if (categoryIndex == 1 && card.mainCategory != MainCategory.A01) matchCategory = false;
            if (categoryIndex == 2 && card.mainCategory != MainCategory.A02) matchCategory = false;
            if (categoryIndex == 3 && card.mainCategory != MainCategory.A03) matchCategory = false;

            if (matchName && matchCategory)
            {
                GameObject obj = Instantiate(cardPrefab, leftContent);
                
                // 🔥 คำนวณจำนวนที่เหลือ (จำนวนที่มี - จำนวนที่ใส่ในเด็คไปแล้ว)
                int usedInDeck = 0;
                if (currentDeckData != null)
                {
                    usedInDeck = currentDeckData.cardIds.Count(id => id == card.card_id);
                }
                int remainAmount = ownedAmount - usedInDeck;

                // ส่งจำนวนที่เหลือไปโชว์
                CardUISlot slot = obj.GetComponent<CardUISlot>();
                slot.Setup(card, remainAmount, AddToDeck, ShowDetail);
                
                // ถ้าใช้จนหมดแล้ว ให้กดปุ่มไม่ได้ (Button Interactable = false)
                if (remainAmount <= 0) 
                {
                    obj.GetComponent<Button>().interactable = false;
                }
            }
        }
    }

    // =================================================================
    // ส่วนจัดการ Deck (Create / Delete / Dropdown)
    // =================================================================
    public void CreateNewDeckButton() {
        string name = newDeckInput.text;
        if (string.IsNullOrEmpty(name)) name = "New Deck " + (allDecks.Count + 1);
        CreateNewDeck(name);
        newDeckInput.text = "";
    }

    void CreateNewDeck(string deckName) {
        DeckData newDeck = new DeckData();
        newDeck.deckName = deckName;
        allDecks.Add(newDeck);
        currentDeckIndex = allDecks.Count - 1;
        RefreshDropdown();
        RefreshUI();
        SaveGame();
    }

    public void OnDropdownChanged(int index) {
        currentDeckIndex = index;
        RefreshUI();
    }

    public void DeleteCurrentDeck() {
        if (allDecks.Count <= 1) return;
        allDecks.RemoveAt(currentDeckIndex);
        currentDeckIndex = 0;
        RefreshDropdown();
        RefreshUI();
        SaveGame();
    }

    // =================================================================
    // ส่วนย้ายการ์ด (Add / Remove)
    // =================================================================
    void AddToDeck(CardData card) {
        DeckData current = allDecks[currentDeckIndex];
        
        // เช็คลิมิตเด็ค (30 ใบ)
        if (current.cardIds.Count >= 30) return;

        // เช็คลิมิตการ์ดซ้ำ (ไม่เกิน 3)
        // AND เช็คว่ามีของพอไหม? (ป้องกันการแฮก)
        int owned = (PlayerSaveManager.Instance != null) ? PlayerSaveManager.Instance.GetCardAmount(card.card_id) : 99;
        int used = current.cardIds.Count(id => id == card.card_id);

        if (used >= 3) return; // กฏห้ามเกิน 3
        if (used >= owned) return; // กฏห้ามเกินที่มี

        current.cardIds.Add(card.card_id);
        RefreshUI(); // อัปเดตขวา
        RefreshLeftPanel(); // อัปเดตซ้าย (ลดจำนวน)
        SaveGame();
    }

    void RemoveFromDeck(CardData card) {
        DeckData current = allDecks[currentDeckIndex];
        if (current.cardIds.Contains(card.card_id)) {
            current.cardIds.Remove(card.card_id);
            RefreshUI(); // อัปเดตขวา
            RefreshLeftPanel(); // อัปเดตซ้าย (คืนจำนวน)
            SaveGame();
        }
    }

    void RefreshDropdown() {
        deckDropdown.ClearOptions();
        List<string> names = new List<string>();
        foreach (var deck in allDecks) names.Add(deck.deckName);
        deckDropdown.AddOptions(names);
        deckDropdown.value = currentDeckIndex;
    }

    // =================================================================
    // ส่วนแสดงผลฝั่งขวา (My Deck)
    // =================================================================
    void RefreshUI() {
        foreach (Transform child in rightContent) Destroy(child.gameObject);
        
        if (allDecks.Count == 0) return;

        DeckData current = allDecks[currentDeckIndex];
        List<CardData> cardsInDeck = new List<CardData>();
        
        // แปลง ID เป็น CardData
        foreach (string id in current.cardIds) {
            CardData found = allCardsLibrary.Find(x => x.card_id == id);
            if (found != null) cardsInDeck.Add(found);
        }
        
        // เรียงลำดับ Cost
        cardsInDeck = cardsInDeck.OrderBy(x => x.cost).ToList();

        foreach (var card in cardsInDeck) {
            GameObject obj = Instantiate(cardPrefab, rightContent);
            // ส่ง -1 ไปช่อง amount เพราะฝั่งขวาไม่ต้องโชว์ x จำนวน
            obj.GetComponent<CardUISlot>().Setup(card, -1, RemoveFromDeck, ShowDetail);
        }

        if (countText != null) countText.text = $"Deck: {current.cardIds.Count} / 30";

        UpdateDeckStats(cardsInDeck);
        RefreshLeftPanel(); // เรียกฝั่งซ้ายให้อัปเดตตามด้วย
    }

    void UpdateDeckStats(List<CardData> deck)
    {
        if (typeStatText != null)
        {
            int monsterCount = deck.Count(x => x.type == CardType.Monster);
            int spellCount = deck.Count(x => x.type == CardType.Spell);
            int equipCount = deck.Count(x => x.type == CardType.EquipSpell);
            typeStatText.text = $"Type: Mon {monsterCount} | Spell {spellCount} | Equip {equipCount}";
        }

        if (costStatText != null)
        {
            string costString = "Cost: ";
            int maxCost = deck.Count > 0 ? deck.Max(x => x.cost) : 0;
            for (int i = 0; i <= maxCost; i++)
            {
                int count = deck.Count(x => x.cost == i);
                if (count > 0) costString += $"[{i}]:{count} ";
            }
            costStatText.text = costString;
        }
    }

    // =================================================================
    // ระบบ Save / Load
    // =================================================================
    void SaveGame() {
        SaveSystemWrapper wrapper = new SaveSystemWrapper();
        wrapper.savedDecks = allDecks;
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("MyCardGameSave", json);
        PlayerPrefs.Save();
    }

    void LoadSavedDecks() {
        if (PlayerPrefs.HasKey("MyCardGameSave")) {
            string json = PlayerPrefs.GetString("MyCardGameSave");
            SaveSystemWrapper wrapper = JsonUtility.FromJson<SaveSystemWrapper>(json);
            allDecks = wrapper.savedDecks;
        }
    }
}