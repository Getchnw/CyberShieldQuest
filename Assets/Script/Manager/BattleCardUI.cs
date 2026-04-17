using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems;

// เพิ่ม Interface ให้ครบ ทั้งการคลิกและการลาก
public class BattleCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    private Image artworkImage;
    private Image frameImage; // 🔥 กรอบการ์ด
    private TextMeshProUGUI atkText; // 🔥 แสดงพลังปัจจุบัน (ซ้ายล่าง)
    private TextMeshProUGUI costText; // 🔥 แสดงคอส (ขวาบน)
    private TextMeshProUGUI statusText; // 🔥 แสดงสถานะพิเศษบนการ์ด (เช่น ห้าม Intercept)
    
    [Header("Card Symbol Icons")]
    private Image skillIconContainer; // 🎨 ฐานรองสำหรับโซนแสดงไอคอนสกิล
    private Image[] skillIcons = new Image[4]; // 🔥 ไอคอนสกิล 4 ช่อง (รองรับหลายสกิล)
    private Image triggerIcon; // 🎯 แสดงไอคอน Trigger (รูป) - backward compat
    private Image actionIcon; // ⚡ แสดงไอคอน Action (รูป) - backward compat
    private TextMeshProUGUI triggerSymbol; // 🎯 แสดงสัญลักษณ์ Trigger (fallback)
    private TextMeshProUGUI actionSymbol; // ⚡ แสดงสัญลักษณ์ Action (fallback)
    private TextMeshProUGUI subCategoryBadge; // 🏷️ แสดง SubCategory (ด้านล่างขวา)
    
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
    public Sprite iconProtection; // ใช้ร่วมกันสำหรับทุก Protect action

    [Header("Card Frame")]
    public Sprite frameSprite; // 🔥 Sprite ของกรอบการ์ด (ตั้งใน Inspector)
    public Sprite commonFrame;
    public Sprite rareFrame;
    public Sprite epicFrame;
    public Sprite legendaryFrame;
    
    private CardData _cardData;
    private CanvasGroup canvasGroup; // ตัวช่วยให้เมาส์ทะลุการ์ดตอนลาก

    // 🔥 ตัวแปรเช็คสถานะ
    public bool isOnField = false; 
    public Transform parentAfterDrag; // จำตำแหน่งเดิมก่อนลาก
    public bool hasAttacked = false;
    public int attacksThisTurn = 0; // จำนวนครั้งที่โจมตีในเทิร์นนี้
    public bool isManualHighlight = false; // ถ้า true = อย่าให้ auto-highlight แตะสี
    private bool mulliganSelected = false;
    
    // 🔥 ตัวแปร override stat สำหรับ ZeroStats (ต่อ card instance)
    private int modifiedCost = -1; // -1 = ใช้ original, >= 0 = ค่า override
    private int modifiedATK = -1;  // -1 = ใช้ original, >= 0 = ค่า override
    
    // 🎯 Intercept System
    public bool canBypassIntercept = false; // การ์ดนี้โจมตีข้ามการกันได้
    public int bypassCostThreshold = 0; // ข้ามการกันได้เฉพาะ Equip ที่ cost < threshold (0 = ไม่ข้ามไม่ได้, -1 = ข้ามทั้งหมด)
    public MainCategory bypassAllowedMainCat = MainCategory.General; // MainCategory ที่สามารถ Intercept ได้ (General = ข้ามทั้งหมด)
    public SubCategory bypassAllowedSubCat = SubCategory.General; // SubCategory ที่สามารถ Intercept ได้ (General = ข้ามทั้งหมด)
    public bool mustIntercept = false; // การ์ดนี้ต้องกันการโจมตีถัดไปบังคับ
    public bool cannotIntercept = false; // การ์ดนี้ไม่สามารถกันการโจมตีได้ในเทิร์นนี้
    public BattleCardUI markedInterceptTarget = null; // เป้าหมาย Equip ที่ถูกเลือกไว้สำหรับเงื่อนไข Intercept
    public int markedInterceptMillCount = 0; // จำนวนการ์ดเด็คที่ต้องส่งลงสุสานเมื่อ trigger สำเร็จ
    public bool hasLostCategory = false; // Category lost from effect (independent of ATK/HP = 0)
    public int categoryLostTurnsRemaining = 0; // จำนวนเทิร์นที่เหลือก่อนคืน category: 0 = ไม่เสีย, -1 = ตลอด, >= 1 = จำนวนเทิร์น
    
    // � ตัวแปรสำหรับ ControlEquip (Equip Spell ที่ถูกควบคุม)
    public bool isControlled = false; // การ์ดนี้กำลังถูกควบคุมโดยฝ่ายตรงข้าม
    public int controlledTurnsRemaining = 0; // จำนวนเทิร์นที่เหลือของการควบคุม: 0 = ไม่ถูกควบคุม, -1 = ตลอด, >= 1 = จำนวนเทิร์น
    public Transform originalEquipSlot = null; // ตำแหน่ง slot เดิมที่ควรคืนการ์ดไป
    public bool originalOwnerIsPlayer = true; // เจ้าของดั้งเดิมของการ์ด (ใช้เมื่อส่งให้สุสาน)
    
    // 🎮 ตัวแปรสำหรับอนิเมชั่นลอย
    private float floatTime = 0f;
    private Vector3 originalPosition = Vector3.zero;
    private bool isFloating = false;
    private Coroutine bounceAnimationCoroutine = null;

    // 🗑️ ตัวแปรสำหรับ Force Choose Discard
    private BattleCardUI referenceCard = null; // เก็บ reference ของการ์ดจริง (สำหรับ UI ที่ copy มา)

    public void SetReferenceCard(BattleCardUI original)
    {
        referenceCard = original;
    }

    public BattleCardUI GetReferenceCard()
    {
        return referenceCard;
    }

    void Awake()
    {
        CreateUIElementsIfNeeded();

        // เพิ่ม CanvasGroup อัตโนมัติ (จำเป็นมากสำหรับระบบลาก)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        // 🎈 อนิเมชั่นลอยขึ้นลงเบาๆ (หยุดใน handArea เพราะ HorizontalLayoutGroup จะจัดตำแหน่งเอง)
        if (isFloating && !isOnField && transform.parent != null)
        {
            // 🔥 เช็คว่า parent เป็น handArea หรือไม่
            Transform p = transform.parent;
            if (p != null && (p.name == "HandArea" || p.name == "handArea"))
            {
                // 🔥 อยู่ในมือ -> หยุดลอย ปล่อยให้ HorizontalLayoutGroup ทำงาน
                return;
            }
            
            floatTime += Time.deltaTime;
            float floatOffset = Mathf.Sin(floatTime * 2f) * 10f; // ลอยขึ้นลง 10 pixels
            transform.localPosition = originalPosition + Vector3.up * floatOffset;
        }

        // 🔥 อัพเดตแสดงพลังปัจจุบัน
        UpdateATKDisplay();
        UpdateStatusDisplay();

        // 🔥 Auto-highlight สำหรับการ์ดบนสนาม
        if (isOnField && _cardData != null && artworkImage != null)
        {
            // ⚠️ ถ้าเป็น manual highlight (effect เลือกเป้าหมาย) ห้ามแตะ
            if (isManualHighlight)
            {
                return;
            }

            // ⚠️ ถ้าอยู่ใน DEFENDER_CHOICE state ให้ใช้ manual highlight แทน (ห้าม override)
            if (BattleManager.Instance != null && BattleManager.Instance.state == BattleState.DEFENDER_CHOICE)
            {
                return; // ปล่อยให้ HighlightInterceptableShields() จัดการ
            }

            // 🟣 ถ้าการ์ดสูญเสีย Category แล้ว ให้คงสีม่วงไว้ (ห้ามแตะ)
            if (hasLostCategory)
            {
                return;
            }

            // 🚫 ถ้า EquipSpell ไม่สามารถ Intercept ได้ ให้แสดงสีแดงอ่อน (ห้ามแตะ)
            if (_cardData.type == CardType.EquipSpell && cannotIntercept)
            {
                if (artworkImage.color != new Color(1f, 0.5f, 0.5f, 1f))
                {
                    artworkImage.color = new Color(1f, 0.5f, 0.5f, 1f); // แดงอ่อน - Cannot Intercept
                }
                return;
            }

            // 🚀 ถ้า Monster/Token มีสถานะข้ามการกัน ให้แสดงสีฟ้าไซเบอร์ชัดเจน
            if ((_cardData.type == CardType.Monster || _cardData.type == CardType.Token) && canBypassIntercept)
            {
                Color bypassColor = new Color(0.45f, 0.95f, 1f, 1f);
                if (artworkImage.color != bypassColor)
                {
                    artworkImage.color = bypassColor;
                }
                return;
            }

            bool shouldHighlight = false;
            bool shouldBeDark = false; // Monster ที่มี summoning sickness

            // EquipSpell สว่างตลอดเวลา (ยกเว้นถ้า cannotIntercept)
            if (_cardData.type == CardType.EquipSpell)
            {
                shouldHighlight = true;
            }
            // Monster/Token: สว่างเมื่อโจมตีได้, มืดเมื่อยังโจมตีไม่ได้ (และเป็นเทิร์นผู้เล่น)
            else if ((_cardData.type == CardType.Monster || _cardData.type == CardType.Token) && BattleManager.Instance != null)
            {
                bool isPlayerTurn = BattleManager.Instance.state == BattleState.PLAYERTURN;
                if (isPlayerTurn && CanAttackNow())
                {
                    shouldHighlight = true;
                }
                else if (isPlayerTurn && !CanAttackNow())
                {
                    shouldBeDark = true; // Summoning Sickness หรือโจมตีครบแล้ว
                }
            }

            // ตั้งค่าสี
            if (shouldHighlight && artworkImage.color != new Color(1.5f, 1.5f, 1.5f, 1f))
            {
                artworkImage.color = new Color(1.5f, 1.5f, 1.5f, 1f); // สว่างขึ้น 50%
            }
            else if (shouldBeDark && artworkImage.color != Color.gray)
            {
                artworkImage.color = Color.gray; // มืด (Summoning Sickness)
            }
            else if (!shouldHighlight && !shouldBeDark && artworkImage.color != Color.white)
            {
                artworkImage.color = Color.white; // ปกติ
            }
        }
    }

    void UpdateATKDisplay()
    {
        // แสดง ATK และ Cost เสมอ (มือ, Panel, สนาม)
        if (atkText == null || costText == null) return;
        
        // 🔥 ถ้าอยู่ใน Hand Reveal Panel ให้ซ่อน Cost/ATK
        if (BattleManager.Instance != null && BattleManager.Instance.handRevealListRoot != null)
        {
            if (transform.IsChildOf(BattleManager.Instance.handRevealListRoot))
            {
                atkText.gameObject.SetActive(false);
                costText.gameObject.SetActive(false);
                return;
            }
        }
        
        // 🔥 ถ้า frameImage ถูก hide ไป = Card Back -> ซ่อน Cost/ATK
        if (frameImage != null && frameImage.color.a == 0f)
        {
            atkText.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
            return;
        }
        
        // 🔥 เช็คว่าอยู่ใน Battle Scene หรือไม่
        bool inBattleScene = BattleManager.Instance != null;
        
        if (inBattleScene && _cardData != null)
        {
            // 🔥 แสดง ATK (มุมซ้ายล่าง) - เฉพาะ Monster/Token
            if (_cardData.type == CardType.Monster || _cardData.type == CardType.Token)
            {
                bool isOwnerPlayerSide = BattleManager.Instance == null || BattleManager.Instance.IsCardOwnedByPlayerSide(this);
                // ใช้ GetModifiedATK() เพื่อแสดงพลังปัจจุบันที่คำนึงถึงสกิลทั้งหมด
                int currentATK = GetModifiedATK(isPlayerAttack: isOwnerPlayerSide);
                
                // ถ้ามี GraveyardATK ให้แสดงเป็นสีเขียว
                if (TryGetGraveyardATKEffect(out CardEffect graveyardEffect) && currentATK > _cardData.atk)
                {
                    atkText.color = new Color(0.5f, 1f, 0.5f); // สีเขียวอ่อน
                }
                else
                {
                    atkText.color = Color.white;
                }
                
                atkText.text = currentATK.ToString();
                atkText.gameObject.SetActive(true);
            }
            else
            {
                atkText.gameObject.SetActive(false);
            }

            // 🔥 แสดง Cost (มุมขวาบน) - แสดงเสมอในมือ Panel หรือสนาม
            costText.text = GetCost().ToString(); // 🔥 ใช้ GetCost() เพื่อให้ได้ค่า override ถ้ามี
            costText.color = Color.white;
            costText.gameObject.SetActive(true);
        }
        else
        {
            // ซ่อนตัวเลขถ้าไม่อยู่ใน Battle Scene
            atkText.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
    }

    void UpdateStatusDisplay()
    {
        if (statusText == null) return;

        // ใช้การเปลี่ยนสีการ์ดแทนข้อความสถานะ
        statusText.gameObject.SetActive(false);
    }

    // 🔥 เมธอด public สำหรับอัปเดต Cost/ATK display (ใช้เมื่อ ui.enabled = false)
    public void RefreshCardDisplay()
    {
        if (atkText == null || costText == null) return;
        
        // 🔥 ถ้าอยู่ใน Hand Reveal Panel ให้ซ่อน Cost/ATK
        if (BattleManager.Instance != null && BattleManager.Instance.handRevealListRoot != null)
        {
            if (transform.IsChildOf(BattleManager.Instance.handRevealListRoot))
            {
                HideCardInfo();
                return;
            }
        }
        
        // 🔥 เช็คว่าเป็น Card Back หรือไม่ (frameImage alpha = 0)
        bool isCardBack = frameImage != null && frameImage.color.a == 0f;
        
        if (isCardBack)
        {
            HideCardInfo();
            return;
        }
        
        // 🔥 ดึงค่า Cost และ ATK โดยตรง
        bool inBattleScene = BattleManager.Instance != null;
        
        if (inBattleScene && _cardData != null)
        {
            // แสดง Cost
            costText.text = GetCost().ToString();
            costText.color = Color.white;
            costText.gameObject.SetActive(true);
            
            // แสดง ATK ถ้าเป็น Monster/Token
            if (_cardData.type == CardType.Monster || _cardData.type == CardType.Token)
            {
                bool isOwnerPlayerSide = BattleManager.Instance == null || BattleManager.Instance.IsCardOwnedByPlayerSide(this);
                int currentATK = GetModifiedATK(isPlayerAttack: isOwnerPlayerSide);
                if (TryGetGraveyardATKEffect(out CardEffect graveyardEffect) && currentATK > _cardData.atk)
                {
                    atkText.color = new Color(0.5f, 1f, 0.5f); // สีเขียว
                }
                else
                {
                    atkText.color = Color.white;
                }
                
                atkText.text = currentATK.ToString();
                atkText.gameObject.SetActive(true);
            }
            else
            {
                atkText.gameObject.SetActive(false);
            }
        }
        else
        {
            costText.gameObject.SetActive(false);
            atkText.gameObject.SetActive(false);
        }
    }

    public int GetMaxAttacksPerTurn()
    {
        if (_cardData == null) return 1;
        bool hasDoubleStrike;

        if (BattleManager.Instance != null)
        {
            hasDoubleStrike = BattleManager.Instance.HasActiveContinuousAction(this, ActionType.DoubleStrike);
        }
        else
        {
            var effects = _cardData.effects ?? new System.Collections.Generic.List<CardEffect>();
            hasDoubleStrike = effects.Any(e => e.trigger == EffectTrigger.Continuous && e.action == ActionType.DoubleStrike);
        }

        return hasDoubleStrike ? 2 : 1;
    }

    public bool CanAttackNow()
    {
        if (_cardData == null) return false;
        if (_cardData.type != CardType.Monster && _cardData.type != CardType.Token) return false;
        if (hasAttacked) return false;
        if (BattleManager.Instance != null && BattleManager.Instance.IsMonsterAttackBlockedByContinuousEffect(this)) return false;
        return attacksThisTurn < GetMaxAttacksPerTurn();
    }

    public int GetModifiedATK(bool isPlayerAttack = true)
    {
        if (_cardData == null) return 0;
        
        // 🔥 ถ้าถูก ZeroStats ให้คืน 0 เสมอ
        if (modifiedATK >= 0)
        {
            return modifiedATK;
        }
        
        int baseATK = _cardData.atk;

        // 🔥 เช็คสกิล GraveyardATK (เพิ่มพลังตามจำนวนการ์ดในสุสาน)
        if (!TryGetGraveyardATKEffect(out CardEffect graveyardEffect))
        {
            return baseATK;
        }
        
        int graveCount = 0;
        
        // ถ้า player โจมตี นับสุสานของ Bot ถ้าเป็น Bot นับสุสานของ Player
        if (BattleManager.Instance != null)
        {
            if (isPlayerAttack)
            {
                graveCount = BattleManager.Instance.GetEnemyGraveyardCount();
            }
            else
            {
                graveCount = BattleManager.Instance.GetPlayerGraveyardCount();
            }
        }
        
        // 🔥 คำนวณ ATK: +1 ต่อทุกๆ 2 ใบ (หารด้วย 2 แล้วปัดลง)
        int extraATK = (graveCount / 2) * graveyardEffect.value;
        Debug.Log($"🔥 GraveyardATK [{_cardData.cardName}]: Trigger={graveyardEffect.trigger}, Base={baseATK}, Graves={graveCount}, Per2Cards={graveCount/2}, Value={graveyardEffect.value}, Extra={extraATK}, Total={baseATK + extraATK}");
        return baseATK + extraATK;
    }

    bool TryGetGraveyardATKEffect(out CardEffect graveyardEffect)
    {
        graveyardEffect = default;

        if (_cardData == null || _cardData.effects == null || _cardData.effects.Count == 0)
        {
            return false;
        }

        graveyardEffect = _cardData.effects.FirstOrDefault(e => e.action == ActionType.GraveyardATK &&
                                                                (e.trigger == EffectTrigger.OnStrike || e.trigger == EffectTrigger.Continuous));
        return graveyardEffect.action == ActionType.GraveyardATK;
    }

    /// <summary>
    /// Returns the effective SubCategory of this card.
    /// If hasLostCategory is true, returns General instead.
    /// For Monster/Token only: if ATK/HP are both 0, returns General.
    /// </summary>
    public SubCategory GetModifiedSubCategory()
    {
        if (_cardData == null) return SubCategory.General;
        
        // Category lost from effect
        if (hasLostCategory) return SubCategory.General;
        
        // Category lost when ATK and HP are both 0 (เฉพาะ Monster/Token เท่านั้น)
        // หมายเหตุ: Equip/Spell ปกติมี ATK/HP = 0 จึงไม่ควรถูกลดเป็น General อัตโนมัติ
        if ((_cardData.type == CardType.Monster || _cardData.type == CardType.Token)
            && _cardData.atk == 0 && _cardData.hp == 0)
        {
            return SubCategory.General;
        }
        
        return _cardData.subCategory;
    }

    /// <summary>
    /// Removes the SubCategory of this card (sets hasLostCategory flag).
    /// Visual feedback: Apply magenta/purple tint when category is lost.
    /// </summary>
    /// <param name="duration">0 = permanent (forever), >= 1 = number of turns</param>
    public void RemoveSubCategory(int duration = 0)
    {
        hasLostCategory = true;
        
        // Set duration: 0 = permanent (-1 internally), >= 1 = turn count
        categoryLostTurnsRemaining = (duration == 0) ? -1 : duration;
        
        // Apply magenta/purple tint as strong visual feedback for lost category
        // Use a distinct color different from other states (gray=summoning sickness, white=normal, bright=ready)
        Color categoryLostColor = new Color(1f, 0.5f, 1f, 1f); // Magenta/Pink-Purple
        
        // Change artwork image color (main visual)
        if (artworkImage != null)
        {
            artworkImage.color = categoryLostColor;
        }
        
        // Also change main Image component if exists
        var img = GetComponent<Image>();
        if (img != null && img != artworkImage)
        {
            img.color = categoryLostColor;
        }
        
        string durationText = (duration == 0) ? "permanent" : $"{duration} turn(s)";
        Debug.Log($"[RemoveSubCategory] {(_cardData != null ? _cardData.cardName : "Unknown")} lost its category → {durationText} (turnsRemaining={categoryLostTurnsRemaining})");
    }

    void CreateUIElementsIfNeeded()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) rectTransform = gameObject.AddComponent<RectTransform>();
        
        // กำหนดขนาดมาตรฐานการ์ด (ขนาดในมือ)
        rectTransform.sizeDelta = new Vector2(140, 200); 

        if (artworkImage == null)
        {
            artworkImage = GetComponent<Image>();
            if (artworkImage == null) artworkImage = gameObject.AddComponent<Image>();
            
            artworkImage.color = Color.white;
            artworkImage.raycastTarget = true; 
            }

            // 🔥 สร้างกรอบการ์ด (ถ้ายังไม่มี)
            if (frameImage == null)
            {
                // ลบ CardFrame เก่าทั้งหมดก่อน (ป้องกันการสร้างซ้ำ)
                foreach (Transform child in transform)
                {
                    if (child.name == "CardFrame")
                    {
                        Destroy(child.gameObject);
                    }
                }

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

            // 🔥 ใส่ Sprite กรอบ (ถ้ามี)
            if (frameImage != null)
            {
                if (frameSprite != null)
                {
                    frameImage.sprite = frameSprite;
                    frameImage.color = Color.white; // แสดงกรอบ
                }
                else
                {
                    frameImage.color = new Color(1f, 1f, 1f, 0f); // ซ่อนกรอบถ้าไม่มี sprite
                }
            }

            // 🔥 สร้างข้อความแสดงพลังปัจจุบัน (ซ้ายล่าง)
            if (atkText == null)
            {
                // ลบ ATKDisplay เก่าทั้งหมดก่อน
                foreach (Transform child in transform)
                {
                    if (child.name == "ATKDisplay")
                    {
                        Destroy(child.gameObject);
                    }
                }

                GameObject atkObj = new GameObject("ATKDisplay");
                atkObj.transform.SetParent(transform, false);
                atkObj.transform.SetAsLastSibling();

                atkText = atkObj.AddComponent<TextMeshProUGUI>();
                atkText.fontSize = 42;
                atkText.alignment = TextAlignmentOptions.BottomLeft; // 🔥 ซ้ายล่าง
                atkText.color = Color.white;
                atkText.fontStyle = FontStyles.Bold;
                atkText.text = "0";
                atkText.raycastTarget = false;

                RectTransform atkRect = atkObj.GetComponent<RectTransform>();
                atkRect.anchorMin = Vector2.zero;
                atkRect.anchorMax = Vector2.one;
                atkRect.offsetMin = new Vector2(12, 3); // 🔥 มุมซ้ายล่าง (ต่ำลง)
                atkRect.offsetMax = new Vector2(-12, -8);
            }

            // 🔥 สร้างข้อความแสดงคอส (ขวาบน)
            if (costText == null)
            {
                // ลบ CostDisplay เก่าทั้งหมดก่อน
                foreach (Transform child in transform)
                {
                    if (child.name == "CostDisplay")
                    {
                        Destroy(child.gameObject);
                    }
                }

                GameObject costObj = new GameObject("CostDisplay");
                costObj.transform.SetParent(transform, false);
                costObj.transform.SetAsLastSibling();

                costText = costObj.AddComponent<TextMeshProUGUI>();
                costText.fontSize = 42;
                costText.alignment = TextAlignmentOptions.TopRight; // 🔥 ขวาบน
                costText.color = Color.white;
                costText.fontStyle = FontStyles.Bold;
                costText.text = "0";
                costText.raycastTarget = false;

                RectTransform costRect = costObj.GetComponent<RectTransform>();
                costRect.anchorMin = Vector2.zero;
                costRect.anchorMax = Vector2.one;
                costRect.offsetMin = new Vector2(12, 13);
                costRect.offsetMax = new Vector2(-10, 5); // 🔥 มุมขวาบน (สูงขึ้นอีก)
            }

            // 🔥 สร้างข้อความสถานะพิเศษ (กึ่งกลางด้านบน)
            if (statusText == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "StatusDisplay")
                    {
                        Destroy(child.gameObject);
                    }
                }

                GameObject statusObj = new GameObject("StatusDisplay");
                statusObj.transform.SetParent(transform, false);
                statusObj.transform.SetAsLastSibling();

                statusText = statusObj.AddComponent<TextMeshProUGUI>();
                statusText.fontSize = 24;
                statusText.alignment = TextAlignmentOptions.Top;
                statusText.color = new Color(1f, 0.9f, 0.2f, 1f);
                statusText.fontStyle = FontStyles.Bold;
                statusText.text = "";
                statusText.raycastTarget = false;
                statusText.gameObject.SetActive(false);

                RectTransform statusRect = statusObj.GetComponent<RectTransform>();
                statusRect.anchorMin = Vector2.zero;
                statusRect.anchorMax = Vector2.one;
                statusRect.offsetMin = new Vector2(8, 8);
                statusRect.offsetMax = new Vector2(-8, -8);
            }
            
            // � สร้างฐานรอง (Container) สำหรับโซนแสดงไอคอนสกิล
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
                skillIconContainer.color = new Color(0.05f, 0.05f, 0.05f, 0.90f); // สีดำโปร่งแสง
                skillIconContainer.gameObject.SetActive(false);

                // 🔒 ใช้ percentage-based anchors เพื่อให้ scale ตามขนาดการ์ด
                RectTransform containerRect = containerObj.GetComponent<RectTransform>();
                // ตำแหน่ง: ชิดซ้ายบน 6% จากซ้าย, 78%-90% จากล่าง - ขยายกว้างสำหรับ 4 ไอคอน
                containerRect.anchorMin = new Vector2(0.06f, 0.78f);
                containerRect.anchorMax = new Vector2(0.97f, 0.90f);
                containerRect.pivot = new Vector2(0, 1);
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;

                // เพิ่มขอบมนให้ดูสวยงาม (ใช้ Outline)
                var outline = containerObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.25f, 0.25f, 0.25f, 0.8f); // ขอบสีเทาดำ
                outline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            // 🔥 สร้าง 4 Skill Icon Slots ใน Container
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

                    // 🔒 แบ่ง 4 ช่อง (0-24%, 25-49%, 50-74%, 75-99%)
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

            // 🎯 สร้างไอคอน Trigger (รูป - ใต้ชื่อการ์ด ชิดซ้าย)
            if (triggerIcon == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "TriggerIcon") Destroy(child.gameObject);
                }

                // ตรวจสอบว่า skillIconContainer มีอยู่หรือไม่
                Transform parentTransform = skillIconContainer != null ? skillIconContainer.transform : transform;

                GameObject triggerIconObj = new GameObject("TriggerIcon");
                triggerIconObj.transform.SetParent(parentTransform, false);
                triggerIconObj.transform.SetAsLastSibling();

                triggerIcon = triggerIconObj.AddComponent<Image>();
                triggerIcon.raycastTarget = false;
                triggerIcon.preserveAspect = true;
                triggerIcon.gameObject.SetActive(false);

                // 🔒 ใช้ percentage anchors ใน container (2%-48% ของความกว้าง container)
                RectTransform iconRect = triggerIconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.02f, 0.1f);
                iconRect.anchorMax = new Vector2(0.48f, 0.9f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
            }
            
            // 🎯 สร้างสัญลักษณ์ Trigger (Text fallback)
            if (triggerSymbol == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "TriggerSymbol") Destroy(child.gameObject);
                }

                GameObject triggerObj = new GameObject("TriggerSymbol");
                triggerObj.transform.SetParent(transform, false);
                triggerObj.transform.SetAsLastSibling();

                triggerSymbol = triggerObj.AddComponent<TextMeshProUGUI>();
                triggerSymbol.fontSize = 28;
                triggerSymbol.alignment = TextAlignmentOptions.Left;
                triggerSymbol.color = Color.white;
                triggerSymbol.fontStyle = FontStyles.Bold;
                triggerSymbol.text = "";
                triggerSymbol.raycastTarget = false;
                triggerSymbol.gameObject.SetActive(false);
                
                // เพิ่ม outline เพื่อให้เห็นชัด
                triggerSymbol.outlineWidth = 0.2f;
                triggerSymbol.outlineColor = Color.black;

                // 🔒 ใช้ percentage anchors ในการ์ด (ชิดซ้ายบน)
                RectTransform triggerRect = triggerObj.GetComponent<RectTransform>();
                triggerRect.anchorMin = new Vector2(0.03f, 0.80f);
                triggerRect.anchorMax = new Vector2(0.30f, 0.92f);
                triggerRect.pivot = new Vector2(0, 1);
                triggerRect.offsetMin = Vector2.zero;
                triggerRect.offsetMax = Vector2.zero;
            }
            
            // ⚡ สร้างไอคอน Action (รูป - ข้าง Trigger)
            if (actionIcon == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "ActionIcon") Destroy(child.gameObject);
                }

                // ตรวจสอบว่า skillIconContainer มีอยู่หรือไม่
                Transform parentTransform = skillIconContainer != null ? skillIconContainer.transform : transform;

                GameObject actionIconObj = new GameObject("ActionIcon");
                actionIconObj.transform.SetParent(parentTransform, false);
                actionIconObj.transform.SetAsLastSibling();

                actionIcon = actionIconObj.AddComponent<Image>();
                actionIcon.raycastTarget = false;
                actionIcon.preserveAspect = true;
                actionIcon.gameObject.SetActive(false);

                // 🔒 ใช้ percentage anchors ใน container (52%-98% ของความกว้าง container)
                RectTransform iconRect = actionIconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.52f, 0.1f);
                iconRect.anchorMax = new Vector2(0.98f, 0.9f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
            }
            
            // ⚡ สร้างสัญลักษณ์ Action (Text fallback)
            if (actionSymbol == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "ActionSymbol") Destroy(child.gameObject);
                }

                GameObject actionObj = new GameObject("ActionSymbol");
                actionObj.transform.SetParent(transform, false);
                actionObj.transform.SetAsLastSibling();

                actionSymbol = actionObj.AddComponent<TextMeshProUGUI>();
                actionSymbol.fontSize = 28;
                actionSymbol.alignment = TextAlignmentOptions.Left;
                actionSymbol.color = Color.white;
                actionSymbol.fontStyle = FontStyles.Bold;
                actionSymbol.text = "";
                actionSymbol.raycastTarget = false;
                actionSymbol.gameObject.SetActive(false);
                
                actionSymbol.outlineWidth = 0.2f;
                actionSymbol.outlineColor = Color.black;

                // 🔒 ใช้ percentage anchors ในการ์ด (ข้าง Trigger)
                RectTransform actionRect = actionObj.GetComponent<RectTransform>();
                actionRect.anchorMin = new Vector2(0.28f, 0.80f);
                actionRect.anchorMax = new Vector2(0.55f, 0.92f);
                actionRect.pivot = new Vector2(0, 1);
                actionRect.offsetMin = Vector2.zero;
                actionRect.offsetMax = Vector2.zero;
            }
            
            // 🏷️ สร้าง SubCategory Badge (ด้านล่างขวา)
            if (subCategoryBadge == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.name == "SubCategoryBadge") Destroy(child.gameObject);
                }

                GameObject badgeObj = new GameObject("SubCategoryBadge");
                badgeObj.transform.SetParent(transform, false);
                badgeObj.transform.SetAsLastSibling();

                subCategoryBadge = badgeObj.AddComponent<TextMeshProUGUI>();
                subCategoryBadge.fontSize = 16;
                subCategoryBadge.alignment = TextAlignmentOptions.BottomRight;
                subCategoryBadge.color = Color.white;
                subCategoryBadge.fontStyle = FontStyles.Bold;
                subCategoryBadge.text = "";
                subCategoryBadge.raycastTarget = false;
                subCategoryBadge.gameObject.SetActive(false);
                
                subCategoryBadge.outlineWidth = 0.25f;
                subCategoryBadge.outlineColor = Color.black;

                RectTransform badgeRect = badgeObj.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(1, 0);
                badgeRect.anchorMax = new Vector2(1, 0);
                badgeRect.pivot = new Vector2(1, 0);
                badgeRect.anchoredPosition = new Vector2(-6, 32); // มุมขวาล่าง เหนือ ATK
                badgeRect.sizeDelta = new Vector2(60, 24);
            }
    }

    /// <summary>ปรับขนาดการ์ดตามตำแหน่ง (ในมือ vs บนสนาม)</summary>
    public void UpdateCardSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) return;

        if (isOnField)
        {
            // ขนาดบนสนาม: ใหญ่กว่า
            rectTransform.sizeDelta = new Vector2(200, 280);
            Debug.Log($"📏 {_cardData?.cardName}: ขนาดสนาม (200x280)");
        }
        else
        {
            // ขนาดในมือ: เล็กกว่า
            rectTransform.sizeDelta = new Vector2(140, 200);
            Debug.Log($"📏 {_cardData?.cardName}: ขนาดมือ (140x200)");
        }
    }

    // 🔥🔥🔥 ฟังก์ชัน Setup ที่หายไป อยู่ตรงนี้ครับ! 🔥🔥🔥
    public void Setup(CardData data)
    {
        _cardData = data;

        if (_cardData == null)
        {
            Debug.LogError("❌ BattleCardUI.Setup called with null CardData");
            return;
        }
        
        // 🔥 สร้าง UI elements ถ้ายังไม่มี (เพื่อให้แสดงไอคอนได้ทุก scene)
        try
        {
            CreateUIElementsIfNeeded();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ CreateUIElementsIfNeeded failed for {_cardData.cardName}: {ex}");
        }
        
        // ตั้งรูปการ์ด และบังคับให้รับ Raycast เสมอ (กันกรณี prefab ปิดไว้)
        if (artworkImage != null)
        {
            artworkImage.raycastTarget = true;
            if (data.artwork != null)
            {
                artworkImage.sprite = data.artwork;
            }
        }
        else if (data.artwork == null)
        {
            // Debug.LogError($"การ์ด {data.cardName} ไม่มีรูป Artwork!");
        }
        
        // รีเซ็ตสถานะทุกครั้งที่สร้างใหม่
        isOnField = false; 
        mulliganSelected = false;
        
        // เปิด CanvasGroup ให้คลิกได้ (กันเคส prefab ปิดไว้)
        if (canvasGroup)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1f;
        }
        
        // ตั้งชื่อ GameObject ให้หาง่ายๆ ใน Hierarchy
        gameObject.name = data.cardName;

        // 🔥 ตั้งกรอบตามความหายาก
        ApplyFrameByRarity();

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
        
        // � อัปเดตสัญลักษณ์บนการ์ด (Trigger, Action, SubCategory)
        try
        {
            UpdateSymbolDisplay();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ UpdateSymbolDisplay failed for {_cardData.cardName}: {ex}");
        }
        
        // �🎈 หยุดอนิเมชั่นลอยเพื่อไม่รบกวน HorizontalLayoutGroup
        floatTime = 0f;
        originalPosition = transform.localPosition;
        isFloating = false; // 🔥 ปิดลอยในมือ
    }

    void ApplyFrameByRarity()
    {
        if (frameImage == null || _cardData == null) return;

        Sprite rarityFrame = null;
        switch (_cardData.rarity)
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

        // ถ้าไม่มีกรอบตาม rarity ให้ใช้ frameSprite เป็นค่า fallback
        if (rarityFrame == null)
        {
            rarityFrame = frameSprite;
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

    public void SetFrameVisible(bool visible)
    {
        if (frameImage == null) return;

        if (visible)
        {
            // ใช้กรอบตาม rarity อีกครั้งเผื่อเปลี่ยนตอน face-down
            ApplyFrameByRarity();
        }
        else
        {
            frameImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    // --- Helper Functions ---
    public int GetCost()
    {
        // 🔥 ถ้าถูก ZeroStats ให้คืน 0 เสมอ
        if (modifiedCost >= 0)
        {
            return modifiedCost;
        }
        return _cardData != null ? _cardData.cost : 0;
    }

    // 🔥 เซ็ต Cost และ ATK ให้ 0 (สำหรับ ZeroStats skill - ต่อ card instance เท่านั้น)
    public void SetZeroStats()
    {
        modifiedCost = 0;
        modifiedATK = 0;
        Debug.Log($"💀 SetZeroStats: {_cardData?.cardName} → Cost=0, ATK=0 (instance only)");
    }

    public CardData GetData()
    {
        return _cardData;
    }

    // 🎯 === Sprite Mapping System ===
    
    /// <summary>แปลง EffectTrigger เป็น Sprite (คืน null ถ้าไม่มี) - ใช้จาก SkillIconAssets ก่อน fallback เป็น local</summary>
    private Sprite GetTriggerSprite(EffectTrigger trigger)
    {
        // ลองใช้จาก SkillIconAssets ก่อน (shared instance)
        if (SkillIconAssets.Instance != null)
        {
            Sprite sharedSprite = SkillIconAssets.Instance.GetTriggerSprite(trigger);
            if (sharedSprite != null) return sharedSprite;
        }
        
        // Fallback เป็น local sprites (ถ้ามี)
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
    
    /// <summary>แปลง ActionType เป็น Sprite (คืน null ถ้าไม่มี) - ใช้จาก SkillIconAssets ก่อน fallback เป็น local</summary>
    private Sprite GetActionSprite(ActionType action)
    {
        // ลองใช้จาก SkillIconAssets ก่อน (shared instance)
        if (SkillIconAssets.Instance != null)
        {
            Sprite sharedSprite = SkillIconAssets.Instance.GetActionSprite(action);
            if (sharedSprite != null) return sharedSprite;
        }
        
        // Fallback เป็น local sprites (ถ้ามี)
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

    // 🎯 === Symbol Mapping System (Text Fallback) ===
    
    /// <summary>แปลง EffectTrigger เป็นสัญลักษณ์</summary>
    private string GetTriggerSymbol(EffectTrigger trigger)
    {
        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return "▶"; // เมื่อลงสนาม (Deploy)
            case EffectTrigger.OnStrike: return "★"; // เมื่อโจมตี (Strike)
            case EffectTrigger.OnStrikeHit: return "◆"; // เมื่อโจมตีโดน (Hit)
            case EffectTrigger.OnIntercept: return "■"; // เมื่อกันการโจมตี (Intercept)
            case EffectTrigger.OnDestroyed: return "✕"; // เมื่อถูกทำลาย (desTroyed)
            case EffectTrigger.OnTurnEnd: return "●"; // สิ้นสุดเทิร์น (End)
            case EffectTrigger.Continuous: return "∞"; // ต่อเนื่อง (Continuous)
            default: return "";
        }
    }
    
    /// <summary>แปลง EffectTrigger เป็นสีประจำ</summary>
    private Color GetTriggerColor(EffectTrigger trigger)
    {
        switch (trigger)
        {
            case EffectTrigger.OnDeploy: return new Color(0.2f, 0.8f, 0.2f); // เขียว
            case EffectTrigger.OnStrike: return new Color(1f, 0.4f, 0.2f); // ส้มแดง
            case EffectTrigger.OnStrikeHit: return new Color(1f, 0.2f, 0.2f); // แดง
            case EffectTrigger.OnIntercept: return new Color(0.3f, 0.6f, 1f); // ฟ้า
            case EffectTrigger.OnDestroyed: return new Color(0.6f, 0.3f, 0.8f); // ม่วง
            case EffectTrigger.OnTurnEnd: return new Color(1f, 0.9f, 0.3f); // เหลือง
            case EffectTrigger.Continuous: return new Color(0f, 1f, 1f); // ฟ้าอมเขียว
            default: return Color.white;
        }
    }
    
    /// <summary>แปลง ActionType เป็นสัญลักษณ์</summary>
    private string GetActionSymbol(ActionType action)
    {
        switch (action)
        {
            // การทำลาย/ควบคุม
            case ActionType.Destroy: return "✕"; // ทำลาย
            case ActionType.ControlEquip: return "♠"; // ควบคุม Equip
            case ActionType.RemoveCategory: return "○"; // ลบ Category
            case ActionType.ZeroStats: return "0"; // ลดสถานะเป็น 0
            
            // ปิด/ห้าม
            case ActionType.DisableAttack: return "⊘"; // ห้ามโจมตี
            case ActionType.DisableAbility: return "⊗"; // ปิดสกิล
            case ActionType.DisableIntercept: return "⊖"; // ห้ามกันการโจมตี
            
            // ดู/เปิดเผย
            case ActionType.RevealHand: return "◎"; // ดูมือ 1 ใบ
            case ActionType.RevealHandMultiple: return "◉"; // ดูมือหลายใบ
            case ActionType.PeekDiscardTopDeck: return "▽"; // ดูและทิ้งบนเด็ค
            
            // ทิ้ง/เด็ค
            case ActionType.DiscardDeck: return "▼"; // Mill จากเด็ค
            case ActionType.ForceChooseDiscard: return "▿"; // บังคับเลือกทิ้ง
            case ActionType.MarkInterceptMillDeck: return "▾"; // Mill เด็คเมื่อ Intercept
            
            // เรียก/สร้าง
            case ActionType.SummonToken: return "+"; // เรียก Token
            case ActionType.DrawCard: return "△"; // จั่วการ์ด
            case ActionType.ReturnEquipFromGraveyard: return "↺"; // คืน Equip จากสุสาน
            
            // การโจมตี
            case ActionType.Rush: return "»"; // โจมตีได้ทันที
            case ActionType.DoubleStrike: return "★★"; // โจมตี 2 ครั้ง
            case ActionType.GraveyardATK: return "†"; // พลังจากสุสาน
            case ActionType.BypassIntercept: return "→"; // ข้ามการกัน
            
            // การกัน
            case ActionType.ForceIntercept: return "↔"; // บังคับกัน
            case ActionType.InterceptAlwaysTypeMatch: return "◈"; // กันได้เสมอ (type match)
            
            // รักษา
            case ActionType.HealHP: return "♥"; // รักษา HP
            case ActionType.HealOnMonsterSummoned: return "♡"; // รักษาเมื่อ Summon Monster
            
            // ปรับสถานะ
            case ActionType.ModifyStat: return "±"; // ปรับสถานะ
            
            // Protection
            case ActionType.ProtectDrawnCards: return "■△"; // ปกป้องการ์ดที่จั่ว
            case ActionType.ProtectRevealHandMultiple: return "■◎"; // ปกป้องจากการดูมือ
            case ActionType.ProtectForceInterceptEquip: return "■↔"; // ปกป้องจากบังคับกัน
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy: return "■✕"; // ปกป้อง Equip ตัวเองจากสกิลทำลาย
            
            default: return "◆";
        }
    }
    
    /// <summary>แปลง ActionType เป็นสีประจำ</summary>
    private Color GetActionColor(ActionType action)
    {
        switch (action)
        {
            // 🔥 การทำลาย/ควบคุม - สีแดง
            case ActionType.Destroy:
            case ActionType.ZeroStats:
            case ActionType.RemoveCategory:
                return new Color(1f, 0.3f, 0.3f); // แดง
            
            // 🛡️ ปิด/ห้าม - สีส้ม
            case ActionType.DisableAttack:
            case ActionType.DisableAbility:
            case ActionType.DisableIntercept:
                return new Color(1f, 0.6f, 0.2f); // ส้ม
            
            // 👁️ ดู/เปิดเผย - สีม่วง
            case ActionType.RevealHand:
            case ActionType.RevealHandMultiple:
            case ActionType.PeekDiscardTopDeck:
                return new Color(0.8f, 0.4f, 1f); // ม่วง
            
            // 📤 ทิ้ง/เด็ค - สีน้ำตาล
            case ActionType.DiscardDeck:
            case ActionType.ForceChooseDiscard:
            case ActionType.MarkInterceptMillDeck:
                return new Color(0.7f, 0.5f, 0.3f); // น้ำตาล
            
            // ➕ เรียก/สร้าง - สีเขียว
            case ActionType.SummonToken:
            case ActionType.DrawCard:
            case ActionType.ReturnEquipFromGraveyard:
                return new Color(0.3f, 0.9f, 0.3f); // เขียว
            
            // ⚔️ การโจมตี - สีฟ้าเข้ม
            case ActionType.Rush:
            case ActionType.DoubleStrike:
            case ActionType.GraveyardATK:
            case ActionType.BypassIntercept:
                return new Color(0.3f, 0.7f, 1f); // ฟ้า
            
            // 🛡️ การกัน - สีเขียวอมฟ้า
            case ActionType.ForceIntercept:
            case ActionType.InterceptAlwaysTypeMatch:
                return new Color(0.2f, 0.8f, 0.8f); // เขียวอมฟ้า
            
            // ❤️ รักษา - สีชมพู
            case ActionType.HealHP:
            case ActionType.HealOnMonsterSummoned:
                return new Color(1f, 0.5f, 0.7f); // ชมพู
            
            // ±️ ปรับสถานะ - สีเหลือง
            case ActionType.ModifyStat:
                return new Color(1f, 0.9f, 0.3f); // เหลือง
            
            // 🎮 ควบคุม - สีม่วงเข้ม
            case ActionType.ControlEquip:
                return new Color(0.6f, 0.2f, 0.8f); // ม่วงเข้ม
            
            // 🛡️ Protection - สีทอง
            case ActionType.ProtectDrawnCards:
            case ActionType.ProtectRevealHandMultiple:
            case ActionType.ProtectForceInterceptEquip:
            case ActionType.ProtectOtherOwnEquipFromAbilityDestroy:
                return new Color(1f, 0.85f, 0.4f); // ทอง
            
            default: return Color.white;
        }
    }
    
    /// <summary>แปลง SubCategory เป็นตัวย่อ</summary>
    private string GetSubCategoryBadgeText(SubCategory sub)
    {
        switch (sub)
        {
            case SubCategory.IDOR: return "IDOR";
            case SubCategory.PathTraversal: return "PATH";
            case SubCategory.MFLAC: return "MFLAC";
            case SubCategory.InsecureTransit: return "TRANS";
            case SubCategory.InsecureRest: return "REST";
            case SubCategory.WeakHash: return "HASH";
            case SubCategory.NoSalt: return "SALT";
            case SubCategory.BadKey: return "KEY";
            case SubCategory.SQLi: return "SQLi";
            case SubCategory.XSS: return "XSS";
            case SubCategory.OSCommand: return "CMD";
            case SubCategory.XXE: return "XXE";
            default: return "";
        }
    }
    
    /// <summary>แปลง SubCategory เป็นสี</summary>
    private Color GetSubCategoryColor(SubCategory sub)
    {
        switch (sub)
        {
            case SubCategory.SQLi: return new Color(1f, 0.5f, 0f); // ส้ม
            case SubCategory.XSS: return new Color(1f, 0.8f, 0f); // เหลืองทอง
            case SubCategory.OSCommand: return new Color(0.8f, 0.2f, 0.2f); // แดงเข้ม
            case SubCategory.XXE: return new Color(0.9f, 0.4f, 0.6f); // ชมพู
            case SubCategory.IDOR: return new Color(0.4f, 0.7f, 1f); // ฟ้าอ่อน
            case SubCategory.PathTraversal: return new Color(0.6f, 0.8f, 0.4f); // เขียวอ่อน
            case SubCategory.WeakHash: 
            case SubCategory.NoSalt: 
            case SubCategory.BadKey: return new Color(0.7f, 0.5f, 0.9f); // ม่วงอ่อน
            default: return new Color(0.8f, 0.8f, 0.8f); // เทา
        }
    }
    
    /// <summary>อัปเดตการแสดงสัญลักษณ์บนการ์ด (Trigger, Action, SubCategory)</summary>
    public void UpdateSymbolDisplay()
    {
        if (_cardData == null) return;

        var effects = _cardData.effects ?? new System.Collections.Generic.List<CardEffect>();
        
        int iconIndex = 0; // ตำแหน่ง slot ปัจจุบัน (0-3)
        bool showAnyIcon = false; // เช็คว่ามีไอคอนใดแสดงอยู่หรือไม่

        // 🔥 ซ่อน icons เก่าก่อน (triggerIcon/actionIcon สำหรับ backward compat)
        if (triggerIcon != null) triggerIcon.gameObject.SetActive(false);
        if (actionIcon != null) actionIcon.gameObject.SetActive(false);
        if (triggerSymbol != null) triggerSymbol.gameObject.SetActive(false);
        if (actionSymbol != null) actionSymbol.gameObject.SetActive(false);
        
        // 🔥 ซ่อน skillIcons ทั้งหมดก่อน
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] != null) skillIcons[i].gameObject.SetActive(false);
        }
        
        // 🎯 รวบรวม sprites ทั้งหมดที่ต้องแสดง (Trigger และ Action แยกกัน สูงสุด 4 ตัว)
        var spritesToShow = new System.Collections.Generic.List<Sprite>();
        
        // รวบรวม Trigger sprites ทั้งหมด
        foreach (var effect in effects)
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
        
        // รวบรวม Action sprites ทั้งหมด
        foreach (var effect in effects)
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
        
        // แสดง sprites ใน slots
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
        
        // 🎨 แสดง/ซ่อน container และปรับขนาดตามจำนวนไอคอน
        if (skillIconContainer != null)
        {
            if (showAnyIcon && iconIndex > 0)
            {
                skillIconContainer.gameObject.SetActive(true);
                
                // 🔥 ปรับขนาด container ตามจำนวนไอคอน (dynamic width)
                RectTransform containerRect = skillIconContainer.GetComponent<RectTransform>();
                // ความกว้างต่อไอคอน ≈ 22% (รวม padding)
                float widthPerIcon = 0.22f;
                float endX = 0.06f + (iconIndex * widthPerIcon);
                endX = Mathf.Min(endX, 0.97f); // จำกัดไม่ให้เกิน 97%
                
                containerRect.anchorMin = new Vector2(0.06f, 0.78f);
                containerRect.anchorMax = new Vector2(endX, 0.90f);
                
                // 🔥 ปรับ position ของ icons ให้กระจายเต็ม container
                float iconWidth = 1f / iconIndex; // แบ่งเท่าๆ กัน
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
        
        // 🏷️ แสดง SubCategory Badge
        if (subCategoryBadge != null)
        {
            if (_cardData.subCategory != SubCategory.General)
            {
                string badgeText = GetSubCategoryBadgeText(_cardData.subCategory);
                subCategoryBadge.text = badgeText;
                subCategoryBadge.color = GetSubCategoryColor(_cardData.subCategory);
                subCategoryBadge.gameObject.SetActive(!string.IsNullOrEmpty(badgeText));
            }
            else
            {
                subCategoryBadge.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>ซ่อนสัญลักษณ์ทั้งหมด (ใช้สำหรับ Card Back)</summary>
    public void HideSymbols()
    {
        if (skillIconContainer != null) skillIconContainer.gameObject.SetActive(false);
        // 🔥 ซ่อน skillIcons ทั้ง 4 ช่อง
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] != null) skillIcons[i].gameObject.SetActive(false);
        }
        if (triggerIcon != null) triggerIcon.gameObject.SetActive(false);
        if (actionIcon != null) actionIcon.gameObject.SetActive(false);
        if (triggerSymbol != null) triggerSymbol.gameObject.SetActive(false);
        if (actionSymbol != null) actionSymbol.gameObject.SetActive(false);
        if (subCategoryBadge != null) subCategoryBadge.gameObject.SetActive(false);
    }
    
    /// <summary>แสดงสัญลักษณ์อีกครั้ง (เมื่อเปิดหน้าการ์ด)</summary>
    public void ShowSymbols()
    {
        UpdateSymbolDisplay();
    }

    // 🔥 ซ่อนข้อมูลการ์ด (Cost และ ATK) ใช้สำหรับ Card Back ของบอท
    public void HideCardInfo()
    {
        // 🔥 ซ่อน child objects ทั้งหมดที่ชื่อ ATKDisplay/CostDisplay (รวมถึง prefab)
        foreach (Transform child in transform)
        {
            if (child.name == "ATKDisplay" || child.name == "CostDisplay")
            {
                child.gameObject.SetActive(false);
            }
        }
        
        if (costText != null)
        {
            costText.text = ""; // เซ็ตเป็นเปล่าเพื่อให้แน่ใจว่าไม่แสดง 0
            costText.gameObject.SetActive(false);
        }
        if (atkText != null)
        {
            atkText.text = ""; // เซ็ตเป็นเปล่าเพื่อให้แน่ใจว่าไม่แสดง 0
            atkText.gameObject.SetActive(false);
        }
        
        // 🎯 ซ่อนสัญลักษณ์ด้วย
        HideSymbols();
    }

    // 🔥 แสดงข้อมูลการ์ด (Cost และ ATK) อีกครั้ง
    public void ShowCardInfo()
    {
        if (costText != null)
        {
            costText.gameObject.SetActive(true);
        }
        if (atkText != null && _cardData != null && (_cardData.type == CardType.Monster || _cardData.type == CardType.Token))
        {
            atkText.gameObject.SetActive(true);
        }
        
        // 🎯 แสดงสัญลักษณ์ด้วย
        ShowSymbols();
    }

    bool IsInsideHandRevealPreview()
    {
        if (BattleManager.Instance == null) return false;

        Transform revealRoot = BattleManager.Instance.handRevealListRoot;
        return revealRoot != null && transform.IsChildOf(revealRoot);
    }

    // --- 🖱️ ส่วนการลาก (Drag System) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsInsideHandRevealPreview())
            return;

        // ห้ามลากการ์ดฝั่งบอทในมือ
        if (BattleManager.Instance != null && transform.parent == BattleManager.Instance.enemyHandArea)
            return;

        // ถ้าลงสนามแล้ว ห้ามลากย้าย (กฏ Cyber Shield Quest: ลงแล้วห้ามย้ายช่อง)
        if (isOnField) return; 

        // ✅ อนุญาตให้ลากได้ในโหมด Mulligan
        // (ไม่ต้องเช็คเพราะเราจะให้ลากได้ทั้งในมือและใน Mulligan phase)

        // 🎈 หยุดอนิเมชั่นลอยระหว่างลาก
        isFloating = false;
        originalPosition = transform.localPosition;

        // 1. จำพ่อเดิมไว้ (HandArea หรือ MulliganSlot) เผื่อวางผิดจะได้เด้งกลับถูก
        parentAfterDrag = transform.parent;
        
        // 2. ย้ายไปอยู่ level นอกสุด เพื่อให้การ์ดลอยเหนือทุกอย่าง
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling(); // บังทุกอย่าง
        
        // 3. ปิดการมองเห็นของเมาส์ เพื่อให้เมาส์ทะลุตัวการ์ดไปเจอ Slot ข้างหลัง
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsInsideHandRevealPreview())
            return;

        if (BattleManager.Instance != null && transform.parent == BattleManager.Instance.enemyHandArea)
            return;
        if (isOnField) return;
        // ขยับการ์ดตามเมาส์
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsInsideHandRevealPreview())
            return;

        if (BattleManager.Instance != null && transform.parent == BattleManager.Instance.enemyHandArea)
            return;

        // เปิดให้ raycast อีกครั้งเสมอ
        // (กรณีลากลงช่องแล้วถูก summon ทันที isOnField จะเป็น true ในเฟรมนี้)
        canvasGroup.blocksRaycasts = true;

        if (isOnField) return;

        // รอ 1 เฟรมให้ OnDrop ทำงานก่อนค่อยตัดสินใจตำแหน่งสุดท้าย
        StartCoroutine(HandleEndDragAfterDrop());
    }

    IEnumerator HandleEndDragAfterDrop()
    {
        // รอ 1 เฟรมเผื่อ OnDrop ใน CardSlot จะ re-parent ให้เรียบร้อย
        yield return null;

        // ถ้ายังลอยอยู่ที่ root แสดงว่าไม่ได้ลงช่อง → เด้งกลับที่เดิม
        if (transform.parent == transform.root)
        {
            transform.SetParent(parentAfterDrag);

            // ถ้ากลับมือ ปล่อยให้ Layout จัดตำแหน่ง
            if (parentAfterDrag != null && (parentAfterDrag.name == "HandArea" || parentAfterDrag.name == "handArea"))
            {
                // ไม่ snap
            }
            else
            {
                transform.localPosition = Vector3.zero; // Snap กลับช่อง
            }
        }
        else
        {
            // ถูกวางลงช่องแล้ว → snap กลางช่อง ยกเว้นอยู่ในมือ
            if (transform.parent != null && (transform.parent.name == "HandArea" || transform.parent.name == "handArea"))
            {
                // ไม่ snap ในมือ
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }

        // หยุดอนิเมชั่นลอยถ้ายังอยู่ในมือ
        if (!isOnField)
        {
            floatTime = 0f;
            originalPosition = transform.localPosition;
            isFloating = false;
        }
    }

    // --- Interaction (คลิก / เมาส์ชี้) ---

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_cardData == null)
        {
            Debug.LogWarning($"❌ OnPointerDown: _cardData is NULL");
            return;
        }

        if (IsInsideHandRevealPreview())
        {
            return;
        }

        bool isPrimary = eventData.button == PointerEventData.InputButton.Left;
        bool isSecondary = eventData.button == PointerEventData.InputButton.Right;
        
        bool isRevealed = BattleManager.Instance != null && BattleManager.Instance.IsCardRevealed(_cardData);
        Debug.Log($"🖱️ Click on {_cardData.cardName}: isPrimary={isPrimary}, isRevealed={isRevealed}, parent={transform.parent?.name}");

        // 👁️ หากการ์ดนี้ reveal แล้ว ให้ดูรายละเอียดได้
        if (isPrimary && BattleManager.Instance != null && BattleManager.Instance.IsCardRevealed(_cardData))
        {
            Debug.Log($"✅ Revealed card clicked, opening detail");
            if (BattleManager.Instance.cardDetailView != null)
            {
                if (BattleManager.Instance.cardDetailView.IsShowingCard(_cardData))
                {
                    BattleManager.Instance.cardDetailView.Close();
                    Debug.Log($"❌ Closed detail");
                }
                else
                {
                    BattleManager.Instance.cardDetailView.Open(_cardData);
                    Debug.Log($"👁️ Opened detail");
                }
            }
            else
            {
                Debug.LogError("❌ cardDetailView is NULL!");
            }
            return;
        }

        // ห้ามโต้ตอบการ์ดที่อยู่ในมือบอท (ถ้ายังไม่ reveal)
        if (BattleManager.Instance != null && transform.parent == BattleManager.Instance.enemyHandArea)
        {
            Debug.Log($"⛔ Bot hand card (not revealed): {_cardData.cardName}");
            return;
        }

        // 🔥 คลิกซ้าย = เปิดรายละเอียดการ์ดเท่านั้น (ถ้าคลิกซ้ำให้ปิด)
        if (isPrimary)
        {
            if (BattleManager.Instance != null && BattleManager.Instance.cardDetailView != null)
            {
                // ถ้ากำลังแสดงการ์ดนี้อยู่แล้ว → ปิด
                if (BattleManager.Instance.cardDetailView.IsShowingCard(_cardData))
                {
                    BattleManager.Instance.cardDetailView.Close();
                    Debug.Log($"❌ ปิด detail: {_cardData.cardName}");
                }
                else
                {
                    // ถ้ายังไม่เปิดหรือเปิดการ์ดอื่นอยู่ → เปิดการ์ดนี้
                    BattleManager.Instance.cardDetailView.Open(_cardData);
                    Debug.Log($"📋 เปิด detail: {_cardData.cardName}");
                }
            }
            else
            {
                Debug.LogWarning("CardDetailView not found in BattleManager");
            }
            return; // หยุดที่นี่ ไม่ทำแอ็กชันอื่น
        }

        // ใช้คลิกขวาเท่านั้นสำหรับการกระทำหลัก (เล่น/โจมตี/ป้องกัน)
        if (!isSecondary) return;

        // === โหมด Mulligan ===
        if (BattleManager.Instance != null && BattleManager.Instance.IsMulliganPhase())
        {
            bool isInSwapSlot = false;

            if (BattleManager.Instance.mulliganSwapSlots != null)
            {
                foreach (var swapSlot in BattleManager.Instance.mulliganSwapSlots)
                {
                    if (swapSlot == transform.parent)
                    {
                        isInSwapSlot = true;
                        break;
                    }
                }
            }

            if (isInSwapSlot)
            {
                Transform freeMulliganSlot = BattleManager.Instance.GetFreeMulliganSlot();
                if (freeMulliganSlot != null)
                {
                    transform.SetParent(freeMulliganSlot);
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                    Debug.Log($"✅ ย้าย {name} กลับไป mulligan slot (ยกเลิกเลือก)");
                }
                else
                {
                    Debug.Log("⚠️ ไม่มี mulligan slot ว่าง");
                }
            }
            else
            {
                bool moved = BattleManager.Instance.TryMoveCardToSwapSlot(this);
                if (moved)
                {
                    Debug.Log($"✅ เลือก {name} เพื่อเปลี่ยน");
                }
                else
                {
                    Debug.Log("⚠️ ช่องเปลี่ยนเต็มแล้ว (4/4)");
                }
            }
            return; // ไม่ให้ทำอื่นในโหมด Mulligan
        }

        // === โหมดปกติ ===

        // 1. เล่นการ์ดจากมือ
        if (!isOnField && BattleManager.Instance != null && BattleManager.Instance.state == BattleState.PLAYERTURN)
        {
            BattleManager.Instance.OnCardPlayed(this);
            Debug.Log($"▶️ เล่น {_cardData.cardName}");
            return;
        }

        // 2. อยู่บนสนาม
        if (isOnField && BattleManager.Instance != null)
        {
            if (BattleManager.Instance.state == BattleState.PLAYERTURN
                && (_cardData.type == CardType.Monster || _cardData.type == CardType.Token))
            {
                if (CanAttackNow())
                {
                    BattleManager.Instance.OnPlayerAttack(this);
                    Debug.Log($"⚔️ โจมตี: {_cardData.cardName}");
                }
                else
                {
                    Debug.Log("⚠️ การ์ดนี้โจมตีครบแล้ว");
                }
            }
            else if (BattleManager.Instance.state == BattleState.DEFENDER_CHOICE)
            {
                Debug.Log($"🖱️ Clicked on {_cardData.cardName} during DEFENDER_CHOICE");
                
                if (_cardData.type == CardType.EquipSpell)
                {
                    // 🔥 เช็คว่าโล่นี้สามารถกันได้หรือไม่ (ถ้าถูกข้าม = กันไม่ได้)
                    var currentAttacker = BattleManager.Instance.GetCurrentAttacker();
                    var currentAttackerData = BattleManager.Instance.GetCurrentAttackerData();
                    
                    Debug.Log($"→ Current Attacker: {(currentAttacker != null ? currentAttacker.GetData().cardName : "NULL")}");
                    Debug.Log($"→ Has BypassIntercept: {(currentAttacker != null ? currentAttacker.canBypassIntercept.ToString() : "N/A")}");
                    
                    if (currentAttacker != null && currentAttackerData != null)
                    {
                        // เช็คว่าผู้โจมตีสามารถข้ามโล่นี้ได้หรือไม่
                        if (currentAttacker.canBypassIntercept)
                        {
                            bool isBypassed = BattleManager.Instance.CanBypassShield(currentAttacker, this);
                            Debug.Log($"→ Is {_cardData.cardName} Bypassed? {isBypassed}");
                            if (isBypassed)
                            {
                                Debug.Log($"⚠️ {_cardData.cardName} ถูกข้าม (Bypassed) - ไม่สามารถกันได้!");
                                return; // ไม่ให้เลือกโล่นี้
                            }
                        }
                        
                        Debug.Log($"✅ {_cardData.cardName} สามารถกันได้ - เรียก OnPlayerSelectBlocker");
                        BattleManager.Instance.OnPlayerSelectBlocker(this);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ ไม่พบข้อมูลการโจมตี!");
                        BattleManager.Instance.OnPlayerSkipBlock();
                    }
                }
                else
                {
                    BattleManager.Instance.OnPlayerSkipBlock();
                    Debug.Log("⚠️ ไม่ได้ใช้กัน (ไม่ใช่ EquipSpell)");
                }
            }
        }
    }

    public void ToggleMulliganSelect()
    {
        // ✅ ไม่ต้อง toggle สีแล้ว (ใช้การลากไปช่องแทน)
        // ฟังก์ชันนี้ค้างไว้เพื่อความเข้ากันได้เท่านั้น
    }

    public bool IsSelectedForMulligan() => mulliganSelected;

    public void SetMulliganSelect(bool val)
    {
        mulliganSelected = val;
        UpdateMulliganHighlight();
    }

    void UpdateMulliganHighlight()
    {
        if (artworkImage && !hasLostCategory) // 🟣 ห้ามทับสีม่วงถ้าสูญเสีย category
        {
            artworkImage.color = mulliganSelected ? Color.yellow : Color.white;
        }
    }
    
    /// <summary>ฮาไลท์การ์ดสำหรับการเลือกเป้าหมาย</summary>
    public void SetHighlight(bool highlight)
    {
        isManualHighlight = highlight; // บอก Update() ว่าห้ามแตะสี
        if (artworkImage && !hasLostCategory) // 🟣 ห้ามทับสีม่วงถ้าสูญเสีย category
        {
            // ฮาไลท์ = สีเหลือง, ปกติ = สีขาว
            artworkImage.color = highlight ? new Color(1f, 1f, 0.5f) : Color.white;
            Debug.Log($"🎯 SetHighlight({highlight}): {_cardData?.cardName} -> {(highlight ? "Yellow" : "White")}");
        }
    }
    
        // ฟังก์ชันรีเซ็ตตอนเริ่มเทิร์น (ให้โจมตีใหม่ได้)
    public void ResetAttackState()
    {
        hasAttacked = false;
        attacksThisTurn = 0; // รีเซ็ตจำนวนครั้งที่โจมตี
        isManualHighlight = false; // รีเซ็ต manual highlight
        cannotIntercept = false; // 🚫 รีเซ็ตการปิดการกัน ให้กันได้ใหม่
        
        // 🟣 ลดจำนวนเทิร์นที่เหลือของ category loss (ถ้ามี)
        ProcessCategoryLossDuration();
        
        // 🎮 ลดจำนวนเทิร์นที่เหลือของการควบคุม (ถ้ามี)
        ProcessControlDuration();
        
        // เปลี่ยนสีกลับเป็นปกติ และตรวจสอบให้แสดงหน้าการ์ด
        if(artworkImage && !hasLostCategory) // 🟣 ห้ามทับสีม่วงถ้าสูญเสีย category
        {
            artworkImage.color = Color.white;
            // 🔥 แก้: ตรวจสอบให้แน่ใจว่าแสดงหน้าการ์ด
            if (_cardData != null && _cardData.artwork != null)
            {
                artworkImage.sprite = _cardData.artwork;
            }
        }
    }
    
    /// <summary>
    /// ลดจำนวนเทิร์นที่เหลือของ category loss และคืน category เมื่อหมดเวลา
    /// เรียกตอนเริ่มเทิร์นของการ์ด
    /// </summary>
    public void ProcessCategoryLossDuration()
    {
        // ถ้าไม่ได้สูญเสีย category หรือเป็นแบบถาวร (-1) ไม่ต้องทำอะไร
        if (!hasLostCategory || categoryLostTurnsRemaining == -1)
        {
            return;
        }
        
        // ลดจำนวนเทิร์น
        categoryLostTurnsRemaining--;
        
        Debug.Log($"[ProcessCategoryLossDuration] {(_cardData != null ? _cardData.cardName : "Unknown")} - Turns remaining: {categoryLostTurnsRemaining}");
        
        // ถ้าหมดเวลา (categoryLostTurnsRemaining == 0) ให้คืน category
        if (categoryLostTurnsRemaining <= 0)
        {
            hasLostCategory = false;
            categoryLostTurnsRemaining = 0;
            
            // คืนสีเป็นปกติ (ขาว)
            if (artworkImage != null)
            {
                artworkImage.color = Color.white;
            }
            
            var img = GetComponent<Image>();
            if (img != null && img != artworkImage)
            {
                img.color = Color.white;
            }
            
            Debug.Log($"✅ [RestoreCategory] {(_cardData != null ? _cardData.cardName : "Unknown")} category restored!");
        }
    }

    /// <summary>
    /// ลดจำนวนเทิร์นที่เหลือของการควบคุม (Control) และคืนการ์ดเมื่อหมดเวลา
    /// เรียกตอนสิ้นเทิร์นของการ์ด OwnerEquipSpell
    /// </summary>
    public void ProcessControlDuration()
    {
        // ถ้าไม่ถูกควบคุมหรือเป็นแบบถาวร (-1) ไม่ต้องทำอะไร
        if (!isControlled || controlledTurnsRemaining == -1)
        {
            return;
        }
        
        // ลดจำนวนเทิร์น
        controlledTurnsRemaining--;
        
        Debug.Log($"[ProcessControlDuration] {(_cardData != null ? _cardData.cardName : "Unknown")} - Turns remaining: {controlledTurnsRemaining}");
        
        // ถ้าหมดเวลา (controlledTurnsRemaining == 0) ให้คืนการ์ดกลับ
        if (controlledTurnsRemaining <= 0)
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ReturnControlledEquip(this);
            }
        }
    }

    /// <summary>
    /// ตรวจสอบว่าการ์ดสามารถเล่นได้หรือไม่ (มี PP พอ และมีเป้าหมายที่ถูกต้อง)
    /// </summary>
    public bool CanPlayCard()
    {
        if (_cardData == null || BattleManager.Instance == null) return false;

        // ตรวจสอบ PP พอหรือไม่ (ใช้ GetCost() เพื่อให้ ZeroStats work)
        if (BattleManager.Instance.currentPP < GetCost()) return false;

        // ตรวจสอบว่ามีเป้าหมายที่ถูกต้องสำหรับทุก OnDeploy effect (เฉพาะ Spell/EquipSpell)
        if (_cardData.type == CardType.Spell || _cardData.type == CardType.EquipSpell)
        {
            if (!HasAllValidTargets()) return false;
        }

        return true;
    }

    /// <summary>
    /// ตรวจสอบว่า OnDeploy effects ทั้งหมดของการ์ดนี้มีเป้าหมายที่ถูกต้อง
    /// </summary>
    private bool HasAllValidTargets()
    {
        if (_cardData == null || _cardData.effects == null) return true;

        foreach (var effect in _cardData.effects)
        {
            // ตรวจสอบเฉพาะ OnDeploy (เอฟเฟกต์ที่ใช้ทันทีเมื่อเล่น)
            if (effect.trigger != EffectTrigger.OnDeploy) continue;

            if (!HasValidTargetForEffect(effect)) return false;
        }
        return true;
    }

    /// <summary>
    /// ตรวจสอบว่า effect นี้มีเป้าหมายที่ถูกต้องในสนามปัจจุบัน
    /// </summary>
    private bool HasValidTargetForEffect(CardEffect effect)
    {
        var bm = BattleManager.Instance;

        switch (effect.targetType)
        {
            case TargetType.Self:
                // ReturnEquipFromGraveyard ต้องมีการ์ดในสุสานที่ตรง targetCardTypeFilter
                if (effect.action == ActionType.ReturnEquipFromGraveyard)
                    return bm.HasMatchingCardInPlayerGraveyard(effect.targetCardTypeFilter);
                return true; // Self effects อื่นๆ ใช้ได้เสมอ

            case TargetType.EnemyMonster:
                return HasCardInSlots(bm.enemyMonsterSlots, effect);

            case TargetType.EnemyEquip:
                return HasCardInSlots(bm.enemyEquipSlots, effect);

            case TargetType.EnemyHand:
                return bm.enemyHandArea != null && bm.enemyHandArea.childCount > 0;

            case TargetType.EnemyDeck:
                return bm.GetEnemyDeckCount() > 0;

            case TargetType.EnemyPlayer:
            case TargetType.AllGlobal:
                return true; // ใช้ได้เสมอ ไม่ต้องการเป้าหมายเฉพาะ

            default:
                return true;
        }
    }

    /// <summary>
    /// ตรวจสอบว่าใน slots มีการ์ดที่ตรงกับเงื่อนไขของ effect หรือไม่
    /// </summary>
    private bool HasCardInSlots(Transform[] slots, CardEffect effect)
    {
        if (slots == null) return false;

        foreach (var slot in slots)
        {
            if (slot == null || slot.childCount == 0) continue;

            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (card == null || !card.isOnField) continue;

            var data = card.GetData();
            if (data == null) continue;

            // --- กรองตาม MainCategory ---
            if (effect.targetMainCat != MainCategory.General && data.mainCategory != effect.targetMainCat)
                continue;

            // --- กรองตาม SubCategory (ใช้ GetModifiedSubCategory เพื่อรองรับ RemoveCategory) ---
            if (effect.targetSubCat != SubCategory.General && card.GetModifiedSubCategory() != effect.targetSubCat)
                continue;

            // --- กรองตาม Cost สูงสุด ---
            if (effect.targetMaxCost > 0 && data.cost > effect.targetMaxCost)
                continue;

            // --- กรองตาม ชื่อการ์ดเจาะจง ---
            if (!string.IsNullOrEmpty(effect.targetCardNameFilter) && data.cardName != effect.targetCardNameFilter)
                continue;

            // --- กรองตาม Exclude filter ---
            if (effect.useExcludeFilter)
            {
                if (effect.excludeMainCat != MainCategory.General && data.mainCategory == effect.excludeMainCat)
                    continue;
                if (effect.excludeSubCat != SubCategory.General && card.GetModifiedSubCategory() == effect.excludeSubCat)
                    continue;
            }

            return true; // พบเป้าหมายที่ถูกต้องอย่างน้อย 1 ใบ
        }
        return false;
    }

    /// <summary>
    /// เล่น bounce animation ให้การ์ดขยับๆเด้งๆบน
    /// </summary>
    public void PlayBounceAnimation(float duration = 1.2f, float bounceHeight = 20f)
    {
        // หยุด animation ที่ทำงานอยู่ก่อน
        StopBounceAnimation();
        bounceAnimationCoroutine = StartCoroutine(BounceAnimationRoutine(duration, bounceHeight));
    }

    public void StopBounceAnimation()
    {
        if (bounceAnimationCoroutine != null)
        {
            StopCoroutine(bounceAnimationCoroutine);
            bounceAnimationCoroutine = null;
        }
    }

    IEnumerator BounceAnimationRoutine(float duration = 1.2f, float bounceHeight = 20f)
    {
        Vector3 originalScale = transform.localScale;
        float minScale = 0.95f; // ขนาดเล็กสุด
        float maxScale = 1.08f; // ขนาดใหญ่สุด
        float elapsedTime = 0f;
        
        while (true)
        {
            elapsedTime += Time.deltaTime;
            
            // เล่น bounce animation แบบ loop
            float cycle = elapsedTime % duration;
            float progress = cycle / duration;
            
            // Ease-in-out cubic สำหรับ bounce effect ที่เรียบ
            // ใช้ sin wave เพื่อให้ลอกขึ้นลง 0 ~ 1 ~ 0
            float normalizedBounce = Mathf.Sin(progress * Mathf.PI);
            
            // Interpolate ระหว่าง minScale และ maxScale
            float currentScale = Mathf.Lerp(minScale, maxScale, normalizedBounce);
            
            transform.localScale = originalScale * currentScale;
            
            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ขยายเฉพาะตอนอยู่ในมือ เพื่อความสวยงาม
        // if (!isOnField) 
        // {
        //     transform.localScale = Vector3.one * 1.2f;
        //     transform.SetAsLastSibling();
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // transform.localScale = Vector3.one; // คืนขนาดเดิม
    }
}