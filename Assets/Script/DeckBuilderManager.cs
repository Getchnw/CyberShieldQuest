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

    [Header("Statistics UI")] // 🔥 ของใหม่: ส่วนโชว์สถิติ
    public TextMeshProUGUI typeStatText;
    public TextMeshProUGUI costStatText;

    [Header("Prefab")]
    public GameObject cardPrefab;

    // --- ข้อมูล ---
    private List<CardData> allCardsLibrary = new List<CardData>(); 
    private List<DeckData> allDecks = new List<DeckData>(); 
    private int currentDeckIndex = 0; 

    // --- Class เซฟข้อมูล ---
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
        if (searchInput != null) searchInput.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });
        if (filterDropdown != null) filterDropdown.onValueChanged.AddListener(delegate { RefreshLeftPanel(); });

        LoadCardLibrary(); 
        LoadSavedDecks();  
        
        if (allDecks.Count == 0) CreateNewDeck("Starter Deck");

        RefreshDropdown(); 
        RefreshUI();       
    }

    void LoadCardLibrary()
    {
        CardData[] loaded = Resources.LoadAll<CardData>("GameContent/Cards");
        allCardsLibrary = loaded.OrderBy(x => x.cost).ThenBy(x => x.card_id).ToList();
        RefreshLeftPanel();
    }

    void RefreshLeftPanel()
    {
        foreach (Transform child in leftContent) Destroy(child.gameObject);

        string searchText = "";
        if (searchInput != null) searchText = searchInput.text.ToLower();

        int categoryIndex = 0;
        if (filterDropdown != null) categoryIndex = filterDropdown.value;

        foreach (var card in allCardsLibrary)
        {
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
                obj.GetComponent<CardUISlot>().Setup(card, AddToDeck);
            }
        }
    }

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

    void AddToDeck(CardData card) {
        DeckData current = allDecks[currentDeckIndex];
        if (current.cardIds.Count >= 30) return;
        if (current.cardIds.Count(id => id == card.card_id) >= 3) return;

        current.cardIds.Add(card.card_id);
        RefreshUI();
        SaveGame();
    }

    void RemoveFromDeck(CardData card) {
        DeckData current = allDecks[currentDeckIndex];
        if (current.cardIds.Contains(card.card_id)) {
            current.cardIds.Remove(card.card_id);
            RefreshUI();
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

    void RefreshUI() {
        foreach (Transform child in rightContent) Destroy(child.gameObject);
        
        if (allDecks.Count == 0) return;

        DeckData current = allDecks[currentDeckIndex];
        List<CardData> cardsInDeck = new List<CardData>();
        
        foreach (string id in current.cardIds) {
            CardData found = allCardsLibrary.Find(x => x.card_id == id);
            if (found != null) cardsInDeck.Add(found);
        }
        
        cardsInDeck = cardsInDeck.OrderBy(x => x.cost).ToList();

        foreach (var card in cardsInDeck) {
            GameObject obj = Instantiate(cardPrefab, rightContent);
            obj.GetComponent<CardUISlot>().Setup(card, RemoveFromDeck);
        }

        if (countText != null) countText.text = $"Deck: {current.cardIds.Count} / 30";

        // 🔥 เรียกฟังก์ชันคำนวณสถิติ
        UpdateDeckStats(cardsInDeck);
    }

    // 🔥 ฟังก์ชันใหม่: คำนวณและแสดงผลสถิติ
    void UpdateDeckStats(List<CardData> deck)
    {
        if (typeStatText != null)
        {
            int monsterCount = deck.Count(x => x.type == CardType.Monster);
            int spellCount = deck.Count(x => x.type == CardType.Spell);
            int equipCount = deck.Count(x => x.type == CardType.EquipSpell);

            typeStatText.text = $"Monster: {monsterCount}  Spell: {spellCount}  Equip: {equipCount}";
        }

        if (costStatText != null)
        {
            // นับว่าแต่ละ Cost มีกี่ใบ (เช่น Cost 1 มี 5 ใบ)
            string costString = "<b>Cost:</b> ";
            
            // หา Cost สูงสุดที่มีในเด็ค
            int maxCost = deck.Count > 0 ? deck.Max(x => x.cost) : 0;

            for (int i = 0; i <= maxCost; i++)
            {
                int count = deck.Count(x => x.cost == i);
                if (count > 0)
                {
                    costString += $"[{i}]: {count}  ";
                }
            }
            costStatText.text = costString;
        }
    }

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