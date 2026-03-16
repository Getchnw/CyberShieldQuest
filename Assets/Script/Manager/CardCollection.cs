using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class CardCollection : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentGrid;
    public TextMeshProUGUI scrapText;
    public GameObject cardPrefab;

    [Header("Popup References")]
    public CollectionDetailView detailPopup; // 🔥 ลาก Popup ใหม่มาใส่
    public ConfirmationPopup confirmPopup;   // ลาก Popup ยืนยันมาใส่

    private List<CardData> allCardsLibrary;

    private bool IsEnglish()
    {
        return LocalizationSettings.SelectedLocale != null &&
               LocalizationSettings.SelectedLocale.Identifier.Code == "en";
    }

    void Start()
    {
        LoadCardLibrary();
        RefreshUI();

        // ฟังการเปลี่ยนแปลงข้อมูลเพื่อรีเฟรช UI ทันที
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged += RefreshUI;
            GameManager.Instance.OnDataLoaded += RefreshUI;
        }
    }

    private void OnDestroy()
    {
        // ลบ listener เวลาออกจาก scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged -= RefreshUI;
            GameManager.Instance.OnDataLoaded -= RefreshUI;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentGameData != null &&
            scrapText != null)
        {
            if (IsEnglish())
            {
                // English
                scrapText.text = $"Scrap: {GameManager.Instance.CurrentGameData.profile.scrap}";
            }
            else
            {
                scrapText.text = $"ชิ้นส่วน : {GameManager.Instance.CurrentGameData.profile.scrap}";
            }
        }
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
        if (contentGrid == null || cardPrefab == null || allCardsLibrary == null || GameManager.Instance == null)
        {
            return;
        }

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
        if (IsEnglish())
        {
            // English
            ConfirmAction($"Create  {card.cardName} \nCost: {cost} Scrap?", () => StartCoroutine(CraftProcess(card)));
        }
        else
        {
            // Thai
            ConfirmAction($"สร้างการ์ด  {card.cardName} \nใช้ชิ้นส่วนทั้งหมด {cost} ชิ้นส่วน", () => StartCoroutine(CraftProcess(card)));
        }
    }

    void OnDismantleButton(CardData card)
    {
        int val = CraftingSystem.GetDismantleValue(card.rarity);
        if (IsEnglish())
        {
            // English
            ConfirmAction($"Dismantle {card.cardName} \nGain: {val} Scrap?", () => StartCoroutine(DismantleProcess(card)));
        }
        else
        {
            // Thai
            ConfirmAction($"ย่อยการ์ด  {card.cardName} \nได้รับชิ้นส่วนทั้งหมด {val} ชิ้นส่วน", () => StartCoroutine(DismantleProcess(card)));
        }
    }

    // --- Process จริงๆ (Coroutine) ---

    IEnumerator CraftProcess(CardData card)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            yield break;
        }

        int cost = CraftingSystem.GetCraftCost(card.rarity);
        if (GameManager.Instance.CurrentGameData.profile.scrap >= cost)
        {
            GameManager.Instance.CurrentGameData.profile.scrap -= cost;
            GameManager.Instance.AddCardToInventory(card.card_id, 1);
            GameManager.Instance.SaveCurrentGame();

            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.UpdateProgress(QuestType.Card, 1, "craft");
            }

            // 🔥 ปิด confirm + detail popup
            confirmPopup?.Close();
            detailPopup?.Close();

            // สำรองไว้เผื่อ listener หลุด จะได้อัปเดต collection ทันที
            RefreshUI();

            // ให้ Save มีเวลา execute
            yield return null;
        }
    }

    IEnumerator DismantleProcess(CardData card)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            yield break;
        }

        int owned = GameManager.Instance.GetCardAmount(card.card_id);
        if (owned > 0)
        {
            int gain = CraftingSystem.GetDismantleValue(card.rarity);
            GameManager.Instance.CurrentGameData.profile.scrap += gain;
            GameManager.Instance.AddCardToInventory(card.card_id, -1);
            GameManager.Instance.SaveCurrentGame();

            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.UpdateProgress(QuestType.Card, 1, "scrap");
            }

            // 🔥 ปิด confirm + detail popup
            confirmPopup?.Close();
            detailPopup?.Close();

            // สำรองไว้เผื่อ listener หลุด จะได้อัปเดต collection ทันที
            RefreshUI();

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