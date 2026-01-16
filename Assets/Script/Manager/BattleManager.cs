using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, DEFENDER_CHOICE, WON, LOST }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("--- Game State ---")]
    public BattleState state;
    public int turnCount = 0;

    [Header("--- Player Stats ---")]
    public int maxHP = 20;
    public int currentHP;
    public int maxPP = 0;
    public int currentPP;

    [Header("--- Enemy Stats ---")]
    public int enemyMaxHP = 20;
    public int enemyCurrentHP;
    public int enemyMaxPP = 0;
    public int enemyCurrentPP;

    [Header("--- Field Slots (ลากใส่ให้ครบ!) ---")]
    public Transform[] playerMonsterSlots; 
    public Transform[] playerEquipSlots;   
    public Transform[] enemyMonsterSlots;  
    public Transform[] enemyEquipSlots;    

    [Header("--- Deck & Hand ---")]
    public List<CardData> deckList = new List<CardData>();
    public Transform handArea;
    public Transform enemyHandArea;
    public GameObject cardPrefab;
    public GameObject cardBackPrefab;

    [Header("--- UI References ---")]
    public Slider playerHPBar;
    public Slider enemyHPBar;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI ppText;
    public TextMeshProUGUI enemyPPText;
    public TextMeshProUGUI turnText;
    public GameObject endTurnButton;
    
    // 🔥 ปุ่มรับดาเมจ (ถ้าลืมลากใส่ เกมจะข้ามขั้นตอนถามไปเลย กันค้าง)
    public GameObject takeDamageButton; 

    [Header("--- Effects ---")]
    public Transform playerSpot;
    public Transform enemySpot;
    public GameObject damagePopupPrefab;

    [Header("--- Scene Flow ---")]
    public string stageSceneName = "stage";
    public float endDelay = 1.5f;

    [Header("--- Result Panel ---")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultDetailText;
    public Button resultConfirmButton;

    [Header("--- Card Detail View ---")]
    public CardDetailView cardDetailView;

    [Header("--- Sacrifice Confirm Popup ---")]
    public GameObject sacrificeConfirmPanel; // Panel ยืนยันการ sacrifice
    public TextMeshProUGUI sacrificeMessageText; // ข้อความอธิบาย
    public Button sacrificeConfirmButton; // ปุ่มยืนยัน
    public Button sacrificeCancelButton; // ปุ่มยกเลิก

    [Header("--- Deck Position ---")]
    public Transform deckPileTransform; // ตำแหน่งเด็คที่การ์ดจะบินออกมา

    [Header("--- Mulligan UI ---")]
    public Button playerMulliganButton;
    public TextMeshProUGUI mulliganText;
    public Button playerMulliganConfirmButton;
    public TextMeshProUGUI mulliganHintText;
    public Transform mulliganCenterArea;
    public Transform[] mulliganSlots; // ช่องวางการ์ดที่จั่วได้ (4 ช่อง)
    public Transform[] mulliganSwapSlots; // ช่องลากการ์ดที่ต้องการเปลี่ยน (4 ช่อง)

    [Header("--- Hand Layout ---")]
    public bool useHandLayoutGroup = true;
    public float handSpacing = 30f;
    public Vector2 handCardPreferredSize = new Vector2(140f, 200f);

    private bool isEnding = false;
    private bool resultConfirmed = false;
    private bool isMulliganPhase = false;

    public bool IsMulliganPhase() => isMulliganPhase;

    // --- ตัวแปร Logic ภายใน ---
    private BattleCardUI currentAttackerBot; 
    private bool playerHasMadeChoice = false; 
    private List<CardData> enemyDeckList = new List<CardData>();
    
    // 🔥 Sacrifice System
    private bool sacrificeConfirmed = false;
    private BattleCardUI newCardToSacrifice = null;
    private BattleCardUI targetCardToReplace = null;
    
    // 🔥 Mulligan System
    private int playerMulliganLeft = 1;
    private int enemyMulliganLeft = 1;
    private bool playerFirstTurn = false; // true = ผู้เล่นเริ่มต้น

    void Awake()
    {
        Instance = this;

        // ผูกปุ่ม TakeDamage อัตโนมัติ กันลืมตั้งใน Inspector
        if (takeDamageButton)
        {
            var btn = takeDamageButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickTakeDamage);
            }
        }

        // หา CardDetailView อัตโนมัติถ้าไม่ได้ลากใส่
        if (cardDetailView == null)
        {
            cardDetailView = FindObjectOfType<CardDetailView>(true); // true = รวม inactive objects
        }
    }

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        // 1. Load Deck จากเซฟ (ถ้าเป็นเกมจริง) หรือใช้ที่ตั้งไว้ใน Inspector เป็น fallback
        bool loadedFromSave = LoadPlayerDeckFromSave();
        if (!loadedFromSave)
        {
            if (deckList == null || deckList.Count == 0)
            {
                Debug.LogWarning("⚠️ deckList ว่าง และยังโหลดจากเซฟไม่สำเร็จ กรุณาตั้งค่า deckList ใน Inspector หรือเช็ค GameData/decks");
            }
            else
            {
                Debug.Log("ℹ️ ใช้ deckList จาก Inspector เป็นค่าเริ่มต้น (ไม่ได้โหลดจากเซฟ)");
            }
        }

        // สำเนาเด็คให้บอทใช้แยกกัน จะได้ไม่เสกการ์ดซ้ำไม่จำกัด
        enemyDeckList = new List<CardData>(deckList);
        ShuffleList(deckList);
        ShuffleList(enemyDeckList);

        // 2. Setup Stats
        currentHP = maxHP;
        enemyCurrentHP = enemyMaxHP;
        enemyMaxPP = 0;
        enemyCurrentPP = 0;
        turnCount = 0;

        if (takeDamageButton) takeDamageButton.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

        // 🔥 สุ่มผู้เริ่มต้น
        playerFirstTurn = Random.value > 0.5f;
        Debug.Log(playerFirstTurn ? "👤 ผู้เล่นเริ่มต้น" : "🤖 บอทเริ่มต้น");

        UpdateUI();

        // 3. เตรียมการจั่วเปิดเกม
        bool mulliganReady = cardPrefab != null && mulliganSlots != null && mulliganSlots.Length >= 4;

        if (!mulliganReady)
        {
            Debug.LogWarning("⚠️ Mulligan UI ไม่พร้อม (เช็ค cardPrefab / mulliganSlots) -> ข้าม Mulligan แล้วจั่วเข้ามือเลย");
            
            // 🔥 เช็คว่า cardPrefab มีไหม ถ้าไม่มีก็ยังไม่สามารถจั่วได้
            if (cardPrefab == null)
            {
                Debug.LogError("❌ FATAL: cardPrefab ยังไม่ถูกตั้ง! ตั้งค่าให้ดีก่อนเริ่มเกม!");
                yield break;
            }

            // จั่วเปิด 4 ใบให้ผู้เล่น และ 4 ใบให้บอท
            DrawCard(4, handArea);
            DrawEnemyCard(4);

            // เริ่มเทิร์นทันที
            if (playerFirstTurn)
                StartPlayerTurn();
            else
                StartCoroutine(EnemyTurn());

            yield break;
        }
        
        // 🔥 ตรวจสอบเพิ่มเติม ว่า cardPrefab มี BattleCardUI component ไหม
        if (cardPrefab.GetComponent<BattleCardUI>() == null)
        {
            Debug.LogError("❌ FATAL: cardPrefab ต้องมี BattleCardUI component!");
            yield break;
        }

        // 4. Draw Cards ลง Mulligan Slots
        yield return StartCoroutine(DrawCardsToSlots(4, mulliganSlots));
        DrawEnemyCard(4);

        // 5. Mulligan Phase (ผู้เล่นเลือกก่อนเสมอ)
        yield return StartCoroutine(PlayerMulliganPhase());

        // 6. เริ่มเทิร์น
        if (playerFirstTurn)
            StartPlayerTurn();
        else
            StartCoroutine(EnemyTurn());
    }

    // --------------------------------------------------------
    // 🃏 MULLIGAN SYSTEM (เลือกการ์ดที่จะเปลี่ยน)
    // --------------------------------------------------------

    IEnumerator PlayerMulliganPhase()
    {
        // เริ่มโหมด Mulligan (การ์ดอยู่ใน mulliganSlots แล้ว)
        isMulliganPhase = true;

        // 🔥 Debug: แสดงการ์ดที่จั่วมา
        Debug.Log("🎴 === เริ่ม Mulligan Phase ===");
        for (int i = 0; i < mulliganSlots.Length; i++)
        {
            if (mulliganSlots[i] != null && mulliganSlots[i].childCount > 0)
            {
                var card = mulliganSlots[i].GetChild(0).GetComponent<BattleCardUI>();
                if (card != null)
                    Debug.Log($"🎴 Slot[{i}]: {card.GetData()?.cardName}");
            }
        }

        if (turnText) turnText.text = "MULLIGAN? Click cards to swap";
        ShowPlayerMulliganButton();
        ShowPlayerMulliganConfirm();

        // รอจนกดยืนยันหรือหมดสิทธิ์
        float safetyTimer = 20f; // กันค้าง (เพิ่มเวลาเป็น 20 วินาที)
        while (isMulliganPhase && safetyTimer > 0f)
        {
            safetyTimer -= Time.deltaTime;
            yield return null;
        }
        if (isMulliganPhase) // timeout
        {
            ReturnMulliganCardsToHand();
            HidePlayerMulliganUI();
            isMulliganPhase = false;
        }
    }

    IEnumerator DrawCardsToSlots(int n, Transform[] slots)
    {
        if (deckList.Count < n)
        {
            Debug.LogWarning("⚠️ Deck empty while drawing (player)");
            StartCoroutine(EndBattle(false));
            yield break;
        }

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("⚠️ No slots provided for drawing cards");
            yield break;
        }

        int slotIndex = 0;
        int cardsDrawn = 0;
        
        for(int i=0; i<n && slotIndex < slots.Length && cardsDrawn < n; i++) 
        { 
            // 🔥 หาช่องว่างข้างหน้า
            while (slotIndex < slots.Length && slots[slotIndex].childCount > 0)
            {
                slotIndex++;
            }
            
            // ถ้าหมดช่องว่าง ออกลูป
            if (slotIndex >= slots.Length) break;
            
            CardData d = deckList[0]; 
            deckList.RemoveAt(0);
            
            Transform targetSlot = slots[slotIndex];
            if (targetSlot == null)
            {
                slotIndex++;
                continue;
            }
            
            if(cardPrefab)
            {
                // 🔥 สร้างการ์ดที่ deckPileTransform ก่อน (เพื่อให้ animation ไป slot ได้)
                Transform createParent = deckPileTransform != null ? deckPileTransform : targetSlot;
                GameObject cardObj = Instantiate(cardPrefab, createParent);
                BattleCardUI ui = cardObj.GetComponent<BattleCardUI>();
                if (ui == null) continue;
                
                ui.Setup(d);
                ui.parentAfterDrag = targetSlot;
                
                RectTransform uiRect = ui.GetComponent<RectTransform>();
                
                // เริ่มต้นจากตำแหน่งเด็ค (ตำแหน่ง local ใน parent)
                Vector2 deckStartPos = Vector2.zero;
                
                Debug.Log($"🎴 {ui.name} เริ่มที่เด็ค");
                
                // พักสักครู่เพื่อให้เห็นการ์ด
                yield return new WaitForSeconds(0.3f);
                
                // 🔥 ตัดการ์ดออกจากเด็ค ย้ายไปยัง targetSlot (ตอน animate)
                // สร้าง temporary parent ให้เคลื่อนที่ได้
                RectTransform tempRect = cardObj.GetComponent<RectTransform>();
                if (tempRect == null) continue;
                
                // จำตำแหน่ง world ของเด็ค
                Vector3 deckWorldPos = deckPileTransform != null ? deckPileTransform.position : Vector3.zero;
                Vector3 slotWorldPos = targetSlot.position;
                
                float flyDuration = 0.5f;
                float elapsed = 0f;
                
                while (elapsed < flyDuration)
                {
                    if (cardObj == null) break;
                    
                    elapsed += Time.deltaTime;
                    float t = elapsed / flyDuration;
                    float easeT = 1f - Mathf.Pow(1f - t, 2); // ease out
                    
                    // บินจาก deck ไป slot ในพื้นที่ world
                    cardObj.transform.position = Vector3.Lerp(deckWorldPos, slotWorldPos, easeT);
                    cardObj.transform.localScale = Vector3.Lerp(Vector3.one * 0.6f, Vector3.one, easeT);
                    
                    yield return null;
                }
                
                // Snap เข้า slot อย่างสุดท้าย
                if (cardObj != null)
                {
                    cardObj.transform.SetParent(targetSlot);
                    cardObj.transform.localPosition = Vector3.zero;
                    cardObj.transform.localScale = Vector3.one;
                    
                    Debug.Log($"✅ {ui.name} เข้า slot!");
                }
                
                // พักระหว่างการ์ด
                yield return new WaitForSeconds(0.2f);
                slotIndex++;
            }
        } 
    }

    void ArrangeCardsIntoMulliganSlots()
    {
        if (mulliganCenterArea == null || mulliganSlots == null) return;
        
        // หาการ์ดทั้งหมดใน mulliganCenterArea
        BattleCardUI[] cards = mulliganCenterArea.GetComponentsInChildren<BattleCardUI>();
        
        // วางลงใน mulliganSlots ตามลำดับ
        int slotIndex = 0;
        foreach (var card in cards)
        {
            if (slotIndex >= mulliganSlots.Length) break;
            
            Transform targetSlot = mulliganSlots[slotIndex];
            if (targetSlot != null && targetSlot.childCount == 0)
            {
                card.transform.SetParent(targetSlot);
                card.transform.localPosition = Vector3.zero;
                card.transform.localScale = Vector3.one;
                card.SetMulliganSelect(false);
                card.parentAfterDrag = targetSlot;
                
                // เช็ค raycast target ของ Image และ CanvasGroup
                Image img = card.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;
                
                CanvasGroup cg = card.GetComponent<CanvasGroup>();
                if (cg != null) 
                { 
                    cg.blocksRaycasts = true;
                    cg.interactable = true;
                }
                
                slotIndex++;
            }
        }
        
        // เปิด mask/overflow ของ mulliganCenterArea เพื่อไม่ให้ block raycast
        RectMask2D mask = mulliganCenterArea.GetComponent<RectMask2D>();
        if (mask != null) mask.enabled = false;
        
        CanvasGroup centerCG = mulliganCenterArea.GetComponent<CanvasGroup>();
        if (centerCG != null)
        {
            centerCG.blocksRaycasts = true;
            centerCG.interactable = true;
        }
    }

    void ShowPlayerMulliganButton()
    {
        if (playerMulliganButton)
        {
            playerMulliganButton.gameObject.SetActive(playerMulliganLeft > 0);
            playerMulliganButton.onClick.RemoveAllListeners();
            playerMulliganButton.onClick.AddListener(OnPlayerMulliganOne); // เปลี่ยนเฉพาะใบที่เลือก
            if (mulliganText) mulliganText.text = "Mulligan Left: " + playerMulliganLeft;
        }
        if (mulliganHintText) mulliganHintText.text = "ลากการ์ดที่ต้องการเปลี่ยนไปช่องด้านล่าง แล้วกดปุ่ม Mulligan";
        
        // 🔥 เปิด mulligan slots และ swap slots
        ShowMulliganSlots();
    }
    
    // 🔥 เปิด mulligan UI slots
    void ShowMulliganSlots()
    {
        if (mulliganSlots != null)
        {
            foreach (var slot in mulliganSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(true);
                    
                    // เปิด Image กลับ
                    Image img = slot.GetComponent<Image>();
                    if (img != null) img.enabled = true;
                    
                    // ตั้ง CanvasGroup
                    CanvasGroup cg = slot.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 1f;
                        cg.blocksRaycasts = true;
                    }
                }
            }
        }
        
        if (mulliganSwapSlots != null)
        {
            foreach (var slot in mulliganSwapSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(true);
                    
                    // เปิด Image กลับ
                    Image img = slot.GetComponent<Image>();
                    if (img != null) img.enabled = true;
                    
                    // ตั้ง CanvasGroup
                    CanvasGroup cg = slot.GetComponent<CanvasGroup>();
                    if (cg != null)
                    {
                        cg.alpha = 1f;
                        cg.blocksRaycasts = true;
                    }
                }
            }
        }
        
        if (mulliganCenterArea != null)
        {
            mulliganCenterArea.gameObject.SetActive(true);
            
            // เปิด Image กลับ
            Image img = mulliganCenterArea.GetComponent<Image>();
            if (img != null) img.enabled = true;
            
            // ตั้ง CanvasGroup
            CanvasGroup cg = mulliganCenterArea.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
            }
        }
    }

    void ShowPlayerMulliganConfirm()
    {
        if (playerMulliganConfirmButton)
        {
            playerMulliganConfirmButton.gameObject.SetActive(true);
            playerMulliganConfirmButton.onClick.RemoveAllListeners();
            playerMulliganConfirmButton.onClick.AddListener(OnPlayerMulliganConfirm);
        }
    }

    void HidePlayerMulliganUI()
    {
        if (playerMulliganButton) playerMulliganButton.gameObject.SetActive(false);
        if (playerMulliganConfirmButton) playerMulliganConfirmButton.gameObject.SetActive(false);
        if (mulliganText) mulliganText.text = string.Empty;
        if (mulliganHintText) mulliganHintText.text = string.Empty;
        
        // 🔥 ปิด Canvas Group ของ mulligan UI เพื่อไม่ให้ block raycast
        if (mulliganCenterArea != null)
        {
            CanvasGroup cg = mulliganCenterArea.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;       // ทำให้โปร่งใส
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
            
            // ปิด Image component ถ้ามี (แถบดำมักเกิดจาก Image)
            Image img = mulliganCenterArea.GetComponent<Image>();
            if (img != null) img.enabled = false;
            
            mulliganCenterArea.gameObject.SetActive(false);
            Debug.Log("✅ ซ่อน mulliganCenterArea");
        }
        
        // 🔥 ซ่อนช่อง mulligan slots ทั้งหมด
        if (mulliganSlots != null && mulliganSlots.Length > 0)
        {
            // ซ่อน parent GameObject ของ mulliganSlots (muliganslot)
            Transform mulliganSlotsParent = mulliganSlots[0]?.parent;
            if (mulliganSlotsParent != null)
            {
                CanvasGroup parentCg = mulliganSlotsParent.GetComponent<CanvasGroup>();
                if (parentCg != null)
                {
                    parentCg.alpha = 0f;
                    parentCg.blocksRaycasts = false;
                    parentCg.interactable = false;
                }
                mulliganSlotsParent.gameObject.SetActive(false);
                Debug.Log($"✅ ซ่อน {mulliganSlotsParent.name}");
            }
            
            foreach (var slot in mulliganSlots)
            {
                if (slot != null)
                {
                    Image slotImg = slot.GetComponent<Image>();
                    if (slotImg != null) slotImg.enabled = false;
                    
                    CanvasGroup slotCg = slot.GetComponent<CanvasGroup>();
                    if (slotCg != null)
                    {
                        slotCg.alpha = 0f;
                        slotCg.blocksRaycasts = false;
                    }
                    
                    slot.gameObject.SetActive(false);
                }
            }
        }
        
        // 🔥 ซ่อนช่อง mulligan swap slots ทั้งหมด
        if (mulliganSwapSlots != null && mulliganSwapSlots.Length > 0)
        {
            // ซ่อน parent GameObject ของ mulliganSwapSlots (muliganswap)
            Transform mulliganSwapParent = mulliganSwapSlots[0]?.parent;
            if (mulliganSwapParent != null)
            {
                CanvasGroup parentCg = mulliganSwapParent.GetComponent<CanvasGroup>();
                if (parentCg != null)
                {
                    parentCg.alpha = 0f;
                    parentCg.blocksRaycasts = false;
                    parentCg.interactable = false;
                }
                mulliganSwapParent.gameObject.SetActive(false);
                Debug.Log($"✅ ซ่อน {mulliganSwapParent.name}");
            }
            
            foreach (var slot in mulliganSwapSlots)
            {
                if (slot != null)
                {
                    Image swapImg = slot.GetComponent<Image>();
                    if (swapImg != null) swapImg.enabled = false;
                    
                    CanvasGroup swapCg = slot.GetComponent<CanvasGroup>();
                    if (swapCg != null)
                    {
                        swapCg.alpha = 0f;
                        swapCg.blocksRaycasts = false;
                    }
                    
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }

    // ผู้เล่นกดเปลี่ยนการ์ด (ใช้สิทธิ์ 1 ครั้ง) เฉพาะใบที่เลือกเท่านั้น
    void OnPlayerMulliganOne()
    {
        if (playerMulliganLeft <= 0) return;

        StartCoroutine(PerformMulliganReplacement());
    }

    IEnumerator PerformMulliganReplacement()
    {
        int replaced = ReplaceSelectedMulliganCards();
        if (replaced > 0)
        {
            playerMulliganLeft = 0; // ใช้สิทธิ์หมด

            if (mulliganText) mulliganText.text = "Mulligan Left: " + playerMulliganLeft;

            // จั่วการ์ดใหม่เข้า mulliganSlots โดยตรง (พร้อมอนิเมชั่น)
            yield return StartCoroutine(DrawCardsToSlots(replaced, mulliganSlots));

            // รอให้เห็นการ์ดใน slot นานขึ้น (เพิ่มเวลาให้เห็นชัด)
            yield return new WaitForSeconds(3.0f); // เพิ่มเป็น 3 วินาที

            // ถ้าใช้สิทธิ์หมด ให้ยืนยันอัตโนมัติ (ย้ายการ์ดขึ้นมือ)
            if (playerMulliganLeft <= 0)
            {
                OnPlayerMulliganConfirm();
            }
        }
    }

    // ยืนยันจบเฟส mulligan และย้ายการ์ดกลับมือ
    void OnPlayerMulliganConfirm()
    {
        if (!isMulliganPhase) return; // 🔥 ป้องกัน double-click
        
        Debug.Log("🎴 ผู้เล่นยืนยัน mulligan - เริ่มกระบวนการ...");
        
        // 🔥 เช็คว่ามีการ์ดใน swap slots ไหม
        int cardsInSwap = 0;
        if (mulliganSwapSlots != null)
        {
            foreach (var slot in mulliganSwapSlots)
            {
                if (slot != null && slot.childCount > 0) cardsInSwap++;
            }
        }
        
        if (cardsInSwap > 0)
        {
            Debug.Log($"🎴 พบการ์ด {cardsInSwap} ใบใน swap slots -> เริ่มเปลี่ยนการ์ด");
            StartCoroutine(ConfirmWithReplacement());
        }
        else
        {
            Debug.Log("🎴 ไม่มีการ์ดใน swap slots -> ขึ้นมือเลย");
            ReturnMulliganCardsToHand();
            Debug.Log("🎴 ขั้น 1: ย้ายการ์ดจาก mulligan slots เข้ามือเสร็จ");
            
            HidePlayerMulliganUI();
            Debug.Log("🎴 ขั้น 2: ซ่อน mulligan UI เสร็จ");
            
            isMulliganPhase = false;
            
            // 🔥 รอ 1 frame ให้ Unity rebuild layout ก่อนจัดการ์ด
            StartCoroutine(ArrangeCardsAfterFrame());
        }
    }
    
    // 🔥 Coroutine สำหรับยืนยันพร้อมเปลี่ยนการ์ด
    IEnumerator ConfirmWithReplacement()
    {
        int replaced = ReplaceSelectedMulliganCards();
        Debug.Log($"🎴 คืนการ์ด {replaced} ใบเข้าเด็คและสับแล้ว");
        
        if (replaced > 0)
        {
            // จั่วการ์ดใหม่เข้า mulliganSlots
            yield return StartCoroutine(DrawCardsToSlots(replaced, mulliganSlots));
            Debug.Log($"🎴 จั่วการ์ดใหม่ {replaced} ใบเข้า mulligan slots แล้ว");
        }
        
        // ย้ายการ์ดทั้งหมดขึ้นมือ
        ReturnMulliganCardsToHand();
        Debug.Log("🎴 ย้ายการ์ดทั้งหมดเข้ามือแล้ว");
        
        HidePlayerMulliganUI();
        Debug.Log("🎴 ซ่อน mulligan UI แล้ว");
        
        isMulliganPhase = false;
        
        // 🔥 รอ 1 frame ให้ Unity rebuild layout ก่อนจัดการ์ด
        yield return StartCoroutine(ArrangeCardsAfterFrame());
    }
    
    // 🔥 จัดการ์ดหลังจาก layout rebuild เสร็จ
    IEnumerator ArrangeCardsAfterFrame()
    {
        yield return null; // รอ 1 frame
        ArrangeCardsInHand();
        Debug.Log("🎴 ขั้น 3: จัดการ์ดในมือเสร็จ (หลัง 1 frame)");
    }
    
    // 🔥 จัดการ์ดในมือ: ใช้ HorizontalLayoutGroup ถ้ามี (ง่ายและเสถียรกว่า)
    void ArrangeCardsInHand()
    {
        if (handArea == null)
        {
            Debug.LogError("❌ handArea ยังไม่ถูกตั้ง! ตรวจสอบ Inspector");
            return;
        }

        var cardsInHand = handArea.GetComponentsInChildren<BattleCardUI>();
        if (cardsInHand.Length == 0)
        {
            Debug.Log("⚠️ ไม่มีการ์ดในมือ");
            return;
        }

        var layout = handArea.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (useHandLayoutGroup && layout != null)
        {
            layout.enabled = true;
            layout.spacing = handSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            Debug.Log($"🎴 HLG Settings: spacing={layout.spacing}, controlW={layout.childControlWidth}, controlH={layout.childControlHeight}, expandW={layout.childForceExpandWidth}");

            // ให้แต่ละการ์ดมี LayoutElement เพื่อกำหนด preferred size
            foreach (var card in cardsInHand)
            {
                if (card == null) continue;
                if (card.transform.parent != handArea)
                    card.transform.SetParent(handArea, false);

                var rt = card.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.localRotation = Quaternion.identity;
                }

                var le = card.GetComponent<UnityEngine.UI.LayoutElement>();
                if (le == null) le = card.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                le.preferredWidth = handCardPreferredSize.x;
                le.preferredHeight = handCardPreferredSize.y;
                le.minWidth = 0f;
                le.minHeight = 0f;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;
                
                Debug.Log($"🎴 Card[{card.name}]: LE(prefW={le.preferredWidth}, prefH={le.preferredHeight}), localPos={rt?.localPosition}");

                var img = card.GetComponent<Image>();
                if (img) img.color = Color.white;

                var cg = card.GetComponent<CanvasGroup>();
                if (cg)
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    cg.alpha = 1f;
                }
            }

            var handRect = handArea as RectTransform;
            if (handRect)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(handRect);
                Canvas.ForceUpdateCanvases(); // 🔥 บังคับให้ Canvas อัพเดททันที
            }
            
            // 🔥 Debug: แสดงตำแหน่งสุดท้ายของการ์ดแต่ละใบ
            Debug.Log("🎴 === ตำแหน่งการ์ดหลัง layout ===");
            for (int i = 0; i < cardsInHand.Length; i++)
            {
                var rt = cardsInHand[i].GetComponent<RectTransform>();
                if (rt != null)
                    Debug.Log($"🎴 Card[{i}] {cardsInHand[i].name}: localPos={rt.localPosition}, anchoredPos={rt.anchoredPosition}");
            }
            
            Debug.Log($"✅ จัดการ์ดในมือด้วย HorizontalLayoutGroup (spacing={handSpacing}, count={cardsInHand.Length})");
            return;
        }

        // Fallback: manual layout ถ้าไม่มี HLG
        if (layout != null) layout.enabled = false;

        int count = 0;
        float cardWidth = Mathf.Max(10f, handCardPreferredSize.x);
        float spacing = Mathf.Max(10f, handSpacing);
        float totalWidth = (cardWidth + spacing) * cardsInHand.Length;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        foreach (var card in cardsInHand)
        {
            if (card == null) continue;
            if (card.transform.parent != handArea)
                card.transform.SetParent(handArea, false);

            float xPos = startX + count * (cardWidth + spacing);
            card.transform.localPosition = new Vector3(xPos, 0, 0);
            card.transform.localScale = Vector3.one;
            card.transform.localRotation = Quaternion.identity;

            var img = card.GetComponent<Image>();
            if (img) img.color = Color.white;
            var cg = card.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
            count++;
        }

        var handRect2 = handArea as RectTransform;
        if (handRect2)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(handRect2);
            Canvas.ForceUpdateCanvases(); // 🔥 บังคับให้ Canvas อัพเดททันที
        }
        Debug.Log($"✅ จัดการ์ดในมือแบบ manual (spacing={spacing}, count={cardsInHand.Length})");
    }

    int ReplaceSelectedMulliganCards()
    {
        int replacedCount = 0;

        // รวบรวมการ์ดที่อยู่ใน mulliganSwapSlots (ช่องเปลี่ยน) เท่านั้น
        List<BattleCardUI> selected = new List<BattleCardUI>();

        if (mulliganSwapSlots != null)
        {
            foreach (var slot in mulliganSwapSlots)
            {
                if (slot != null && slot.childCount > 0)
                {
                    var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                    if (card != null) selected.Add(card);
                }
            }
        }

        if (selected.Count == 0) return 0;

        // คืนการ์ดที่เลือกกลับเข้าเด็คแล้วสับ
        foreach (var card in selected)
        {
            CardData data = card.GetData();
            if (data != null)
            {
                deckList.Add(data);
            }
            Destroy(card.gameObject);
            replacedCount++;
        }
        ShuffleList(deckList);

        // ไม่ต้องจั่วที่นี่ จะจั่วใน PerformMulliganReplacement แทน
        // (เพื่อให้มีอนิเมชั่นและวางเข้า slots โดยตรง)

        return replacedCount;
    }

    void ReturnMulliganCardsToHand()
    {
        // 🔥 ย้ายการ์ดจาก mulliganSlots ไปที่ handArea เพื่อเล่นจริง
        if (mulliganSlots != null && handArea != null)
        {
            foreach (var slot in mulliganSlots)
            {
                if (slot != null)
                {
                    while (slot.childCount > 0)
                    {
                        var child = slot.GetChild(0);
                        var cardUI = child.GetComponent<BattleCardUI>();
                        
                        // ย้ายการ์ดไปที่มือจริง
                        child.SetParent(handArea, false);
                        
                        // ล้าง state mulligan
                        if (cardUI != null)
                        {
                            cardUI.SetMulliganSelect(false);
                            cardUI.parentAfterDrag = handArea;
                        }
                        
                        Debug.Log($"✅ ย้ายการ์ดจาก mulliganSlot → handArea");
                    }
                }
            }
        }

        // เก็บจากช่องเปลี่ยน (mulliganSwapSlots) และส่งกลับไปมือด้วย
        if (mulliganSwapSlots != null && handArea != null)
        {
            foreach (var slot in mulliganSwapSlots)
            {
                if (slot != null)
                {
                    while (slot.childCount > 0)
                    {
                        var child = slot.GetChild(0);
                        var cardUI = child.GetComponent<BattleCardUI>();
                        
                        // ย้ายกลับไปมือ
                        child.SetParent(handArea, false);
                        
                        if (cardUI != null)
                        {
                            cardUI.SetMulliganSelect(false);
                            cardUI.parentAfterDrag = handArea;
                        }
                        
                        Debug.Log($"✅ ย้ายการ์ดจาก mulliganSwapSlot → handArea");
                    }
                }
            }
        }

        // ล้างจาก mulliganCenterArea (ถ้ามีการ์ดวนอยู่) ส่งกลับไปมือด้วย
        if (mulliganCenterArea != null && handArea != null)
        {
            var cardsInCenter = mulliganCenterArea.GetComponentsInChildren<BattleCardUI>();
            foreach (var card in cardsInCenter)
            {
                card.transform.SetParent(handArea, false);
                card.SetMulliganSelect(false);
                card.parentAfterDrag = handArea;
                
                Debug.Log($"✅ ย้ายการ์ดจาก mulliganCenterArea → handArea");
            }
        }
        
        Debug.Log("✅ ย้ายการ์ด mulligan ทั้งหมดเข้ามือ");
    }

    // หาช่อง mulliganSwapSlots ว่าง
    public Transform GetFreeSwapSlot()
    {
        if (mulliganSwapSlots == null) return null;

        foreach (var slot in mulliganSwapSlots)
        {
            if (slot != null && slot.childCount == 0) return slot;
        }
        return null;
    }
    
    // 🔥 หาช่อง mulliganSlots ว่าง
    public Transform GetFreeMulliganSlot()
    {
        if (mulliganSlots == null) return null;

        foreach (var slot in mulliganSlots)
        {
            if (slot != null && slot.childCount == 0) return slot;
        }
        return null;
    }

    // ย้ายการ์ดไปช่องเปลี่ยน (เรียกจาก BattleCardUI เมื่อคลิกซ้าย)
    public bool TryMoveCardToSwapSlot(BattleCardUI card)
    {
        if (!isMulliganPhase) return false;

        Transform freeSlot = GetFreeSwapSlot();
        if (freeSlot == null)
        {
            Debug.Log("ช่องเปลี่ยนเต็มแล้ว (4/4)");
            return false;
        }

        card.transform.SetParent(freeSlot);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;
        return true;
    }


    // --------------------------------------------------------
    // 🔄 TURN SYSTEM
    // --------------------------------------------------------

    void StartPlayerTurn()
    {
        if (isEnding) return;

        // เด็คหมดก่อนจั่ว -> แพ้ทันที
        if (deckList.Count <= 0)
        {
            Debug.Log("⚠️ Deck empty (player) -> Lose");
            StartCoroutine(EndBattle(false));
            return;
        }

        state = BattleState.PLAYERTURN;
        turnCount++;
        
        maxPP = Mathf.Clamp(turnCount, 1, 10);
        currentPP = maxPP;

        ResetAllMonstersAttackState();

        if (turnText) turnText.text = "YOUR TURN";
        if (endTurnButton) endTurnButton.SetActive(true);
        if (takeDamageButton) takeDamageButton.SetActive(false);

        DrawCard(1);
        UpdateUI();
    }

    /// <summary>
    /// โหลดเด็คที่ผู้เล่นจัดไว้จาก GameData (เลือกจาก PlayerPrefs "SelectedDeckIndex" หรือ index 0)
    /// </summary>
    bool LoadPlayerDeckFromSave()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentGameData == null)
        {
            Debug.LogWarning("⚠️ GameManager หรือ CurrentGameData เป็น null - ข้ามการโหลดเด็ค");
            return false;
        }

        var data = GameManager.Instance.CurrentGameData;
        if (data.decks == null || data.decks.Count == 0)
        {
            Debug.LogWarning("⚠️ ผู้เล่นยังไม่มีเด็ค (data.decks ว่าง)");
            return false;
        }

        int selectedIndex = PlayerPrefs.GetInt("SelectedDeckIndex", 0);
        if (selectedIndex < 0 || selectedIndex >= data.decks.Count) selectedIndex = 0;

        DeckData selectedDeck = data.decks[selectedIndex];
        if (selectedDeck == null || selectedDeck.card_ids_in_deck == null || selectedDeck.card_ids_in_deck.Count == 0)
        {
            Debug.LogWarning($"⚠️ เด็ค index {selectedIndex} ยังไม่มีการ์ด");
            return false;
        }

        // โหลดคลังการ์ดทั้งหมดจาก Resources เพื่อ map ด้วย card_id
        CardData[] allCards = Resources.LoadAll<CardData>("GameContent/Cards");
        if (allCards == null || allCards.Length == 0)
        {
            Debug.LogError("❌ ไม่พบการ์ดใน Resources/GameContent/Cards");
            return false;
        }

        var lookup = allCards.ToDictionary(c => c.card_id, c => c);
        List<CardData> loadedDeck = new List<CardData>();

        foreach (string id in selectedDeck.card_ids_in_deck)
        {
            if (string.IsNullOrEmpty(id)) continue;

            if (lookup.TryGetValue(id, out CardData card))
            {
                loadedDeck.Add(card);
            }
            else
            {
                Debug.LogWarning($"⚠️ ไม่พบการ์ด id={id} ใน Resources");
            }
        }

        if (loadedDeck.Count == 0)
        {
            Debug.LogWarning("⚠️ โหลดเด็คไม่สำเร็จ ไม่มีการ์ดที่ match");
            return false;
        }

        deckList = loadedDeck;
        Debug.Log($"✅ โหลดเด็คผู้เล่นสำเร็จ: {selectedDeck.deck_name} (index {selectedIndex}) จำนวน {deckList.Count} ใบ");
        return true;
    }

    public void OnEndTurnButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        if (endTurnButton) endTurnButton.SetActive(false);
        StartCoroutine(EnemyTurn());
    }

    // --------------------------------------------------------
    // 🃏 PLAYER SUMMON
    // --------------------------------------------------------

    public void OnCardPlayed(BattleCardUI cardUI)
    {
        if (isMulliganPhase) return; // ห้ามเล่นระหว่าง mulligan
        if (state != BattleState.PLAYERTURN) return;

        CardData data = cardUI.GetData();
        if (currentPP < data.cost) return;

        // ถ้าเป็นการ์ดเวทย์ (Spell) ให้ใช้งานแล้วทิ้ง ไม่ลงช่อง Equip
        if (data.type == CardType.Spell)
        {
            CastSpellCard(cardUI);
            return;
        }

        Transform freeSlot = GetFreeSlot(data.type, true);
        if (freeSlot != null) PayCostAndSummon(cardUI, freeSlot, data.cost);
    }

    public void TrySummonCard(BattleCardUI cardUI, CardSlot targetSlot)
    {
        if (isMulliganPhase) return;
        if (state != BattleState.PLAYERTURN) return;

        CardData data = cardUI.GetData();

        // Spell ไม่ควรถูกลากลงสนาม ใช้ได้เฉพาะกดเล่นจากมือ
        if (data.type == CardType.Spell) return;

        if (data.type != targetSlot.allowedType) return;
        if (targetSlot.transform.childCount > 0) return;
        if (currentPP < data.cost) return;

        PayCostAndSummon(cardUI, targetSlot.transform, data.cost);
    }

    void PayCostAndSummon(BattleCardUI cardUI, Transform parentSlot, int cost)
    {
        currentPP -= cost;
        cardUI.transform.SetParent(parentSlot);
        cardUI.transform.localPosition = Vector3.zero;
        
        cardUI.isOnField = true;
        cardUI.hasAttacked = true; 
        cardUI.GetComponent<Image>().color = Color.gray; // สีเทา = Summoning Sickness

        if(AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");
        UpdateUI();
    }

    // ใช้การ์ดเวทย์แล้วทิ้ง (ยังไม่ได้ใส่เอฟเฟกต์ แค่ตัดค่า PP และทำลายการ์ดบนมือ)
    void CastSpellCard(BattleCardUI cardUI)
    {
        currentPP -= cardUI.GetCost();

        // TODO: ใส่เอฟเฟกต์การ์ดเวทย์ที่ต้องการที่นี่

        Destroy(cardUI.gameObject);
        if(AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");
        UpdateUI();
    }

    // --------------------------------------------------------
    // ⚔️ PLAYER ATTACK
    // --------------------------------------------------------

    public void OnPlayerAttack(BattleCardUI attacker)
    {
        if (state != BattleState.PLAYERTURN) return;

        attacker.hasAttacked = true;
        attacker.GetComponent<Image>().color = Color.gray;

        StartCoroutine(ProcessPlayerAttack(attacker));
    }

    IEnumerator ProcessPlayerAttack(BattleCardUI attacker)
    {
        Vector3 startPos = attacker.transform.position;
        int damage = attacker.GetData().atk;
        
        // พุ่งไป (เร็วขึ้น 0.3 วินาที)
        yield return StartCoroutine(MoveToTarget(attacker.transform, enemySpot.position, 0.3f));

        BattleCardUI botShield = GetBestEnemyEquip(attacker.GetData().subCategory);

        if (botShield != null)
        {
            Debug.Log($"🛡️ บอทกันด้วย {botShield.GetData().cardName} ({botShield.GetData().subCategory})");
            if(AudioManager.Instance) AudioManager.Instance.PlaySFX("Block");

            // 🔥 ตรวจสอบ null ก่อนเช็คประเภท
            if (attacker == null || attacker.GetData() == null || botShield.GetData() == null)
            {
                Debug.LogWarning("ProcessPlayerAttack: null card data detected!");
                yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
                yield break;
            }

            CardData attackerData = attacker.GetData();
            CardData shieldData = botShield.GetData();
            bool match = (attackerData.subCategory == shieldData.subCategory);

            if (match)
            {
                // ประเภทตรง → ทำลายทั้งคู่
                ShowDamagePopupString("Double KO!", attacker.transform);
                Destroy(attacker.gameObject);
                Destroy(botShield.gameObject);
                Debug.Log($"✅ บอทกันได้! ประเภทตรงกัน ({shieldData.subCategory}) - ทั้งคู่ทำลาย ไม่เสีย HP");
            }
            else
            {
                // ประเภทต่างกัน → ทำลายเฉพาะโล่
                ShowDamagePopupString("Shield Break!", botShield.transform);
                Destroy(botShield.gameObject);
                Debug.Log($"✅ บอทกันได้! ประเภทต่างกัน ({attackerData.subCategory} ≠ {shieldData.subCategory}) - โล่แตก ไม่เสีย HP");
            }

            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
        }
        else
        {
            Debug.Log($"💥 ไม่มีโล่ -> บอทรับดาเมจ {damage}");
            EnemyTakeDamage(damage);
            yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
        }

        // เช็คชนะ
        if (enemyCurrentHP <= 0)
        {
            Debug.Log("🎉 ศัตรูตายแล้ว -> Win!");
            StartCoroutine(EndBattle(true));
        }

        UpdateUI();
    }

    // --------------------------------------------------------
    // 🤖 ENEMY TURN
    // --------------------------------------------------------

    IEnumerator EnemyTurn()
    {
        if (isEnding) yield break;

        // เด็คหมด -> ผู้เล่นชนะ
        if (enemyDeckList.Count <= 0)
        {
            Debug.Log("⚠️ Deck empty (enemy) -> Win");
            StartCoroutine(EndBattle(true));
            yield break;
        }

        state = BattleState.ENEMYTURN;
        if (turnText) turnText.text = "ENEMY TURN";

        // ตั้ง PP ฝั่งบอทให้เท่าจำนวนเทิร์น (เหมือนผู้เล่น) สูงสุด 10
        enemyMaxPP = Mathf.Clamp(turnCount, 1, 10);
        enemyCurrentPP = enemyMaxPP;

        // 🔥 รีเซ็ตสถานะโจมตีของมอนสเตอร์บอททั้งหมด
        ResetAllEnemyMonstersAttackState();

        yield return new WaitForSeconds(0.5f);
        BotSummonPhase();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(BotAttackPhase());
        yield return new WaitForSeconds(0.5f);

        if (isEnding || state == BattleState.WON || state == BattleState.LOST) yield break;

        if (state != BattleState.LOST) StartPlayerTurn();
    }

    void BotSummonPhase()
    {
        Transform freeMonSlot = GetFreeSlot(CardType.Monster, false);
        if (freeMonSlot != null)
        {
            CardData botCard = enemyDeckList.Find(x => x.type == CardType.Monster);
            if (botCard != null && enemyCurrentPP >= botCard.cost)
            {
                SpawnBotCard(botCard, freeMonSlot);
                enemyCurrentPP -= botCard.cost;
                enemyDeckList.Remove(botCard);
            }
        }

        Transform freeEqSlot = GetFreeSlot(CardType.EquipSpell, false);
        if (freeEqSlot != null)
        {
            CardData botCard = enemyDeckList.Find(x => x.type == CardType.EquipSpell);
            if (botCard != null && enemyCurrentPP >= botCard.cost)
            {
                SpawnBotCard(botCard, freeEqSlot);
                enemyCurrentPP -= botCard.cost;
                enemyDeckList.Remove(botCard);
            }
        }
    }

    void SpawnBotCard(CardData data, Transform slot)
    {
        GameObject newCard = Instantiate(cardPrefab, slot);
        var ui = newCard.GetComponent<BattleCardUI>();
        ui.Setup(data);
        ui.isOnField = true;
        // จัดให้อยู่กึ่งกลางช่องและขนาดมาตรฐาน
        RectTransform rect = newCard.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(140, 200);
        }
        
        // 🔥 แก้: บอทลงมาตีไม่ได้ เทิร์นนี้ (Summoning Sickness)
        ui.hasAttacked = true;
        newCard.GetComponent<Image>().color = Color.gray;
        
        Debug.Log($"🤖 บอทลงมอนสเตอร์: {data.cardName} (ห้ามตีเทิร์นนี้)");
    }

    // 🔥 Logic บอทโจมตี (แบบ Safe Mode กันค้าง)
    IEnumerator BotAttackPhase()
    {
        foreach (Transform slot in enemyMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var monster = slot.GetChild(0).GetComponent<BattleCardUI>();
                // 🔥 แก้: เช็คว่าตัวมอนสเตอร์ยังไม่ได้โจมตีในเทิร์นนี้ (Summoning Sickness)
                if (monster != null && !monster.hasAttacked)
                {
                    currentAttackerBot = monster;
                    
                    // ตั้งสถานะว่าโจมตีแล้ว และเปลี่ยนสีเป็นเทา
                    monster.hasAttacked = true;
                    monster.GetComponent<Image>().color = Color.gray;
                    
                    Vector3 startPos = monster.transform.position;
                    // กัน Error: ถ้าลืมลาก PlayerSpot ให้วิ่งไปที่ (0,0,0)
                    Vector3 targetPos = (playerSpot != null) ? playerSpot.position : Vector3.zero;

                    // 1. พุ่งมา (เร็วขึ้น 0.3 วินาที)
                    yield return StartCoroutine(MoveToTarget(monster.transform, targetPos, 0.3f));

                    Debug.Log($"🚨 บอทใช้ {monster.GetData().cardName} โจมตี!");

                    // 2. เช็คโล่
                    bool playerHasShield = HasEquipInSlots(playerEquipSlots);

                    // ต้องมีโล่ และ มีปุ่ม ถึงจะหยุดถาม (ถ้าลืมลากปุ่ม จะตีเลยกันค้าง)
                    if (playerHasShield && takeDamageButton != null)
                    {
                        state = BattleState.DEFENDER_CHOICE;
                        playerHasMadeChoice = false;

                        takeDamageButton.SetActive(true);
                        if (turnText) turnText.text = "DEFEND!";

                        yield return new WaitUntil(() => playerHasMadeChoice);

                        if(takeDamageButton) takeDamageButton.SetActive(false);
                    }
                    else
                    {
                        // ตีเลย
                        if(playerHasShield && takeDamageButton == null) Debug.LogError("⚠️ ลืมลากปุ่ม TakeDamageButton!");
                        
                        yield return new WaitForSeconds(0.2f);
                        if(monster != null) PlayerTakeDamage(monster.GetData().atk);
                    }

                    // 3. ดึงกลับ (เช็คว่าตัวยังอยู่ไหม ถ้าถูกทำลายในระหว่าง defend จะ skip)
                    if (monster != null && monster.gameObject != null && monster.transform != null)
                    {
                        yield return StartCoroutine(MoveToTarget(monster.transform, startPos, 0.25f));
                        if (monster != null && monster.transform != null) 
                        {
                            monster.transform.localPosition = Vector3.zero; // Snap (check again)
                        }
                    }
                    else
                    {
                        Debug.Log("✅ มอนสเตอร์ถูกทำลายแล้ว (กันได้) → ไม่ต้องดึงกลับ");
                    }

                    if (state == BattleState.LOST) break;
                }
            }
        }
    }

    // --------------------------------------------------------
    // 🛡️ PLAYER DEFENSE INPUT
    // --------------------------------------------------------

    public void OnClickTakeDamage()
    {
        // ปุ่มนี้ควรทำงานเหมือนการคลิกใบอื่น (ไม่กัน ปล่อยดาเมจเข้า)
        OnPlayerSkipBlock();
    }

    // ใช้เมื่อผู้เล่นเลือกไม่กัน (เช่น คลิกที่การ์ดอื่นหรือกดปุ่มข้าม)
    public CardData GetCurrentAttackerData()
    {
        return currentAttackerBot != null ? currentAttackerBot.GetData() : null;
    }

    public void OnPlayerSkipBlock()
    {
        if (state != BattleState.DEFENDER_CHOICE) return;
        if (currentAttackerBot == null)
        {
            Debug.LogWarning("SkipBlock but attacker is null; continuing turn.");
            playerHasMadeChoice = true;
            if (takeDamageButton) takeDamageButton.SetActive(false);
            return;
        }

        PlayerTakeDamage(currentAttackerBot.GetData().atk);
        playerHasMadeChoice = true;
        if (takeDamageButton) takeDamageButton.SetActive(false);
    }

    public void OnPlayerSelectBlocker(BattleCardUI myShield)
    {
        if (state != BattleState.DEFENDER_CHOICE) return;

        // 🔥 ตรวจสอบ null ก่อนเช็คประเภท
        if (currentAttackerBot == null || currentAttackerBot.GetData() == null || 
            myShield == null || myShield.GetData() == null)
        {
            Debug.LogWarning("OnPlayerSelectBlocker: null card data detected!");
            playerHasMadeChoice = true;
            if (takeDamageButton) takeDamageButton.SetActive(false);
            return;
        }

        CardData attackerData = currentAttackerBot.GetData();
        CardData shieldData = myShield.GetData();
        
        Debug.Log($"🛡️ ตรวจสอบการกัน: โจมตี={attackerData.cardName} ({attackerData.subCategory}), โล่={shieldData.cardName} ({shieldData.subCategory})");
        
        bool match = (attackerData.subCategory == shieldData.subCategory);

        if (match)
        {
            ShowDamagePopupString("Double KO!", currentAttackerBot.transform);
            Destroy(currentAttackerBot.gameObject);
            Destroy(myShield.gameObject);
            Debug.Log($"✅ กันได้! ประเภทตรงกัน ({attackerData.subCategory}) - ทั้งคู่ทำลาย ไม่เสีย HP");
        }
        else
        {
            ShowDamagePopupString("Shield Break!", myShield.transform);
            Destroy(myShield.gameObject);
            
            // 🔥 ประเภทไม่ตรง → โล่แตก แต่ไม่เสีย HP (ปกป้องสำเร็จ)
            Debug.Log($"✅ กันได้! ประเภทต่างกัน ({attackerData.subCategory} ≠ {shieldData.subCategory}) - โล่แตก แต่ไม่เสีย HP");
        }
        
        // 🔥 เซ็ตแล้วหลัง logic กันค้าง
        playerHasMadeChoice = true;
        if (takeDamageButton) takeDamageButton.SetActive(false);
    }

    // --------------------------------------------------------
    // 🔧 UTILITIES
    // --------------------------------------------------------

    // ฟังก์ชันเคลื่อนที่ (พร้อม Safety Timer กันค้าง)
    // --------------------------------------------------------
    // 🔧 UTILITIES (แก้ใหม่: ใช้เวลาแทนความเร็ว พุ่งแรงแน่นอน)
    // --------------------------------------------------------

    IEnumerator MoveToTarget(Transform obj, Vector3 target, float duration)
    {
        // 🔥 ตรวจสอบ object ก่อน - ถ้า null หรือ destroy ไปแล้วให้หยุดทันที
        if (obj == null) 
        {
            Debug.Log("⚠️ MoveToTarget: obj เป็น null → ข้าม");
            yield break;
        }

        // duration = เวลาที่ใช้ (เช่น 0.2 วินาที คือเร็วมาก)
        if (duration <= 0f) duration = 0.1f; 

        Vector3 startPos = obj.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 🔥 เช็ค obj ทุก frame เพื่อหยุดถ้ามันถูก destroy
            if (obj == null)
            {
                Debug.Log("⚠️ MoveToTarget: obj ถูก destroy ระหว่าง coroutine → ข้าม");
                yield break;
            }

            // ขยับตามเวลา (Lerp)
            obj.position = Vector3.Lerp(startPos, target, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 🔥 เช็ค obj สุดท้ายก่อน snap
        if (obj != null)
        {
            obj.position = target;
            
            // 🔥 เพิ่ม: Shake effect ตอนถึงเป้าหมาย (Impact)
            if (obj != null) // เช็คอีกครั้งเผื่อ destroy ระหว่างรอ
            {
                yield return StartCoroutine(ShakeEffect(obj, 0.15f, 15f));
            }
        }
        else
        {
            yield break; // ออกจาก coroutine ถ้า obj เป็น null
        }
    }

    // 🔥 เพิ่ม: Shake effect สำหรับ Impact
    IEnumerator ShakeEffect(Transform obj, float duration, float magnitude)
    {
        Vector3 originalPos = obj.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // สั่นไหวแบบสุ่ม
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            obj.position = originalPos + new Vector3(x, y, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.position = originalPos; // คืนตำแหน่งเดิม
    }

    // 🔥 Screen shake สำหรับทั้งจอ (ใช้ตอนโดนตี/โจมตี)
    IEnumerator ScreenShake(float duration, float magnitude)
    {
        if (Camera.main == null) yield break;

        Vector3 originalPos = Camera.main.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.position = originalPos + new Vector3(x, y, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }

    public Transform GetFreeSlot(CardType type, bool isPlayer)
    {
        Transform[] slots = isPlayer 
            ? (type == CardType.Monster ? playerMonsterSlots : playerEquipSlots)
            : (type == CardType.Monster ? enemyMonsterSlots : enemyEquipSlots);

        foreach (Transform t in slots) if (t.childCount == 0) return t;
        return null;
    }

    BattleCardUI GetBestEnemyEquip(SubCategory cat)
    {
        // 🔥 เลือกโล่ตัวแรกที่มี (ไม่สนใจ subCategory)
        // OnPlayerSelectBlocker จะจัดการตรรมชาติการป้องกัน (ตรง = ทำลายทั้งคู่, ต่างกัน = ทำลายแค่โล่)
        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var s = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (s != null && s.GetData() != null) return s;
            }
        }
        return null;
    }

    bool HasEquipInSlots(Transform[] slots)
    {
        foreach (Transform t in slots) if (t.childCount > 0) return true;
        return false;
    }

    void ResetAllMonstersAttackState()
    {
        foreach (Transform slot in playerMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c) {
                    c.hasAttacked = false;
                    c.GetComponent<Image>().color = Color.white; // คืนสี
                }
            }
        }
    }

    // 🔥 รีเซ็ตสถานะมอนสเตอร์บอท (เอาไว้ใช้ตอนเริ่มเทิร์นบอท)
    void ResetAllEnemyMonstersAttackState()
    {
        foreach (Transform slot in enemyMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c) {
                    c.hasAttacked = false;
                    c.GetComponent<Image>().color = Color.white; // คืนสี
                }
            }
        }
    }

    // --- Standard Functions (แบบ Safe Mode) ---

    public void DrawCard(int n, Transform parentOverride = null) 
    { 
        StartCoroutine(DrawCardWithAnimation(n, parentOverride));
    }

    IEnumerator DrawCardWithAnimation(int n, Transform parentOverride = null)
    {
        if (deckList.Count < n)
        {
            Debug.LogWarning("⚠️ Deck empty while drawing (player)");
            StartCoroutine(EndBattle(false));
            yield break;
        }

        Transform targetParent = parentOverride != null ? parentOverride : handArea;
        
        // 🔴 Debug: เช็ค handArea และ cardPrefab
        if (!handArea) Debug.LogError("❌ handArea ยังไม่ถูกตั้งค่า!");
        if (!cardPrefab) Debug.LogError("❌ cardPrefab ยังไม่ถูกตั้งค่า!");
        if (!targetParent) Debug.LogError("❌ targetParent เป็น null!");

        for(int i=0;i<n;i++) 
        { 
            CardData d=deckList[0]; 
            deckList.RemoveAt(0); 
            
            if(targetParent && cardPrefab)
            {
                // 🎴 หาตำแหน่งเด็ค - ใช้ deckPileTransform ก่อน ถ้าไม่มีใช้ default
                Vector3 startPos = Vector3.zero;
                
                if (deckPileTransform != null)
                {
                    startPos = deckPileTransform.position;
                }
                else
                {
                    GameObject deckPos = GameObject.Find("DeckPile"); // หา object ตำแหน่งเด็ค
                    if (deckPos != null)
                        startPos = deckPos.transform.position;
                    else
                        startPos = new Vector3(-500, 0, 0); // default position
                }
                
                Debug.Log($"✅ DrawCard #{i}: {d.cardName}, startPos={startPos}, targetParent={targetParent.name}");
                
                // 🔥 สร้างที่เด็ค world position แล้ว SetParent พร้อมเก็บตำแหน่ง
                var ui = Instantiate(cardPrefab, startPos, Quaternion.identity).GetComponent<BattleCardUI>();
                
                if (ui == null)
                {
                    Debug.LogError("❌ cardPrefab ไม่มี BattleCardUI component!");
                    yield break;
                }
                
                // 🔥 ตั้ง parent พร้อมเก็บตำแหน่ง world (worldPositionStays = true)
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    ui.transform.SetParent(canvas.transform, worldPositionStays: true);
                }
                
                ui.transform.localScale = Vector3.zero; // เริ่มจากเล็ก
                ui.Setup(d);
                
                // อนิเมชั่นบินเข้ามา + ขยาย
                float duration = 0.3f;
                float elapsed = 0f;
                Vector3 endPos = targetParent.position;
                
                while (elapsed < duration && ui != null)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    
                    // Ease-out curve
                    float easeT = 1f - Mathf.Pow(1f - t, 3);
                    
                    ui.transform.position = Vector3.Lerp(startPos, endPos, easeT);
                    ui.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT);
                    
                    yield return null;
                }
                
                // Snap เข้า parent สุดท้าย และปรับตำแหน่ง
                if (ui != null)
                {
                    if (targetParent == handArea)
                    {
                        // ให้ HorizontalLayoutGroup จัดการตำแหน่ง
                        ui.transform.SetParent(targetParent, false);
                        ui.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        // เป้าหมายอื่นๆ เช่น slot ต่างๆ ยัง snap ศูนย์
                        ui.transform.SetParent(targetParent);
                        ui.transform.localPosition = Vector3.zero;
                        ui.transform.localScale = Vector3.one;
                    }
                    
                    if (isMulliganPhase) ui.SetMulliganSelect(false); // reset highlight
                }
                
                // พักเล็กน้อยระหว่างการ์ด
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                Debug.LogError($"❌ ไม่สามารถวาดการ์ดได้! targetParent={targetParent}, cardPrefab={cardPrefab}");
            }
        }
        
        // 🔥 จัดการ์ดในมือหลังจากจั่วเสร็จ (ถ้าเป็นการจั่วเข้ามือ)
        if (targetParent == handArea && !isMulliganPhase)
        {
            ArrangeCardsInHand();
            Debug.Log("✅ จัดการ์ดในมือหลังจากจั่วเสร็จ");
        }
    }

    public void DrawEnemyCard(int n) 
    { 
        if (enemyDeckList.Count < n)
        {
            Debug.LogWarning("⚠️ Deck empty while drawing (enemy)");
            StartCoroutine(EndBattle(true));
            return;
        }

        for(int i=0;i<n;i++) 
        {
            enemyDeckList.RemoveAt(0);
            if(cardBackPrefab && enemyHandArea) Instantiate(cardBackPrefab, enemyHandArea);
        }
    }

    void ShuffleList(List<CardData> list) 
    { 
        for(int i=0; i<list.Count; i++) 
        { 
            CardData t=list[i]; 
            int r=Random.Range(i,list.Count); 
            list[i]=list[r]; 
            list[r]=t; 
        } 
    }

    void PlayerTakeDamage(int d) 
    { 
        currentHP=Mathf.Max(0, currentHP-d); 
        
        // Safe Check
        if(playerSpot) ShowDamagePopupString($"-{d}", playerSpot);
        if(AudioManager.Instance)AudioManager.Instance.PlaySFX("Damage");
        StartCoroutine(ScreenShake(0.15f, 6f));
        
        UpdateUI(); 
        
        if(currentHP<=0)
        {
            Debug.Log("LOSE (HP=0)");
            StartCoroutine(EndBattle(false));
        } 
    }

    void EnemyTakeDamage(int d) 
    { 
        enemyCurrentHP=Mathf.Max(0, enemyCurrentHP-d); 
        
        if(enemySpot) ShowDamagePopupString($"-{d}", enemySpot);
        if(AudioManager.Instance)AudioManager.Instance.PlaySFX("Damage");
        StartCoroutine(ScreenShake(0.12f, 5f));
        UpdateUI();
        
        if(enemyCurrentHP<=0)
        {
            Debug.Log("WIN (enemy HP=0)");
            StartCoroutine(EndBattle(true));
        } 
    }

    IEnumerator EndBattle(bool playerWin)
    {
        if (isEnding) yield break;

        isEnding = true;
        state = playerWin ? BattleState.WON : BattleState.LOST;

        if (turnText) turnText.text = playerWin ? "VICTORY" : "DEFEAT";

        // ปิดปุ่มที่อาจค้างอยู่
        if (endTurnButton) endTurnButton.SetActive(false);
        if (takeDamageButton) takeDamageButton.SetActive(false);

        // แสดงหน้าสรุปผล ถ้ามี
        if (resultPanel)
        {
            resultPanel.SetActive(true);
            resultConfirmed = false;

            if (resultTitleText) resultTitleText.text = playerWin ? "VICTORY" : "DEFEAT";
            if (resultDetailText) resultDetailText.text = playerWin ? "คุณชนะ!" : "คุณแพ้!";

            if (resultConfirmButton)
            {
                resultConfirmButton.onClick.RemoveAllListeners();
                resultConfirmButton.onClick.AddListener(() => { resultConfirmed = true; });
            }

            // รอจนกดปุ่ม หรือถ้าไม่มีปุ่มให้รอตาม endDelay
            if (resultConfirmButton)
                yield return new WaitUntil(() => resultConfirmed);
            else
                yield return new WaitForSeconds(endDelay);
        }
        else
        {
            // ไม่มีหน้าสรุป ใช้ดีเลย์ปกติ
            yield return new WaitForSeconds(endDelay);
        }

        if (!string.IsNullOrEmpty(stageSceneName))
        {
            SceneManager.LoadScene(stageSceneName);
        }
    }

    void ShowDamagePopupString(string t, Transform pos) 
    { 
        if(damagePopupPrefab && pos) 
        {
            var go = Instantiate(damagePopupPrefab, pos.position, Quaternion.identity);
            if(go.GetComponent<DamagePopup>()) go.GetComponent<DamagePopup>().Setup(0);
        }
    }

    void UpdateUI() 
    { 
        // ใส่ ? กัน Error
        if(playerHPBar)playerHPBar.value=currentHP; 
        if(enemyHPBar)enemyHPBar.value=enemyCurrentHP; 
        if(ppText)ppText.text=$"{currentPP}/{maxPP} PP"; 
        if(enemyPPText)enemyPPText.text=$"{enemyCurrentPP}/{enemyMaxPP} PP";
        if(playerHPText)playerHPText.text=$"{currentHP}/{maxHP}"; 
        if(enemyHPText)enemyHPText.text=$"{enemyCurrentHP}/{enemyMaxHP}"; 
    }

    // --------------------------------------------------------
    // 🔄 SACRIFICE SYSTEM (ลงการ์ดใหม่ทับเก่า)
    // --------------------------------------------------------

    public void ShowSacrificeConfirmPopup(BattleCardUI newCard, BattleCardUI oldCard)
    {
        if (sacrificeConfirmPanel == null)
        {
            Debug.LogError("❌ sacrificeConfirmPanel ยังไม่ถูกตั้ง!");
            return;
        }

        newCardToSacrifice = newCard;
        targetCardToReplace = oldCard;
        sacrificeConfirmed = false;

        // คำนวณคอสต์ส่วนต่าง
        CardData newData = newCard.GetData();
        CardData oldData = oldCard.GetData();
        int costDiff = newData.cost - oldData.cost;
        int costToPay = Mathf.Max(0, costDiff); // ถ้าใบใหม่ถูกกว่า ไม่จ่ายเพิ่มและไม่คืน

        string message = $"Sacrifice {oldData.cardName} ({oldData.cost} PP)\n" +
                 $"to {newData.cardName} ({newData.cost} PP)?\n\n" +
                 $"Cost: {(costToPay > 0 ? "-" + costToPay : "0")} PP";

        if (sacrificeMessageText) sacrificeMessageText.text = message;

        // ตั้ง Listener สำหรับปุ่ม
        if (sacrificeConfirmButton)
        {
            sacrificeConfirmButton.onClick.RemoveAllListeners();
            sacrificeConfirmButton.onClick.AddListener(OnSacrificeConfirm);
        }

        if (sacrificeCancelButton)
        {
            sacrificeCancelButton.onClick.RemoveAllListeners();
            sacrificeCancelButton.onClick.AddListener(OnSacrificeCancel);
        }

        // เปิด panel
        sacrificeConfirmPanel.SetActive(true);
        Debug.Log($"🔄 เปิด Sacrifice Popup: {oldData.cardName} → {newData.cardName}");
    }

    void OnSacrificeConfirm()
    {
        if (newCardToSacrifice == null || targetCardToReplace == null)
        {
            Debug.LogWarning("⚠️ Sacrifice Card หรือ Target Card เป็น null");
            OnSacrificeCancel();
            return;
        }

        CardData newData = newCardToSacrifice.GetData();
        CardData oldData = targetCardToReplace.GetData();
        int costDiff = newData.cost - oldData.cost;
        int costToPay = Mathf.Max(0, costDiff);

        // เช็ค PP ว่าเพียงพอ (เฉพาะเมื่อต้องจ่าย)
        if (costToPay > 0 && currentPP < costToPay)
        {
            Debug.Log($"⚠️ PP ไม่พอ ({currentPP}/{costToPay})");
            if (sacrificeMessageText) 
                sacrificeMessageText.text = $"PP ไม่พอ! ต้องการ {costToPay} PP แต่มีแค่ {currentPP} PP";
            return;
        }

        // บังคับปิด popup ก่อนทำ sacrifice logic
        sacrificeConfirmPanel.SetActive(false);

        // ทำการ Sacrifice
        PerformSacrifice(newCardToSacrifice, targetCardToReplace, costToPay);

        // ล้างตัวแปร
        newCardToSacrifice = null;
        targetCardToReplace = null;
    }

    void OnSacrificeCancel()
    {
        sacrificeConfirmPanel.SetActive(false);
        newCardToSacrifice = null;
        targetCardToReplace = null;
        Debug.Log("❌ ยกเลิก Sacrifice");
    }

    void PerformSacrifice(BattleCardUI newCard, BattleCardUI oldCard, int costToPay)
    {
        CardData newData = newCard.GetData();
        CardData oldData = oldCard.GetData();

        // จ่าย PP เฉพาะส่วนที่ต้องจ่าย (ไม่คืนกรณีถูกกว่า)
        currentPP -= costToPay;
        Debug.Log($"🔄 Sacrifice: {oldData.cardName} → {newData.cardName}, Cost To Pay: {costToPay}, PP: {currentPP}");

        // ย้ายการ์ดใหม่ไปยังช่องของการ์ดเก่า
        Transform oldCardSlot = oldCard.transform.parent;
        newCard.transform.SetParent(oldCardSlot);
        newCard.transform.localPosition = Vector3.zero;
        newCard.isOnField = true;
        newCard.hasAttacked = true; // ลงแบบสังเวยต้องรอเทิร์นถัดไปถึงจะตีได้
        newCard.GetComponent<Image>().color = Color.white; // ไม่เป็นสีเทา

        // ทำลายการ์ดเก่า
        Destroy(oldCard.gameObject);

        // เล่นเสียง
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");

        UpdateUI();
        Debug.Log($"✅ Sacrifice สำเร็จ!");
    }
}

