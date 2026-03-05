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
                // ใช้ GetModifiedATK() เพื่อแสดงพลังปัจจุบันที่คำนึงถึงสกิลทั้งหมด
                int currentATK = GetModifiedATK(isPlayerAttack: true);
                
                // ถ้ามี GraveyardATK ให้แสดงเป็นสีเขียว
                var graveyardEffect = _cardData.effects.FirstOrDefault(e => e.trigger == EffectTrigger.OnStrike && e.action == ActionType.GraveyardATK);
                if (graveyardEffect.action == ActionType.GraveyardATK && currentATK > _cardData.atk)
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
                int currentATK = GetModifiedATK(isPlayerAttack: true);
                var graveyardEffect = _cardData.effects.FirstOrDefault(e => e.trigger == EffectTrigger.OnStrike && e.action == ActionType.GraveyardATK);
                
                if (graveyardEffect.action == ActionType.GraveyardATK && currentATK > _cardData.atk)
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
            hasDoubleStrike = _cardData.effects.Any(e => e.trigger == EffectTrigger.Continuous && e.action == ActionType.DoubleStrike);
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
        var graveyardEffect = _cardData.effects.FirstOrDefault(e => e.trigger == EffectTrigger.OnStrike && e.action == ActionType.GraveyardATK);
        
        if (graveyardEffect.action == ActionType.GraveyardATK)
        {
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
            Debug.Log($"🔥 GraveyardATK [{_cardData.cardName}]: Base={baseATK}, Graves={graveCount}, Per2Cards={graveCount/2}, Value={graveyardEffect.value}, Extra={extraATK}, Total={baseATK + extraATK}");
            return baseATK + extraATK;
        }

        return baseATK;
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
                atkRect.offsetMin = new Vector2(12, 8); // 🔥 มุมซ้ายล่าง (ห่างจากขอบ)
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
                costRect.offsetMax = new Vector2(-10, -1 ); // 🔥 มุมขวาบน (สูงขึ้นอีก 10px)
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
        
        // 🎈 หยุดอนิเมชั่นลอยเพื่อไม่รบกวน HorizontalLayoutGroup
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
    /// ตรวจสอบว่าการ์ดสามารถเล่นได้หรือไม่ (มี PP พอ)
    /// </summary>
    public bool CanPlayCard()
    {
        if (_cardData == null || BattleManager.Instance == null) return false;
        
        // ตรวจสอบ PP พอหรือไม่ (ใช้ GetCost() เพื่อให้ ZeroStats work)
        return BattleManager.Instance.currentPP >= GetCost();
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