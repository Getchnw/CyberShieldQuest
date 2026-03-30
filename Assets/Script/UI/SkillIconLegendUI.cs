using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIconLegendUI : MonoBehaviour
{
    [Header("Behavior")]
    public bool createButtonAtRuntime = true;
    public string buttonLabel = "i";
    public string panelTitle = "Skill Icon Guide";
    public bool startHidden = true;

    [Header("Optional Manual References")]
    public Canvas targetCanvas;
    public Button helpButton;
    public GameObject panelRoot;

    [Header("Button Style")]
    public Vector2 buttonSize = new Vector2(56f, 56f);
    public Vector2 buttonAnchoredPosition = new Vector2(-22f, -20f);
    public Color buttonBackgroundColor = new Color(0.08f, 0.1f, 0.14f, 0.92f);
    public Color buttonOutlineColor = new Color(0.45f, 0.78f, 1f, 0.65f);
    public Color buttonLabelColor = Color.white;
    [Range(16f, 64f)] public float buttonLabelFontSize = 34f;

    [Header("Panel Style")]
    public Vector2 panelSize = new Vector2(900f, 620f);
    public Vector2 panelAnchoredPosition = Vector2.zero;
    public Color panelBackgroundColor = new Color(0.01f, 0.015f, 0.025f, 0.98f);
    public Color headerBackgroundColor = new Color(0.15f, 0.18f, 0.24f, 0.95f);
    public Color rowBackgroundColor = new Color(0.11f, 0.12f, 0.16f, 0.9f);
    public Color scrollBackgroundColor = new Color(0.09f, 0.1f, 0.13f, 0.9f);
    public Color titleColor = Color.white;
    public Color bodyTextColor = Color.white;
    [Range(16f, 48f)] public float titleFontSize = 34f;
    [Range(14f, 36f)] public float bodyFontSize = 21f;
    [Range(24f, 72f)] public float iconSize = 40f;

    private const float RowHeight = 58f;
    private const float HeaderHeight = 42f;
    private IconProvider iconProvider;
    private bool warnedIconSourceMissing;

    public static SkillIconLegendUI EnsureInScene(string hostName = "SkillIconLegendUI")
    {
        SkillIconLegendUI existing = FindObjectOfType<SkillIconLegendUI>(true);
        if (existing != null) return existing;

        Canvas canvas = FindBestCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("SkillIconLegendUI: no Canvas found in scene.");
            return null;
        }

        GameObject host = new GameObject(hostName, typeof(RectTransform));
        host.transform.SetParent(canvas.transform, false);

        SkillIconLegendUI legend = host.AddComponent<SkillIconLegendUI>();
        legend.targetCanvas = canvas;
        return legend;
    }

    private static Canvas FindBestCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || !canvas.isRootCanvas) continue;
            return canvas;
        }

        return canvases.FirstOrDefault();
    }

    void Start()
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindBestCanvas();
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("SkillIconLegendUI: targetCanvas is missing.");
            return;
        }

        if (createButtonAtRuntime)
        {
            EnsureRuntimeButton();
            EnsureRuntimePanel();
        }

        ApplyRuntimeStyles();
        iconProvider = ResolveIconProvider();

        if (helpButton != null)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(TogglePanel);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(!startHidden);
            BuildLegendContent();
        }
    }

    public void TogglePanel()
    {
        if (panelRoot == null) return;
        bool nextState = !panelRoot.activeSelf;
        panelRoot.SetActive(nextState);
        AudioManager.Instance.PlaySFX("ButtonClick");
        if (nextState)
        {
            iconProvider = ResolveIconProvider();
            BuildLegendContent();
        }
    }

    public void RefreshLayoutAndStyle()
    {
        ApplyRuntimeStyles();
        if (panelRoot != null && panelRoot.activeSelf)
        {
            iconProvider = ResolveIconProvider();
            BuildLegendContent();
        }
    }

    private void EnsureRuntimeButton()
    {
        if (helpButton != null) return;

        GameObject buttonObj = new GameObject("SkillLegendButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(targetCanvas.transform, false);
        buttonObj.transform.SetAsLastSibling();

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = buttonAnchoredPosition;
        rect.sizeDelta = buttonSize;

        Image bg = buttonObj.GetComponent<Image>();
        bg.color = buttonBackgroundColor;

        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = buttonOutlineColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        helpButton = buttonObj.GetComponent<Button>();
        ColorBlock colors = helpButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.9f, 1f, 1f);
        helpButton.colors = colors;

        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(buttonObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = txtObj.GetComponent<TextMeshProUGUI>();
        label.text = buttonLabel;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = buttonLabelFontSize;
        label.color = buttonLabelColor;
        label.raycastTarget = false;
    }

    private void EnsureRuntimePanel()
    {
        if (panelRoot != null) return;

        GameObject panelObj = new GameObject("SkillLegendPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(targetCanvas.transform, false);
        panelObj.transform.SetAsLastSibling();

        panelRoot = panelObj;

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = panelAnchoredPosition;
        panelRect.sizeDelta = panelSize;

        Image panelBg = panelObj.GetComponent<Image>();
        panelBg.color = panelBackgroundColor;

        CreateTitle(panelObj.transform);
        CreateCloseButton(panelObj.transform);
        CreateScrollArea(panelObj.transform);
    }

    private void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(parent, false);

        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        rect.sizeDelta = new Vector2(-120f, 46f);

        TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
        title.text = panelTitle;
        title.fontSize = titleFontSize;
        title.alignment = TextAlignmentOptions.Center;
        title.color = titleColor;
    }

    private void CreateCloseButton(Transform parent)
    {
        GameObject closeObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(parent, false);

        RectTransform rect = closeObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-14f, -12f);
        rect.sizeDelta = new Vector2(44f, 44f);

        Image bg = closeObj.GetComponent<Image>();
        bg.color = new Color(0.22f, 0.08f, 0.08f, 1f);

        Button closeButton = closeObj.GetComponent<Button>();
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(TogglePanel);

        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(closeObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = txtObj.GetComponent<TextMeshProUGUI>();
        label.text = "X";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24f;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private void CreateScrollArea(Transform parent)
    {
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);

        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(24f, 20f);
        scrollRect.offsetMax = new Vector2(-24f, -76f);

        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = scrollBackgroundColor;

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(8f, 8f);
        viewportRect.offsetMax = new Vector2(-8f, -8f);

        Image viewportImage = viewportObj.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        viewportImage.raycastTarget = true;

        Mask viewportMask = viewportObj.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = contentObj.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(6, 6, 8, 8);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = contentObj.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
    }

    private void BuildLegendContent()
    {
        if (panelRoot == null) return;

        Transform content = panelRoot.transform.Find("ScrollView/Viewport/Content");
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        AddSection(content, "Trigger Icons", BuildTriggerEntries());
        AddSection(content, "Action Icons", BuildActionEntries());
    }

    private void AddSection(Transform content, string title, List<LegendEntry> entries)
    {
        GameObject headerObj = new GameObject($"Header_{title}", typeof(RectTransform), typeof(Image));
        headerObj.transform.SetParent(content, false);

        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0f, HeaderHeight);

        Image headerBg = headerObj.GetComponent<Image>();
        headerBg.color = headerBackgroundColor;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = new Vector2(14f, 0f);
        titleRect.offsetMax = new Vector2(-14f, 0f);

        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = title;
        titleText.fontSize = 26f;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        titleText.color = titleColor;
        titleText.raycastTarget = false;

        foreach (LegendEntry entry in entries)
        {
            AddRow(content, entry);
        }
    }

    private void AddRow(Transform parent, LegendEntry entry)
    {
        GameObject rowObj = new GameObject($"Row_{entry.Key}", typeof(RectTransform), typeof(Image));
        rowObj.transform.SetParent(parent, false);

        RectTransform rowRect = rowObj.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, RowHeight);

        Image rowBg = rowObj.GetComponent<Image>();
        rowBg.color = rowBackgroundColor;

        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(rowObj.transform, false);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(10f, 0f);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        Image icon = iconObj.GetComponent<Image>();
        icon.sprite = entry.Icon;
        icon.preserveAspect = true;
        icon.color = entry.Icon != null ? Color.white : new Color(1f, 1f, 1f, 0.25f);

        GameObject textObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(rowObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(62f, 2f);
        textRect.offsetMax = new Vector2(-8f, -2f);

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = entry.Description;
        text.fontSize = bodyFontSize;
        text.enableWordWrapping = true;
        text.color = bodyTextColor;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
    }

    private List<LegendEntry> BuildTriggerEntries()
    {
        var entries = new List<LegendEntry>();

        foreach (EffectTrigger trigger in Enum.GetValues(typeof(EffectTrigger)))
        {
            if (trigger == EffectTrigger.None) continue;

            Sprite icon = GetTriggerSpriteWithFallback(trigger);
            entries.Add(new LegendEntry
            {
                Key = trigger.ToString(),
                Icon = icon,
                Description = $"{GetTriggerThaiLabel(trigger)} ({trigger})"
            });
        }

        return entries;
    }

    private List<LegendEntry> BuildActionEntries()
    {
        var groupedByIcon = new Dictionary<Sprite, List<string>>();
        var entriesNoIcon = new List<string>();

        foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
        {
            if (action == ActionType.None) continue;

            string label = $"{GetActionThaiLabel(action)} ({action})";
            Sprite icon = GetActionSpriteWithFallback(action);

            if (icon == null)
            {
                entriesNoIcon.Add(label);
                continue;
            }

            if (!groupedByIcon.TryGetValue(icon, out List<string> list))
            {
                list = new List<string>();
                groupedByIcon.Add(icon, list);
            }

            list.Add(label);
        }

        var result = new List<LegendEntry>();

        foreach (var kv in groupedByIcon)
        {
            result.Add(new LegendEntry
            {
                Key = kv.Key != null ? kv.Key.name : Guid.NewGuid().ToString(),
                Icon = kv.Key,
                Description = string.Join(", ", kv.Value)
            });
        }

        foreach (string label in entriesNoIcon)
        {
            result.Add(new LegendEntry
            {
                Key = $"NoIcon_{label}",
                Icon = null,
                Description = label
            });
        }

        return result.OrderBy(x => x.Description).ToList();
    }

    private void ApplyRuntimeStyles()
    {
        if (helpButton != null)
        {
            RectTransform rect = helpButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = buttonAnchoredPosition;
                rect.sizeDelta = buttonSize;
            }

            Image buttonImage = helpButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = buttonBackgroundColor;

            TextMeshProUGUI label = helpButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = buttonLabel;
                label.fontSize = buttonLabelFontSize;
                label.color = buttonLabelColor;
            }
        }

        if (panelRoot != null)
        {
            RectTransform rect = panelRoot.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = panelAnchoredPosition;
                rect.sizeDelta = panelSize;
            }

            Image panelImage = panelRoot.GetComponent<Image>();
            if (panelImage != null) panelImage.color = panelBackgroundColor;

            Transform titleTransform = panelRoot.transform.Find("Title");
            if (titleTransform != null)
            {
                TextMeshProUGUI titleText = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = panelTitle;
                    titleText.fontSize = titleFontSize;
                    titleText.color = titleColor;
                }
            }
        }
    }

    private Sprite GetTriggerSpriteWithFallback(EffectTrigger trigger)
    {
        if (iconProvider == null) iconProvider = ResolveIconProvider();
        if (iconProvider == null) return null;

        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return iconProvider.iconOnDeploy;
            case EffectTrigger.OnStrike: return iconProvider.iconOnStrike;
            case EffectTrigger.OnStrikeHit: return iconProvider.iconOnStrikeHit;
            case EffectTrigger.OnIntercept: return iconProvider.iconOnIntercept;
            case EffectTrigger.OnDestroyed: return iconProvider.iconOnDestroyed;
            case EffectTrigger.OnTurnEnd: return iconProvider.iconOnTurnEnd;
            case EffectTrigger.Continuous: return iconProvider.iconContinuous;
            default: return null;
        }
    }

    private Sprite GetActionSpriteWithFallback(ActionType action)
    {
        if (iconProvider == null) iconProvider = ResolveIconProvider();
        if (iconProvider == null) return null;

        switch (action)
        {
            case ActionType.Destroy: return iconProvider.iconDestroy;
            case ActionType.DisableAttack: return iconProvider.iconDisableAttack;
            case ActionType.DisableAbility: return iconProvider.iconDisableAbility;
            case ActionType.RevealHand:
            case ActionType.RevealHandMultiple: return iconProvider.iconRevealHand;
            case ActionType.DiscardDeck: return iconProvider.iconDiscardDeck;
            case ActionType.SummonToken: return iconProvider.iconSummonToken;
            case ActionType.ModifyStat: return iconProvider.iconModifyStat;
            case ActionType.ControlEquip: return iconProvider.iconControlEquip;
            case ActionType.HealHP:
            case ActionType.HealOnMonsterSummoned: return iconProvider.iconHealHP;
            case ActionType.ForceIntercept: return iconProvider.iconForceIntercept;
            case ActionType.BypassIntercept: return iconProvider.iconBypassIntercept;
            case ActionType.DisableIntercept: return iconProvider.iconDisableIntercept;
            case ActionType.DrawCard: return iconProvider.iconDrawCard;
            case ActionType.Rush: return iconProvider.iconRush;
            case ActionType.DoubleStrike: return iconProvider.iconDoubleStrike;
            case ActionType.GraveyardATK: return iconProvider.iconGraveyardATK;
            case ActionType.ZeroStats: return iconProvider.iconZeroStats;
            case ActionType.RemoveCategory: return iconProvider.iconRemoveCategory;
            case ActionType.ForceChooseDiscard: return iconProvider.iconForceChooseDiscard;
            case ActionType.ReturnEquipFromGraveyard: return iconProvider.iconReturnEquipFromGraveyard;
            case ActionType.PeekDiscardTopDeck: return iconProvider.iconPeekDiscardTopDeck;
            case ActionType.MarkInterceptMillDeck: return iconProvider.iconMarkInterceptMillDeck;
            case ActionType.InterceptAlwaysTypeMatch: return iconProvider.iconForceIntercept;
            case ActionType.ProtectDrawnCards:
            case ActionType.ProtectRevealHandMultiple:
            case ActionType.ProtectForceInterceptEquip:
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy: return iconProvider.iconProtection;
            default: return null;
        }
    }

    private IconProvider ResolveIconProvider()
    {
        SkillIconAssets assets = SkillIconAssets.Instance;
        if (assets == null)
        {
            assets = Resources.FindObjectsOfTypeAll<SkillIconAssets>().FirstOrDefault();
        }

        if (assets != null)
        {
            return IconProvider.FromAssets(assets);
        }

        BattleCardUI battleCardUi = FindObjectOfType<BattleCardUI>(true);
        if (battleCardUi != null)
        {
            return IconProvider.FromBattleCardUI(battleCardUi);
        }

        if (!warnedIconSourceMissing)
        {
            warnedIconSourceMissing = true;
            Debug.LogWarning("SkillIconLegendUI: cannot find SkillIconAssets or BattleCardUI icon source.");
        }

        return null;
    }

    private string GetTriggerThaiLabel(EffectTrigger trigger)
    {
        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return "เมื่อลงสนาม";
            case EffectTrigger.OnStrike: return "เมื่อโจมตี";
            case EffectTrigger.OnStrikeHit: return "เมื่อโจมตีโดน";
            case EffectTrigger.Continuous: return "เอฟเฟกต์ต่อเนื่อง";
            case EffectTrigger.OnIntercept: return "เมื่อ Intercept";
            case EffectTrigger.OnDestroyed: return "เมื่อถูกทำลาย";
            case EffectTrigger.OnTurnEnd: return "เมื่อจบเทิร์น";
            default: return "ไม่ระบุ";
        }
    }

    private string GetActionThaiLabel(ActionType action)
    {
        switch (action)
        {
            case ActionType.Destroy: return "ทำลาย";
            case ActionType.DisableAttack: return "ปิดการโจมตี";
            case ActionType.DisableAbility: return "ปิดความสามารถ";
            case ActionType.RevealHand: return "เปิดดูมือ";
            case ActionType.RevealHandMultiple: return "เปิดดูมือหลายใบ";
            case ActionType.DiscardDeck: return "ทิ้งการ์ดจากเด็ค";
            case ActionType.SummonToken: return "อัญเชิญ Token";
            case ActionType.ModifyStat: return "ปรับค่าสเตตัส";
            case ActionType.ControlEquip: return "ควบคุม Equip";
            case ActionType.HealHP: return "ฟื้นฟู HP";
            case ActionType.ForceIntercept: return "บังคับ Intercept";
            case ActionType.BypassIntercept: return "โจมตีทะลุ Intercept";
            case ActionType.DisableIntercept: return "ปิด Intercept";
            case ActionType.DrawCard: return "จั่วการ์ด";
            case ActionType.Rush: return "Rush";
            case ActionType.DoubleStrike: return "โจมตีสองครั้ง";
            case ActionType.GraveyardATK: return "พลังโจมตีจากสุสาน";
            case ActionType.ZeroStats: return "ทำให้ค่าสถานะเป็น 0";
            case ActionType.RemoveCategory: return "ลบหมวดหมู่";
            case ActionType.ForceChooseDiscard: return "บังคับเลือกทิ้ง";
            case ActionType.ReturnEquipFromGraveyard: return "คืน Equip จากสุสาน";
            case ActionType.PeekDiscardTopDeck: return "ดู/ทิ้งการ์ดบนสุดเด็ค";
            case ActionType.MarkInterceptMillDeck: return "ติดเครื่องหมาย Intercept และตัดเด็ค";
            case ActionType.InterceptAlwaysTypeMatch: return "Intercept ตามประเภทเสมอ";
            case ActionType.ProtectDrawnCards: return "ป้องกันการ์ดที่จั่ว";
            case ActionType.ProtectRevealHandMultiple: return "ป้องกันการเปิดดูมือ";
            case ActionType.ProtectForceInterceptEquip: return "ป้องกัน Equip จากการบังคับ Intercept";
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy: return "ป้องกัน Equip จากการถูกทำลายด้วยสกิล";
            case ActionType.HealOnMonsterSummoned: return "ฟื้นฟูเมื่ออัญเชิญมอนสเตอร์";
            default: return "ไม่ระบุ";
        }
    }

    private class LegendEntry
    {
        public string Key;
        public Sprite Icon;
        public string Description;
    }

    private class IconProvider
    {
        public Sprite iconOnDeploy;
        public Sprite iconOnStrike;
        public Sprite iconOnStrikeHit;
        public Sprite iconOnIntercept;
        public Sprite iconOnDestroyed;
        public Sprite iconOnTurnEnd;
        public Sprite iconContinuous;
        public Sprite iconDestroy;
        public Sprite iconDisableAttack;
        public Sprite iconDisableAbility;
        public Sprite iconRevealHand;
        public Sprite iconDiscardDeck;
        public Sprite iconSummonToken;
        public Sprite iconModifyStat;
        public Sprite iconControlEquip;
        public Sprite iconHealHP;
        public Sprite iconForceIntercept;
        public Sprite iconBypassIntercept;
        public Sprite iconDisableIntercept;
        public Sprite iconDrawCard;
        public Sprite iconRush;
        public Sprite iconDoubleStrike;
        public Sprite iconGraveyardATK;
        public Sprite iconZeroStats;
        public Sprite iconRemoveCategory;
        public Sprite iconForceChooseDiscard;
        public Sprite iconReturnEquipFromGraveyard;
        public Sprite iconPeekDiscardTopDeck;
        public Sprite iconMarkInterceptMillDeck;
        public Sprite iconProtection;

        public static IconProvider FromAssets(SkillIconAssets assets)
        {
            return new IconProvider
            {
                iconOnDeploy = assets.iconOnDeploy,
                iconOnStrike = assets.iconOnStrike,
                iconOnStrikeHit = assets.iconOnStrikeHit,
                iconOnIntercept = assets.iconOnIntercept,
                iconOnDestroyed = assets.iconOnDestroyed,
                iconOnTurnEnd = assets.iconOnTurnEnd,
                iconContinuous = assets.iconContinuous,
                iconDestroy = assets.iconDestroy,
                iconDisableAttack = assets.iconDisableAttack,
                iconDisableAbility = assets.iconDisableAbility,
                iconRevealHand = assets.iconRevealHand,
                iconDiscardDeck = assets.iconDiscardDeck,
                iconSummonToken = assets.iconSummonToken,
                iconModifyStat = assets.iconModifyStat,
                iconControlEquip = assets.iconControlEquip,
                iconHealHP = assets.iconHealHP,
                iconForceIntercept = assets.iconForceIntercept,
                iconBypassIntercept = assets.iconBypassIntercept,
                iconDisableIntercept = assets.iconDisableIntercept,
                iconDrawCard = assets.iconDrawCard,
                iconRush = assets.iconRush,
                iconDoubleStrike = assets.iconDoubleStrike,
                iconGraveyardATK = assets.iconGraveyardATK,
                iconZeroStats = assets.iconZeroStats,
                iconRemoveCategory = assets.iconRemoveCategory,
                iconForceChooseDiscard = assets.iconForceChooseDiscard,
                iconReturnEquipFromGraveyard = assets.iconReturnEquipFromGraveyard,
                iconPeekDiscardTopDeck = assets.iconPeekDiscardTopDeck,
                iconMarkInterceptMillDeck = assets.iconMarkInterceptMillDeck,
                iconProtection = assets.iconProtection
            };
        }

        public static IconProvider FromBattleCardUI(BattleCardUI battleUi)
        {
            return new IconProvider
            {
                iconOnDeploy = battleUi.iconOnDeploy,
                iconOnStrike = battleUi.iconOnStrike,
                iconOnStrikeHit = battleUi.iconOnStrikeHit,
                iconOnIntercept = battleUi.iconOnIntercept,
                iconOnDestroyed = battleUi.iconOnDestroyed,
                iconOnTurnEnd = battleUi.iconOnTurnEnd,
                iconContinuous = battleUi.iconContinuous,
                iconDestroy = battleUi.iconDestroy,
                iconDisableAttack = battleUi.iconDisableAttack,
                iconDisableAbility = battleUi.iconDisableAbility,
                iconRevealHand = battleUi.iconRevealHand,
                iconDiscardDeck = battleUi.iconDiscardDeck,
                iconSummonToken = battleUi.iconSummonToken,
                iconModifyStat = battleUi.iconModifyStat,
                iconControlEquip = battleUi.iconControlEquip,
                iconHealHP = battleUi.iconHealHP,
                iconForceIntercept = battleUi.iconForceIntercept,
                iconBypassIntercept = battleUi.iconBypassIntercept,
                iconDisableIntercept = battleUi.iconDisableIntercept,
                iconDrawCard = battleUi.iconDrawCard,
                iconRush = battleUi.iconRush,
                iconDoubleStrike = battleUi.iconDoubleStrike,
                iconGraveyardATK = battleUi.iconGraveyardATK,
                iconZeroStats = battleUi.iconZeroStats,
                iconRemoveCategory = battleUi.iconRemoveCategory,
                iconForceChooseDiscard = battleUi.iconForceChooseDiscard,
                iconReturnEquipFromGraveyard = battleUi.iconReturnEquipFromGraveyard,
                iconPeekDiscardTopDeck = battleUi.iconPeekDiscardTopDeck,
                iconMarkInterceptMillDeck = battleUi.iconMarkInterceptMillDeck,
                iconProtection = battleUi.iconProtection
            };
        }
    }
}