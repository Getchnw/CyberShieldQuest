using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentGrid;
    public TextMeshProUGUI scrapText;
    public GameObject cardPrefab;

    [Header("Popup References")]
    public CollectionDetailView detailPopup; // 🔥 ลาก Popup ใหม่มาใส่
    public ConfirmationPopup confirmPopup;   // ลาก Popup ยืนยันมาใส่

    private List<CardData> allCardsLibrary;

    void Start()
    {
        LoadCardLibrary();
        RefreshUI();
        
        // 🔥 ฟังการเปลี่ยนแปลง inventory
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged += RefreshUI;
        }
    }
    
    private void OnDestroy()
    {
        // 🔥 ลบ listener เวลาออกจาก scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && scrapText != null)
            scrapText.text = $"Scrap: {GameManager.Instance.CurrentGameData.profile.scrap}";
    }

    void LoadCardLibrary()
    {
        CardData[] loaded = Resources.LoadAll<CardData>("GameContent/Cards");
        allCardsLibrary = loaded
            .Where(x => x.type != CardType.Token) // ซ่อน Token ไม่ให้โชว์ใน Collection
            .OrderBy(x => x.cost)
            .ThenBy(x => x.card_id)
            .ToList();
    }

    void RefreshUI()
    {
        foreach (Transform child in contentGrid) Destroy(child.gameObject);

        foreach (var card in allCardsLibrary)
        {
            GameObject obj = Instantiate(cardPrefab, contentGrid);
            CardUISlot slot = obj.GetComponent<CardUISlot>();

            int owned = GameManager.Instance.GetCardAmount(card.card_id);

            // เมื่อกดการ์ด -> เปิด Popup ใหม่
            slot.Setup(card, owned, OnCardClicked, null);

            if (owned <= 0) slot.cardImage.color = Color.gray;
        }
    }

    // 🔥 เปิด Popup รายละเอียด
    void OnCardClicked(CardData card)
    {
        if (detailPopup != null)
        {
            detailPopup.Open(card, OnCraftButton, OnDismantleButton);
        }
    }

    // --- Logic การกดปุ่ม (ส่งไปให้ Popup เรียกใช้) ---

    void OnCraftButton(CardData card)
    {
        int cost = CraftingSystem.GetCraftCost(card.rarity);
        ConfirmAction($"Create  {card.cardName} \nCost: {cost} Scrap?", () => StartCoroutine(CraftProcess(card)));
    }

    void OnDismantleButton(CardData card)
    {
        int val = CraftingSystem.GetDismantleValue(card.rarity);
        ConfirmAction($"Dismantle {card.cardName} \nGain: {val} Scrap?", () => StartCoroutine(DismantleProcess(card)));
    }

    // --- Process จริงๆ (Coroutine) ---

    IEnumerator CraftProcess(CardData card)
    {
        int cost = CraftingSystem.GetCraftCost(card.rarity);
        if (GameManager.Instance.CurrentGameData.profile.scrap >= cost)
        {
            GameManager.Instance.CurrentGameData.profile.scrap -= cost;
            GameManager.Instance.AddCardToInventory(card.card_id, 1);
            GameManager.Instance.SaveCurrentGame();

            DailyQuestManager.Instance.UpdateProgress(QuestType.Card, 1, "craft");

            // 🔥 ปิด confirm + detail popup
            confirmPopup?.Close();
            detailPopup?.Close();
            
            // ให้ Save มีเวลา execute
            yield return null;
        }
    }

    IEnumerator DismantleProcess(CardData card)
    {
        int owned = GameManager.Instance.GetCardAmount(card.card_id);
        if (owned > 0)
        {
            int gain = CraftingSystem.GetDismantleValue(card.rarity);
            GameManager.Instance.CurrentGameData.profile.scrap += gain;
            GameManager.Instance.AddCardToInventory(card.card_id, -1);
            GameManager.Instance.SaveCurrentGame();

            DailyQuestManager.Instance.UpdateProgress(QuestType.Card, 1, "scrap");
            
            // 🔥 ปิด confirm + detail popup
            confirmPopup?.Close();
            detailPopup?.Close();
            
            // ให้ Save มีเวลา execute
            yield return null;
        }
    }

    void ConfirmAction(string message, System.Action action)
    {
        Debug.Log($"🔵 ConfirmAction: {message}");

        if (confirmPopup != null)
        {
            Debug.Log("✅ Opening confirmation popup");
            confirmPopup.Open(message, action);
        }
        else
        {
            Debug.LogWarning("⚠️ confirmPopup is NULL! Executing action immediately");
            action?.Invoke();
        }
    }

}