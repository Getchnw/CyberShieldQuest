using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CollectionDetailView : MonoBehaviour
{
    [Header("Card Info UI")]
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI amountOwnedText; // โชว์จำนวนที่มี

    [Header("Crafting UI")]
    public Button craftButton;
    public TextMeshProUGUI craftCostText;
    public Button dismantleButton;
    public TextMeshProUGUI dismantleValText;

    // เก็บข้อมูลไว้เรียกใช้ตอนกดปุ่ม
    private CardData currentCard;
    private Action<CardData> onCraftAction;
    private Action<CardData> onDismantleAction;

    public void Open(CardData card, Action<CardData> onCraft, Action<CardData> onDismantle)
    {
        gameObject.SetActive(true);
        currentCard = card;
        onCraftAction = onCraft;
        onDismantleAction = onDismantle;

        RefreshView(); // อัปเดตข้อมูล
    }

    public void RefreshView()
    {
        if (currentCard == null) return;

        // 1. แสดงข้อมูลการ์ด (เหมือน CardDetailView ปกติ)
        if (currentCard.artwork != null) artworkImage.sprite = currentCard.artwork;
        nameText.text = currentCard.cardName;
        
        string subCat = currentCard.subCategory != SubCategory.General ? $" / [{currentCard.subCategory}]" : "";
        typeText.text = $"{currentCard.type}{subCat}";

        string stats = $"Cost: {currentCard.cost}";
        if (currentCard.type == CardType.Monster) stats += $" | Atk: {currentCard.atk}";
        statsText.text = stats;

        abilityText.text = currentCard.abilityText;
        flavorText.text = $"<i>{currentCard.flavorText}</i>";

        // 2. ข้อมูลจำนวนที่มี
        int owned = GameManager.Instance.GetCardAmount(currentCard.card_id);
        amountOwnedText.text = $"Owned: {owned}";

        // 3. ตั้งค่าปุ่ม Craft
        int craftCost = CraftingSystem.GetCraftCost(currentCard.rarity);
        int playerScrap = GameManager.Instance.CurrentGameData.profile.scrap;
        
        craftCostText.text = $"Craft\n-{craftCost} Scrap";
        craftButton.interactable = (playerScrap >= craftCost);
        
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => onCraftAction?.Invoke(currentCard));

        // 4. ตั้งค่าปุ่ม Dismantle
        int dismantleVal = CraftingSystem.GetDismantleValue(currentCard.rarity);
        
        dismantleValText.text = $"Dismantle\n+{dismantleVal} Scrap";
        dismantleButton.interactable = (owned > 0); // ต้องมีของถึงจะย่อยได้
        
        dismantleButton.onClick.RemoveAllListeners();
        dismantleButton.onClick.AddListener(() => onDismantleAction?.Invoke(currentCard));
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}