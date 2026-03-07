using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUISlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image cardImage;
    public Image frameImage; // 🔥 กรอบการ์ด
    public Button btn;
    public TextMeshProUGUI amountText;

    [Header("Card Frame")]
    public Sprite commonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    public Sprite legendaryFrame;

    [Header("Skill Icon Container")]
    private Image skillIconContainer;
    private Image[] skillIcons = new Image[4];

    [Header("Trigger Icon Sprites (ตั้งใน Inspector)")]
    public Sprite iconOnDeploy;
    public Sprite iconOnStrike;
    public Sprite iconOnStrikeHit;
    public Sprite iconOnIntercept;
    public Sprite iconOnDestroyed;
    public Sprite iconOnTurnEnd;
    public Sprite iconContinuous;
    
    [Header("Action Icon Sprites (ตั้งใน Inspector)")]
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

    private CardData _data;
    private UnityAction<CardData> _onLeftClick;
    private UnityAction<CardData> _onRightClick;

    private Vector3 originalScale;
    private Coroutine currentAnim;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Setup(CardData data, int amount, UnityAction<CardData> leftClick, UnityAction<CardData> rightClick)
    {
        _data = data;
        _onLeftClick = leftClick;
        _onRightClick = rightClick;

        if (currentAnim != null) StopCoroutine(currentAnim);
        transform.localScale = originalScale;

        if (data.artwork != null) {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        } else {
            cardImage.color = Color.red; 
        }

        ApplyFrameByRarity();
        
        // 🔥 สร้างและอัพเดตไอคอนสกิล
        CreateSkillIconsIfNeeded();
        UpdateSkillIconDisplay();

        if (amountText != null) amountText.text = (amount >= 0) ? $"x{amount}" : "";
        if (amount == 0) cardImage.color = Color.gray;
        
        // 🔥 Debug log
        if (data.card_id.Contains("Fire") || data.card_id.Contains("Ice"))
        {
            Debug.Log($"🖼️ CardUISlot.Setup({data.card_id}): Displayed amount = {amount}");
        }

        btn.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale * 1.05f, 0.1f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale, 0.1f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        StartCoroutine(ClickEffect());

        if (eventData.button == PointerEventData.InputButton.Left) _onLeftClick?.Invoke(_data);
        else if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke(_data);
    }

    IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t); 
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator ClickEffect()
    {
        yield return StartCoroutine(AnimateScale(originalScale * 0.9f, 0.05f));
        yield return StartCoroutine(AnimateScale(originalScale * 1.1f, 0.05f));
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

    void ApplyFrameByRarity()
    {
        if (_data == null) return;
        EnsureFrameImage();
        if (frameImage == null) return;

        Sprite rarityFrame = null;
        switch (_data.rarity)
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

    // 🔥 สร้าง Skill Icon Container และ Icons
    void CreateSkillIconsIfNeeded()
    {
        // สร้าง Container
        if (skillIconContainer == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "SkillIconContainer") Destroy(child.gameObject);
            }

            GameObject containerObj = new GameObject("SkillIconContainer");
            containerObj.transform.SetParent(transform, false);
            containerObj.transform.SetAsLastSibling();

            skillIconContainer = containerObj.AddComponent<Image>();
            skillIconContainer.raycastTarget = false;
            skillIconContainer.color = new Color(0.05f, 0.05f, 0.05f, 0.90f);
            skillIconContainer.gameObject.SetActive(false);

            RectTransform containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.06f, 0.78f);
            containerRect.anchorMax = new Vector2(0.97f, 0.90f);
            containerRect.pivot = new Vector2(0, 1);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var outline = containerObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // สร้าง 4 Icon Slots
        for (int i = 0; i < 4; i++)
        {
            if (skillIcons[i] == null)
            {
                string iconName = $"SkillIcon_{i}";
                foreach (Transform child in skillIconContainer.transform)
                {
                    if (child.name == iconName) Destroy(child.gameObject);
                }

                GameObject iconObj = new GameObject(iconName);
                iconObj.transform.SetParent(skillIconContainer.transform, false);
                iconObj.transform.SetAsLastSibling();

                skillIcons[i] = iconObj.AddComponent<Image>();
                skillIcons[i].raycastTarget = false;
                skillIcons[i].preserveAspect = true;
                skillIcons[i].gameObject.SetActive(false);

                float startX = i * 0.25f + 0.02f;
                float endX = (i + 1) * 0.25f - 0.02f;
                
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(startX, 0.1f);
                iconRect.anchorMax = new Vector2(endX, 0.9f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
            }
        }
    }

    // 🔥 อัพเดตการแสดง Skill Icons
    void UpdateSkillIconDisplay()
    {
        if (_data == null) return;

        int iconIndex = 0;
        bool showAnyIcon = false;

        // ซ่อน icons ทั้งหมดก่อน
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] != null) skillIcons[i].gameObject.SetActive(false);
        }

        // รวบรวม sprites ที่ต้องแสดง
        var spritesToShow = new System.Collections.Generic.List<Sprite>();

        // Trigger sprites
        foreach (var effect in _data.effects)
        {
            if (spritesToShow.Count >= 4) break;
            if (effect.trigger != EffectTrigger.None)
            {
                Sprite triggerSprite = GetTriggerSprite(effect.trigger);
                if (triggerSprite != null && !spritesToShow.Contains(triggerSprite))
                {
                    spritesToShow.Add(triggerSprite);
                }
            }
        }

        // Action sprites
        foreach (var effect in _data.effects)
        {
            if (spritesToShow.Count >= 4) break;
            if (effect.action != ActionType.None)
            {
                Sprite actionSprite = GetActionSprite(effect.action);
                if (actionSprite != null && !spritesToShow.Contains(actionSprite))
                {
                    spritesToShow.Add(actionSprite);
                }
            }
        }

        // แสดง sprites
        foreach (var sprite in spritesToShow)
        {
            if (iconIndex >= 4) break;
            if (skillIcons[iconIndex] != null)
            {
                skillIcons[iconIndex].sprite = sprite;
                skillIcons[iconIndex].color = Color.white;
                skillIcons[iconIndex].gameObject.SetActive(true);
                showAnyIcon = true;
                iconIndex++;
            }
        }

        // ปรับขนาด container ตามจำนวนไอคอน
        if (skillIconContainer != null)
        {
            if (showAnyIcon && iconIndex > 0)
            {
                skillIconContainer.gameObject.SetActive(true);

                RectTransform containerRect = skillIconContainer.GetComponent<RectTransform>();
                float widthPerIcon = 0.22f;
                float endX = 0.06f + (iconIndex * widthPerIcon);
                endX = Mathf.Min(endX, 0.97f);

                containerRect.anchorMin = new Vector2(0.06f, 0.78f);
                containerRect.anchorMax = new Vector2(endX, 0.90f);

                // ปรับ position ของ icons
                float iconWidth = 1f / iconIndex;
                for (int i = 0; i < iconIndex; i++)
                {
                    if (skillIcons[i] != null)
                    {
                        RectTransform iconRect = skillIcons[i].GetComponent<RectTransform>();
                        float startX = i * iconWidth + 0.02f;
                        float endXIcon = (i + 1) * iconWidth - 0.02f;
                        iconRect.anchorMin = new Vector2(startX, 0.1f);
                        iconRect.anchorMax = new Vector2(endXIcon, 0.9f);
                    }
                }
            }
            else
            {
                skillIconContainer.gameObject.SetActive(false);
            }
        }
    }

    // 🎯 รับ Sprite ของ Trigger
    Sprite GetTriggerSprite(EffectTrigger trigger)
    {
        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return iconOnDeploy;
            case EffectTrigger.OnStrike: return iconOnStrike;
            case EffectTrigger.OnStrikeHit: return iconOnStrikeHit;
            case EffectTrigger.OnIntercept: return iconOnIntercept;
            case EffectTrigger.OnDestroyed: return iconOnDestroyed;
            case EffectTrigger.OnTurnEnd: return iconOnTurnEnd;
            case EffectTrigger.Continuous: return iconContinuous;
            default: return null;
        }
    }

    // ⚡ รับ Sprite ของ Action
    Sprite GetActionSprite(ActionType action)
    {
        switch (action)
        {
            case ActionType.Destroy: return iconDestroy;
            case ActionType.DisableAttack: return iconDisableAttack;
            case ActionType.DisableAbility: return iconDisableAbility;
            case ActionType.RevealHand:
            case ActionType.RevealHandMultiple: return iconRevealHand;
            case ActionType.DiscardDeck: return iconDiscardDeck;
            case ActionType.SummonToken: return iconSummonToken;
            case ActionType.ModifyStat: return iconModifyStat;
            case ActionType.ControlEquip: return iconControlEquip;
            case ActionType.HealHP:
            case ActionType.HealOnMonsterSummoned: return iconHealHP;
            case ActionType.ForceIntercept: return iconForceIntercept;
            case ActionType.BypassIntercept: return iconBypassIntercept;
            case ActionType.DisableIntercept: return iconDisableIntercept;
            case ActionType.DrawCard: return iconDrawCard;
            case ActionType.Rush: return iconRush;
            case ActionType.DoubleStrike: return iconDoubleStrike;
            case ActionType.GraveyardATK: return iconGraveyardATK;
            case ActionType.ZeroStats: return iconZeroStats;
            case ActionType.RemoveCategory: return iconRemoveCategory;
            case ActionType.ForceChooseDiscard: return iconForceChooseDiscard;
            case ActionType.ReturnEquipFromGraveyard: return iconReturnEquipFromGraveyard;
            case ActionType.PeekDiscardTopDeck: return iconPeekDiscardTopDeck;
            case ActionType.MarkInterceptMillDeck: return iconMarkInterceptMillDeck;
            case ActionType.InterceptAlwaysTypeMatch: return iconForceIntercept;
            case ActionType.ProtectDrawnCards:
            case ActionType.ProtectRevealHandMultiple:
            case ActionType.ProtectForceInterceptEquip:
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy: return iconProtection;
            default: return null;
        }
    }
}