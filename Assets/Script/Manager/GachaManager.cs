using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.EventSystems;

public class GachaManager : MonoBehaviour
{
    private class PullResultItem
    {
        public CardData card;
        public GameObject root;
        public CardUISlot slot;
        public Image coverImage;
        public GameObject facedownAura;
        public Image facedownAuraPrimary;
        public Image facedownAuraSecondary;
        public Coroutine facedownAuraRoutine;
        public bool revealed;
    }

    [Header("Settings")]
    public int costPerPull = 100;

    [Header("Drop Rates")]
    public int commonRate = 60;
    public int rareRate = 30;
    public int epicRate = 7;
    public int legendaryRate = 3;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public GameObject resultPanel;
    public Transform resultGrid;
    public GameObject cardDisplayPrefab;
    public Button closeResultButton;
    public Button revealAllButton;

    [Header("Reveal Experience")]
    public Sprite cardBackSprite;
    [Range(0.1f, 0.6f)] public float revealFlipDuration = 0.4f;
    [Range(0.05f, 0.3f)] public float spawnInterval = 0.1f;
    [Range(0.2f, 1.2f)] public float revealPulseDuration = 0.45f;
    [Range(1.05f, 1.8f)] public float rareRevealScale = 1.12f;
    [Range(1.1f, 2.0f)] public float epicRevealScale = 1.24f;
    [Range(1.2f, 2.3f)] public float legendaryRevealScale = 1.4f;
    [Range(0f, 1f)] public float epicFlashAlpha = 0.45f;
    [Range(0f, 1f)] public float legendaryFlashAlpha = 0.75f;
    [Range(0.06f, 0.4f)] public float flashDuration = 0.2f;
    [Range(0.08f, 0.5f)] public float epicShakeDuration = 0.16f;
    [Range(0.1f, 0.6f)] public float legendaryShakeDuration = 0.28f;
    [Range(4f, 24f)] public float epicShakeDistance = 7f;
    [Range(8f, 38f)] public float legendaryShakeDistance = 14f;

    [Header("Reveal All Speed")]
    [Range(0.2f, 1.4f)] public float revealAllFlipDurationMultiplier = 0.4f;
    [Range(0f, 0.35f)] public float revealAllCardInterval = 0.1f;
    public bool revealAllSkipRarityCinematics = false;

    [Header("Facedown Aura")]
    [Range(0.1f, 1f)] public float epicFacedownAuraAlpha = 0.34f;
    [Range(0.1f, 1f)] public float legendaryFacedownAuraAlpha = 0.46f;
    [Range(1.02f, 1.8f)] public float epicFacedownAuraScale = 1.14f;
    [Range(1.05f, 2.0f)] public float legendaryFacedownAuraScale = 1.24f;
    [Range(0.8f, 6f)] public float facedownAuraPulseSpeed = 1.9f;
    [Range(8f, 220f)] public float facedownAuraRotateSpeed = 34f;
    public Color cyberEpicPrimary = new Color(0.30f, 0.82f, 1f, 1f);
    public Color cyberEpicSecondary = new Color(0.62f, 0.90f, 1f, 1f);
    public Color cyberLegendaryPrimary = new Color(0.18f, 0.92f, 1f, 1f);
    public Color cyberLegendarySecondary = new Color(0.75f, 0.96f, 1f, 1f);

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
    private readonly List<PullResultItem> currentPullItems = new List<PullResultItem>();
    private Image revealFlashOverlay;
    private bool isRevealAllInProgress;

    void Start()
    {
        allCards = Resources.LoadAll<CardData>("GameContent/Cards").ToList();

        if (closeResultButton != null) closeResultButton.onClick.AddListener(CloseResult);
        if (revealAllButton != null) revealAllButton.onClick.AddListener(OnClickRevealAll);

        UpdateBannerButtonsVisual();
        SelectBanner(1);
        UpdateUI();
        RefreshRevealAllButton();
        RefreshPullButtonsState();

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void Update() { UpdateUI(); }

    void UpdateUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameData != null && goldText != null)
        {
            goldText.text = $"{GameManager.Instance.CurrentGameData.profile.gold}";
        }

        RefreshPullButtonsState();
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

            currentBannerNameText.text = LocalizationSettings.SelectedLocale.Identifier.Code == "en"
                                        ? $"Banner: {currentTargetCategory}"
                                        : $"ตู้: {currentTargetCategory}";

            if (!isUnlocked)
            {
                currentBannerNameText.text += " <color=red> ( LOCKED )</color>";
                if (bannerImageDisplay != null) bannerImageDisplay.color = Color.gray;
            }
            else
            {
                if (bannerImageDisplay != null) bannerImageDisplay.color = Color.white;
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
            if (btn.image != null) btn.image.color = colors.normalColor;
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

        if (HasUnrevealedCards())
        {
            Debug.LogWarning("❌ ต้องเปิดการ์ดที่สุ่มมาก่อนหน้าให้ครบก่อนสุ่มใหม่");
            return;
        }

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
            // เช็คQuest ที่เกี่ยวข้อง (ถ้ามี)
            UpdateDailyQuest(catIndex, 1);
            Debug.Log($"✅ Pulled: {pulledCard.cardName}");
            ShowResult(new List<CardData> { pulledCard });
        }
        else Debug.LogWarning($"❌ เงินไม่พอ! มี {currentGold} ต้องการ {costPerPull}");
    }

    public void PullTen()
    {
        Debug.Log("🔵 PullTen() called");

        if (HasUnrevealedCards())
        {
            Debug.LogWarning("❌ ต้องเปิดการ์ดที่สุ่มมาก่อนหน้าให้ครบก่อนสุ่มใหม่");
            return;
        }

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
            // เช็คQuest ที่เกี่ยวข้อง (ถ้ามี)
            UpdateDailyQuest(catIndex, 10);
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
        isRevealAllInProgress = false;
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultGrid != null)
        {
            foreach (Transform child in resultGrid) Destroy(child.gameObject);
            StartCoroutine(SpawnCardsRoutine(cards));
        }
        RefreshRevealAllButton();
        RefreshPullButtonsState();
        RefreshCloseButtonState();
    }

    IEnumerator SpawnCardsRoutine(List<CardData> cards)
    {
        currentPullItems.Clear();
        EnsureRevealFlashOverlay();
        yield return null;

        int hiddenCount = 0;
        foreach (var card in cards)
        {
            GameObject obj = Instantiate(cardDisplayPrefab, resultGrid);
            obj.SetActive(false);
            DisableDragBehaviours(obj);
            var slot = obj.GetComponent<CardUISlot>();
            if (slot != null) slot.Setup(card, -1, null, null);
            HideStatTextsForShop(obj);

            PullResultItem item = new PullResultItem
            {
                card = card,
                root = obj,
                slot = slot,
                revealed = false
            };

            SetCardFaceVisible(item, false);
            SetupHiddenCard(item);

            // Animation
            obj.SetActive(true);
            obj.transform.localScale = Vector3.zero;
            float timer = 0;
            while (timer < 0.3f)
            {
                timer += Time.deltaTime;
                float t = timer / 0.3f;
                float ease = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
                obj.transform.localScale = Vector3.one * ease;
                yield return null;
            }
            obj.transform.localScale = Vector3.one;

            currentPullItems.Add(item);
            hiddenCount++;

            RefreshRevealAllButton();
            RefreshPullButtonsState();
            RefreshCloseButtonState();

            yield return new WaitForSeconds(spawnInterval);
        }

        RefreshRevealAllButton();
        RefreshPullButtonsState();
        RefreshCloseButtonState();
        Debug.Log($"🃏 Spawned {hiddenCount} hidden gacha cards - click to reveal");
    }

    void DisableDragBehaviours(GameObject root)
    {
        if (root == null) return;

        var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var behaviour in behaviours)
        {
            if (behaviour == null || !behaviour.enabled) continue;

            if (behaviour is IBeginDragHandler || behaviour is IDragHandler || behaviour is IEndDragHandler)
            {
                behaviour.enabled = false;
            }
        }
    }

    void HideStatTextsForShop(GameObject root)
    {
        if (root == null) return;

        bool IsStatName(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string key = value.ToLowerInvariant();
            return key.Contains("cost")
                   || key.Contains("atk")
                   || key.Contains("attack")
                   || key.Contains("power")
                   || key.Contains("hp")
                   || key.Contains("def")
                   || key.Contains("stat");
        }

        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
        {
            if (tr == null) continue;
            if (IsStatName(tr.name))
                tr.gameObject.SetActive(false);
        }

        foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp == null || tmp.gameObject == null) continue;
            string text = (tmp.text ?? string.Empty).Trim().ToLowerInvariant();

            bool isStatText = text == "atk" || text == "hp" || text == "def" || text == "cost" || text == "power"
                || text.StartsWith("atk:") || text.StartsWith("hp:") || text.StartsWith("def:")
                || text.StartsWith("cost:") || text.StartsWith("power:");

            if (isStatText)
                tmp.gameObject.SetActive(false);
        }
    }

    void SetupHiddenCard(PullResultItem item)
    {
        if (item == null || item.root == null) return;

        SetupFacedownAura(item);

        GameObject coverObj = new GameObject("RevealCover", typeof(RectTransform), typeof(Image));
        coverObj.transform.SetParent(item.root.transform, false);
        coverObj.transform.SetAsLastSibling();

        RectTransform rt = coverObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image coverImage = coverObj.GetComponent<Image>();
        coverImage.raycastTarget = true;
        coverImage.color = Color.white;

        if (cardBackSprite != null)
        {
            coverImage.sprite = cardBackSprite;
            coverImage.type = Image.Type.Sliced;
            coverImage.preserveAspect = true;
        }
        else
        {
            coverImage.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        }

        item.coverImage = coverImage;

        Button revealButton = coverObj.AddComponent<Button>();
        ColorBlock colors = revealButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        revealButton.colors = colors;
        revealButton.transition = Selectable.Transition.ColorTint;
        revealButton.onClick.AddListener(() =>
        {
            if (!item.revealed) StartCoroutine(RevealCardRoutine(item, false));
        });

        RefreshRevealAllButton();
    }

    IEnumerator RevealCardRoutine(PullResultItem item, bool fastMode = false)
    {
        if (item == null || item.revealed || item.root == null)
        {
            RefreshRevealAllButton();
            RefreshPullButtonsState();
            RefreshCloseButtonState();
            yield break;
        }
        item.revealed = true;
        ClearFacedownAura(item);

        Transform cardTransform = item.root.transform;
        Vector3 startScale = cardTransform.localScale;
        float currentFlipDuration = fastMode
            ? Mathf.Max(0.05f, revealFlipDuration * revealAllFlipDurationMultiplier)
            : revealFlipDuration;

        float t = 0f;
        while (t < currentFlipDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / currentFlipDuration);
            float sx = Mathf.Lerp(startScale.x, 0.01f, p);
            cardTransform.localScale = new Vector3(sx, startScale.y, startScale.z);
            yield return null;
        }

        if (item.coverImage != null)
        {
            Destroy(item.coverImage.gameObject);
            item.coverImage = null;
        }

        SetCardFaceVisible(item, true);

        t = 0f;
        while (t < currentFlipDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / currentFlipDuration);
            float sx = Mathf.Lerp(0.01f, startScale.x, p);
            cardTransform.localScale = new Vector3(sx, startScale.y, startScale.z);
            yield return null;
        }
        cardTransform.localScale = startScale;

        if (!fastMode)
        {
            yield return StartCoroutine(PlayRarityRevealEffect(item));
        }
        else if (!revealAllSkipRarityCinematics)
        {
            yield return StartCoroutine(PlayRarityRevealEffect(item));
        }

        RefreshRevealAllButton();
        RefreshPullButtonsState();
        RefreshCloseButtonState();
    }

    void OnClickRevealAll()
    {
        if (isRevealAllInProgress) return;
        StartCoroutine(RevealAllRoutine());
    }

    IEnumerator RevealAllRoutine()
    {
        if (!HasUnrevealedCards())
        {
            RefreshRevealAllButton();
            RefreshPullButtonsState();
            RefreshCloseButtonState();
            yield break;
        }

        isRevealAllInProgress = true;
        RefreshRevealAllButton();

        var pendingItems = currentPullItems.Where(x => x != null && !x.revealed).ToList();
        foreach (var item in pendingItems)
        {
            if (item != null && !item.revealed)
                yield return StartCoroutine(RevealCardRoutine(item, true));

            if (revealAllCardInterval > 0f)
                yield return new WaitForSeconds(revealAllCardInterval);
        }

        isRevealAllInProgress = false;
        RefreshRevealAllButton();
        RefreshPullButtonsState();
        RefreshCloseButtonState();
    }

    IEnumerator PlayRarityRevealEffect(PullResultItem item)
    {
        if (item == null || item.root == null || item.card == null) yield break;

        float targetScale = 1.05f;
        Color glowColor = new Color(1f, 1f, 1f, 0.18f);

        switch (item.card.rarity)
        {
            case Rarity.Rare:
                targetScale = rareRevealScale;
                glowColor = new Color(0.45f, 0.7f, 1f, 0.25f);
                break;
            case Rarity.Epic:
                targetScale = epicRevealScale;
                glowColor = new Color(0.9f, 0.45f, 1f, 0.32f);
                break;
            case Rarity.Legendary:
                targetScale = legendaryRevealScale;
                glowColor = new Color(1f, 0.85f, 0.35f, 0.42f);
                break;
        }

        if (item.card.rarity == Rarity.Epic)
        {
            yield return StartCoroutine(PlayScreenFlash(new Color(0.78f, 0.35f, 1f, 1f), epicFlashAlpha, flashDuration));
            yield return StartCoroutine(ShakeTransform(item.root.transform, epicShakeDuration, epicShakeDistance));
        }
        else if (item.card.rarity == Rarity.Legendary)
        {
            yield return StartCoroutine(PlayScreenFlash(new Color(1f, 0.84f, 0.2f, 1f), legendaryFlashAlpha, flashDuration * 1.2f));
            yield return StartCoroutine(ShakeTransform(item.root.transform, legendaryShakeDuration, legendaryShakeDistance));
            if (resultGrid != null)
            {
                yield return StartCoroutine(ShakeTransform(resultGrid, legendaryShakeDuration * 0.9f, legendaryShakeDistance * 0.7f));
            }
        }

        GameObject glowObj = new GameObject("RarityGlow", typeof(RectTransform), typeof(Image));
        glowObj.transform.SetParent(item.root.transform, false);
        glowObj.transform.SetAsFirstSibling();

        RectTransform glowRt = glowObj.GetComponent<RectTransform>();
        glowRt.anchorMin = Vector2.zero;
        glowRt.anchorMax = Vector2.one;
        glowRt.offsetMin = Vector2.zero;
        glowRt.offsetMax = Vector2.zero;

        Image glow = glowObj.GetComponent<Image>();
        glow.raycastTarget = false;
        glow.color = glowColor;

        Transform cardTransform = item.root.transform;
        Vector3 baseScale = Vector3.one;

        float elapsed = 0f;
        while (elapsed < revealPulseDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / revealPulseDuration);
            float scaleP = Mathf.Sin(p * Mathf.PI);
            float scale = Mathf.Lerp(1f, targetScale, scaleP);
            cardTransform.localScale = baseScale * scale;

            float alpha = Mathf.Lerp(glowColor.a, 0f, p);
            glow.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
            yield return null;
        }

        cardTransform.localScale = Vector3.one;
        Destroy(glowObj);
    }

    void SetupFacedownAura(PullResultItem item)
    {
        if (item == null || item.root == null || item.card == null) return;

        if (item.card.rarity != Rarity.Epic && item.card.rarity != Rarity.Legendary)
            return;

        ClearFacedownAura(item);

        Color primaryColor;
        Color secondaryColor;
        float maxScale;
        float baseAlpha;
        Vector2 outerPadding;

        if (item.card.rarity == Rarity.Legendary)
        {
            primaryColor = cyberLegendaryPrimary;
            secondaryColor = cyberLegendarySecondary;
            maxScale = legendaryFacedownAuraScale;
            baseAlpha = legendaryFacedownAuraAlpha;
            outerPadding = new Vector2(24f, 24f);
        }
        else
        {
            primaryColor = cyberEpicPrimary;
            secondaryColor = cyberEpicSecondary;
            maxScale = epicFacedownAuraScale;
            baseAlpha = epicFacedownAuraAlpha;
            outerPadding = new Vector2(18f, 18f);
        }

        GameObject auraObj = new GameObject("FacedownAura", typeof(RectTransform));
        auraObj.transform.SetParent(item.root.transform, false);
        auraObj.transform.SetAsFirstSibling();

        RectTransform auraRt = auraObj.GetComponent<RectTransform>();
        auraRt.anchorMin = Vector2.zero;
        auraRt.anchorMax = Vector2.one;
        auraRt.offsetMin = -outerPadding;
        auraRt.offsetMax = outerPadding;

        GameObject primaryObj = new GameObject("AuraPrimary", typeof(RectTransform), typeof(Image));
        primaryObj.transform.SetParent(auraObj.transform, false);
        RectTransform primaryRt = primaryObj.GetComponent<RectTransform>();
        primaryRt.anchorMin = Vector2.zero;
        primaryRt.anchorMax = Vector2.one;
        primaryRt.offsetMin = Vector2.zero;
        primaryRt.offsetMax = Vector2.zero;
        Image primaryImage = primaryObj.GetComponent<Image>();
        primaryImage.raycastTarget = false;
        primaryImage.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, baseAlpha);

        GameObject secondaryObj = new GameObject("AuraSecondary", typeof(RectTransform), typeof(Image));
        secondaryObj.transform.SetParent(auraObj.transform, false);
        RectTransform secondaryRt = secondaryObj.GetComponent<RectTransform>();
        secondaryRt.anchorMin = Vector2.zero;
        secondaryRt.anchorMax = Vector2.one;
        secondaryRt.offsetMin = new Vector2(8f, 8f);
        secondaryRt.offsetMax = new Vector2(-8f, -8f);
        Image secondaryImage = secondaryObj.GetComponent<Image>();
        secondaryImage.raycastTarget = false;
        secondaryImage.color = new Color(secondaryColor.r, secondaryColor.g, secondaryColor.b, baseAlpha * 0.75f);

        item.facedownAura = auraObj;
        item.facedownAuraPrimary = primaryImage;
        item.facedownAuraSecondary = secondaryImage;
        item.facedownAuraRoutine = StartCoroutine(PulseFacedownAura(item, baseAlpha, maxScale));
    }

    void ClearFacedownAura(PullResultItem item)
    {
        if (item == null) return;

        if (item.facedownAuraRoutine != null)
        {
            StopCoroutine(item.facedownAuraRoutine);
            item.facedownAuraRoutine = null;
        }

        if (item.facedownAura != null)
        {
            Destroy(item.facedownAura);
            item.facedownAura = null;
        }

        item.facedownAuraPrimary = null;
        item.facedownAuraSecondary = null;
    }

    IEnumerator PulseFacedownAura(PullResultItem item, float baseAlpha, float maxScale)
    {
        if (item == null || item.facedownAuraPrimary == null || item.facedownAuraSecondary == null) yield break;

        RectTransform primaryRt = item.facedownAuraPrimary.rectTransform;
        RectTransform secondaryRt = item.facedownAuraSecondary.rectTransform;
        float timer = 0f;

        while (item != null && !item.revealed && item.facedownAuraPrimary != null && item.facedownAuraSecondary != null)
        {
            timer += Time.deltaTime;
            float waveA = (Mathf.Sin(timer * facedownAuraPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            float waveB = (Mathf.Sin((timer * facedownAuraPulseSpeed * 1.22f + 0.17f) * Mathf.PI * 2f) + 1f) * 0.5f;

            float primaryScale = Mathf.Lerp(1f, maxScale, waveA);
            float secondaryScale = Mathf.Lerp(0.96f, maxScale * 0.92f, waveB);
            primaryRt.localScale = new Vector3(primaryScale, primaryScale, 1f);
            secondaryRt.localScale = new Vector3(secondaryScale, secondaryScale, 1f);

            float primaryRot = Mathf.Sin(timer * 2.1f) * 8f;
            float secondaryRot = -timer * facedownAuraRotateSpeed;
            primaryRt.localRotation = Quaternion.Euler(0f, 0f, primaryRot);
            secondaryRt.localRotation = Quaternion.Euler(0f, 0f, secondaryRot);

            Color p = item.facedownAuraPrimary.color;
            Color s = item.facedownAuraSecondary.color;
            p.a = Mathf.Lerp(baseAlpha * 0.35f, baseAlpha, waveA);
            s.a = Mathf.Lerp(baseAlpha * 0.2f, baseAlpha * 0.9f, waveB);
            item.facedownAuraPrimary.color = p;
            item.facedownAuraSecondary.color = s;
            yield return null;
        }
    }

    void SetCardFaceVisible(PullResultItem item, bool visible)
    {
        if (item == null || item.slot == null) return;

        if (item.slot.cardImage != null)
            item.slot.cardImage.enabled = visible;

        if (item.slot.frameImage != null)
            item.slot.frameImage.enabled = visible;

        if (item.slot.amountText != null)
            item.slot.amountText.enabled = visible;
    }

    void EnsureRevealFlashOverlay()
    {
        if (revealFlashOverlay != null) return;
        if (resultPanel == null) return;

        Canvas canvas = resultPanel.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        GameObject flashObj = new GameObject("GachaRevealFlash", typeof(RectTransform), typeof(Image));
        flashObj.transform.SetParent(canvas.transform, false);
        flashObj.transform.SetAsLastSibling();

        RectTransform rt = flashObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        revealFlashOverlay = flashObj.GetComponent<Image>();
        revealFlashOverlay.raycastTarget = false;
        revealFlashOverlay.color = new Color(1f, 1f, 1f, 0f);
        flashObj.SetActive(false);
    }

    IEnumerator PlayScreenFlash(Color color, float peakAlpha, float duration)
    {
        EnsureRevealFlashOverlay();
        if (revealFlashOverlay == null) yield break;

        revealFlashOverlay.gameObject.SetActive(true);
        revealFlashOverlay.transform.SetAsLastSibling();

        float half = Mathf.Max(0.01f, duration * 0.5f);
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            revealFlashOverlay.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0f, peakAlpha, p));
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            revealFlashOverlay.color = new Color(color.r, color.g, color.b, Mathf.Lerp(peakAlpha, 0f, p));
            yield return null;
        }

        revealFlashOverlay.color = new Color(color.r, color.g, color.b, 0f);
        revealFlashOverlay.gameObject.SetActive(false);
    }

    IEnumerator ShakeTransform(Transform target, float duration, float distance)
    {
        if (target == null) yield break;

        Vector3 original = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = 1f - Mathf.Clamp01(elapsed / duration);
            float shakeX = Random.Range(-1f, 1f) * distance * strength;
            float shakeY = Random.Range(-1f, 1f) * distance * strength;
            target.localPosition = original + new Vector3(shakeX, shakeY, 0f);
            yield return null;
        }

        target.localPosition = original;
    }

    public void CloseResult()
    {
        if (HasUnrevealedCards() || isRevealAllInProgress)
        {
            Debug.LogWarning("❌ ต้องเปิดการ์ดให้ครบก่อนถึงจะปิดหน้าผลสุ่มได้");
            RefreshCloseButtonState();
            return;
        }

        foreach (var item in currentPullItems)
            ClearFacedownAura(item);

        if (resultPanel != null) resultPanel.SetActive(false);
        RefreshRevealAllButton();
        RefreshPullButtonsState();
        RefreshCloseButtonState();
    }

    bool HasUnrevealedCards()
    {
        return currentPullItems.Any(x => x != null && !x.revealed);
    }

    void RefreshRevealAllButton()
    {
        if (revealAllButton == null) return;

        bool hasHidden = HasUnrevealedCards();
        revealAllButton.gameObject.SetActive(hasHidden || isRevealAllInProgress);
        revealAllButton.interactable = hasHidden && !isRevealAllInProgress;
    }

    void RefreshPullButtonsState()
    {
        bool canPullNow = !HasUnrevealedCards() && !isRevealAllInProgress;

        if (pullOneButton != null && pullOneButton.gameObject.activeInHierarchy)
            pullOneButton.interactable = canPullNow;

        if (pullTenButton != null && pullTenButton.gameObject.activeInHierarchy)
            pullTenButton.interactable = canPullNow;
    }

    void RefreshCloseButtonState()
    {
        if (closeResultButton == null) return;

        bool canClose = !HasUnrevealedCards() && !isRevealAllInProgress;
        closeResultButton.interactable = canClose;
    }

    void UpdateDailyQuest(int categoryIndex, int amount)
    {
        // Debug.Log("cat index", categoryIndex);
        string questKey = "";

        // ตรงนี้อาจจะยังต้อง if/switch บ้าง แต่สั้นกว่าเดิมเยอะ
        // หรือถ้าตั้งชื่อ Scene ให้ตรงกับ Key ก็แทบไม่ต้อง if เลย
        if (categoryIndex == 1) questKey = "A01";
        else if (categoryIndex == 2) questKey = "A02";
        else if (categoryIndex == 3) questKey = "A03";
        // Debug.Log("quest key", questKey);
        // 2. ตะโกนบอก Manager ทีเดียวจบ!
        // "เฮ้! มีคนเล่น Story จบ 1 ครั้งนะ รหัสคือ A01"
        DailyQuestManager.Instance.UpdateProgress(QuestType.Gacha, amount, questKey);

    }
}