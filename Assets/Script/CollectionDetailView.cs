using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization.Settings;

public class CollectionDetailView : MonoBehaviour
{
    [Header("Card Info UI")]
    public Image artworkImage;
    public Image frameImage; // 🔥 กรอบการ์ด
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI amountOwnedText; // โชว์จำนวนที่มี

    [Header("Card Frame")]
    public Sprite commonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    public Sprite legendaryFrame;

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
        currentCard = card;
        onCraftAction = onCraft;
        onDismantleAction = onDismantle;

        // 🔥 ลบ listeners เก่าก่อน
        if (craftButton != null) craftButton.onClick.RemoveAllListeners();
        if (dismantleButton != null) dismantleButton.onClick.RemoveAllListeners();

        gameObject.SetActive(true);
        RefreshView(); // อัปเดตข้อมูล

        // 🔥 ฟังการเปลี่ยนแปลง inventory เพื่ออัปเดต UI
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged += RefreshView;
        }

        // 🔥 Debug: แสดงสถานะตัวแปร
        Debug.Log($"[CollectionDetailView] Opened: {card.cardName} | onCraftAction={onCraftAction != null} | onDismantleAction={onDismantleAction != null}");
    }

    public void RefreshView()
    {
        if (currentCard == null) return;

        // 1. แสดงข้อมูลการ์ด (เหมือน CardDetailView ปกติ)
        if (currentCard.artwork != null) artworkImage.sprite = currentCard.artwork;
        ApplyFrameByRarity(currentCard);
        nameText.text = currentCard.cardName;

        string subCat = currentCard.subCategory != SubCategory.General ? $" / [{currentCard.subCategory}]" : "";
        typeText.text = $"{currentCard.type}{subCat}";

        string stats = $"Cost: {currentCard.cost}";
        if (currentCard.type == CardType.Monster) stats += $" | Atk: {currentCard.atk}";
        statsText.text = stats;

        if (LocalizationSettings.SelectedLocale.Identifier.Code == "en")
        {
            // English
            // ดึงคำแปลมาจากตาราง
            abilityText.text = LanguageBridge.Get(currentCard.abilityText);
            flavorText.text = $"<i>{LanguageBridge.Get(currentCard.flavorText)}</i>";
        }
        else
        {
            // Thai
            abilityText.text = currentCard.abilityText;
            flavorText.text = $"<i>{currentCard.flavorText}</i>";
        }

        // 2. ข้อมูลจำนวนที่มี
        int owned = GameManager.Instance.GetCardAmount(currentCard.card_id);
        amountOwnedText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
                                ? $"Owned : {owned}" // true = English
                                : $"คงเหลือ : {owned}";//false = thai

        // 3. ตั้งค่าปุ่ม Craft
        int craftCost = CraftingSystem.GetCraftCost(currentCard.rarity);
        int playerScrap = GameManager.Instance.CurrentGameData.profile.scrap;

        craftCostText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
                                ? $"Craft\n-{craftCost} Scrap"
                                : $"สร้างการ์ด\n-{craftCost} ชิ้นส่วน";
        craftButton.interactable = (playerScrap >= craftCost);

        // 🔥 ลบ listeners เก่า + เพิ่ม listeners ใหม่
        craftButton.onClick.RemoveAllListeners();
        if (onCraftAction != null)
        {
            craftButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySFX("ButtonClick");
                Debug.Log($"[Craft Button] Clicked - Card: {currentCard.cardName}");
                onCraftAction?.Invoke(currentCard);
            });
        }
        else
        {
            Debug.LogWarning($"[CollectionDetailView] onCraftAction is NULL! Cannot craft {currentCard.cardName}");
        }

        // 4. ตั้งค่าปุ่ม Dismantle
        int dismantleVal = CraftingSystem.GetDismantleValue(currentCard.rarity);

        dismantleValText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
                                ? $"Dismantle\n+{dismantleVal} Scrap"
                                : $"ย่อยการ์ด\n+{dismantleVal} ชิ้นส่วน";
        dismantleButton.interactable = (owned > 0); // ต้องมีของถึงจะย่อยได้

        // 🔥 ลบ listeners เก่า + เพิ่ม listeners ใหม่
        dismantleButton.onClick.RemoveAllListeners();
        if (onDismantleAction != null)
        {
            dismantleButton.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySFX("ButtonClick");
                Debug.Log($"[Dismantle Button] Clicked - Card: {currentCard.cardName}");
                onDismantleAction?.Invoke(currentCard);
            });
        }
        else
        {
            Debug.LogWarning($"[CollectionDetailView] onDismantleAction is NULL! Cannot dismantle {currentCard.cardName}");
        }

        // 🔥 Rebuild Layout เพื่อให้ UI settle อย่างถูกต้องตอนครั้งแรก
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void Close()
    {
        // 🔥 ลบ listener เมื่อปิด popup
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnInventoryChanged -= RefreshView;
        }

        gameObject.SetActive(false);
    }

    void EnsureFrameImage()
    {
        // ถ้ามี frameImage อยู่แล้ว ไม่ต้องสร้างใหม่
        if (frameImage != null) return;

        // ลบ CardFrame เก่าทั้งหมดก่อน (ป้องกันการสร้างซ้ำ)
        foreach (Transform child in transform)
        {
            if (child.name == "CardFrame")
            {
                Destroy(child.gameObject);
            }
        }

        // สร้าง CardFrame ใหม่
        GameObject frameObj = new GameObject("CardFrame");
        frameObj.transform.SetParent(transform, false);
        frameObj.transform.SetAsFirstSibling();

        frameImage = frameObj.AddComponent<Image>();
        frameImage.raycastTarget = false;
        frameImage.color = new Color(0f, 0f, 0f, 0f); // โปร่งใสสนิท
        frameImage.sprite = null; // ไม่มี sprite

        RectTransform frameRect = frameObj.GetComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;
    }

    void ApplyFrameByRarity(CardData data)
    {
        if (data == null) return;
        EnsureFrameImage();
        if (frameImage == null) return;

        Sprite rarityFrame = null;
        switch (data.rarity)
        {
            case Rarity.Common:
                rarityFrame = commonFrame;
                break;
            case Rarity.Rare:
                rarityFrame = rareFrame;
                break;
            case Rarity.Epic:
                rarityFrame = epicFrame;
                break;
            case Rarity.Legendary:
                rarityFrame = legendaryFrame;
                break;
        }

        if (rarityFrame != null)
        {
            frameImage.sprite = rarityFrame;
            frameImage.color = Color.white;
        }
        else
        {
            frameImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}