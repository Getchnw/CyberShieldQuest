using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, DEFENDER_CHOICE, FORCED_DISCARD, WON, LOST }

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

    [Header("--- Pause & Log ---")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button quitBattleButton;
    public Button logButton;
    public Button logCloseButton;
    public GameObject logPanel;
    public TextMeshProUGUI logText;
    public ScrollRect logScrollRect; // ScrollRect สำหรับเลื่อน Log

    [Header("--- Log Panel Style ---")]
    public bool autoStyleLogPanel = true;
    [Range(0f, 1f)] public float logPanelOpacity = 0.35f; // ความโปร่งใสของพื้นหลัง
    public Vector2 logPanelMargin = new Vector2(48f, 48f); // ระยะขอบรอบ ๆ ให้พอดีจอ

    [Header("--- Card Detail View ---")]
    public CardDetailView cardDetailView;

    [Header("--- Sacrifice Confirm Popup ---")]
    public GameObject sacrificeConfirmPanel; // Panel ยืนยันการ sacrifice
    public TextMeshProUGUI sacrificeMessageText; // ข้อความอธิบาย
    public Button sacrificeConfirmButton; // ปุ่มยืนยัน
    public Button sacrificeCancelButton; // ปุ่มยกเลิก

    [Header("--- Deck Position ---")]
    public Transform deckPileTransform; // ตำแหน่งเด็คที่การ์ดจะบินออกมา
    public Transform enemyDeckPileTransform; // ตำแหน่งเด็คบอทที่การ์ดจะบินออกมา
    public TextMeshProUGUI playerDeckCountText; // แสดงจำนวนการ์ดที่เหลือในเด็คผู้เล่น
    public TextMeshProUGUI enemyDeckCountText; // แสดงจำนวนการ์ดที่เหลือในเด็คบอท
    [Range(3, 10)] public int deckVisualizationCount = 5; // จำนวนหลังการ์ดที่ซ้อนกัน

    [Header("--- Graveyard UI ---")]
    public Transform playerGraveyardArea;          // จุดวาง/โชว์การ์ดสุสานผู้เล่น (เลือกใส่ได้)
    public Transform enemyGraveyardArea;           // จุดวาง/โชว์การ์ดสุสานบอท (เลือกใส่ได้)
    public TextMeshProUGUI playerGraveyardCountText; // UI นับจำนวนการ์ดสุสานผู้เล่น
    public TextMeshProUGUI enemyGraveyardCountText;  // UI นับจำนวนการ์ดสุสานบอท
    public GameObject playerGraveyardPanel;        // Popup รายการสุสานผู้เล่น (ถ้ามี)
    public GameObject enemyGraveyardPanel;         // Popup รายการสุสานบอท (ถ้ามี)
    public Transform playerGraveyardListRoot;      // Root สำหรับ spawn item สุสานผู้เล่น
    public Transform enemyGraveyardListRoot;       // Root สำหรับ spawn item สุสานบอท
    public GameObject graveyardListItemPrefab;     // Prefab รายการสุสาน (ถ้ามี)

    [Header("--- Target Selection UI (สำหรับ Spell) ---")]
    public GameObject targetSelectionPanel; // Panel ให้เลือกเป้าหมาย
    public TextMeshProUGUI targetSelectionText; // ข้อความอธิบาย "เลือกเป้าหมาย"
    public Button targetSelectionCancelButton; // ปุ่มยกเลิกการเลือก

    [Header("--- Hand Reveal Panel (ดูการ์ดบนมือ) ---")]
    public GameObject handRevealPanel; // Panel แสดงการ์ดบนมือที่ดูได้
    public Transform handRevealListRoot; // Root สำหรับ spawn การ์ดที่ดู
    public TextMeshProUGUI handRevealTitleText; // ชื่อ Panel (เช่น "การ์ดบนมือฝ่ายตรงข้าม")
    public Button handRevealCloseButton; // ปุ่มปิด

    [Header("--- Force Choose Discard Panel (บังคับให้เลือกทิ้งการ์ด) ---")]
    public GameObject forceDiscardPanel; // Panel หลักให้เลือกการ์ดทิ้ง
    public Transform forceDiscardListRoot; // Root สำหรับ spawn การ์ดบนมือที่เลือกได้
    public TextMeshProUGUI forceDiscardTitleText; // ข้อความหัวข้อ (เช่น "เลือกการ์ดที่จะทิ้ง")
    public TextMeshProUGUI forceDiscardCountText; // แสดงจำนวนที่ต้องเลือก (เช่น "0/2")
    public Button forceDiscardConfirmButton; // ปุ่มยืนยันการทิ้ง

    [Header("--- Mulligan UI ---")]
    public GameObject muliganPanel; // Panel หลักของ mulligan
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
    public Vector2 handCardPreferredSize = new Vector2(100f, 140f);

    private bool isEnding = false;
    private bool resultConfirmed = false;
    private bool isMulliganPhase = false;

    public bool IsMulliganPhase() => isMulliganPhase;

    // --- ตัวแปร Logic ภายใน ---
    private BattleCardUI currentAttackerBot;
    private bool playerHasMadeChoice = false;
    private List<CardData> enemyDeckList = new List<CardData>();
    private Dictionary<string, CardData> revealedEnemyCards = new Dictionary<string, CardData>(); // เก็บการ์ดที่ทัยเหงแล้วท่อแคส
    private Dictionary<string, CardData> cardLookupCache = null;
    private Dictionary<string, CardData> cardNameLookupCache = null;
    private int lastDestroyedAtkSum = 0;

    // 🔥 Sacrifice System
    private bool sacrificeConfirmed = false;
    private BattleCardUI newCardToSacrifice = null;
    private BattleCardUI targetCardToReplace = null;

    // 🔥 Force Choose Discard System
    private bool isChoosingDiscard = false;
    private List<BattleCardUI> selectedCardsToDiscard = new List<BattleCardUI>();
    private int requiredDiscardCount = 0;
    private bool discardConfirmed = false;

    // 🔥 Peek & Choose Discard From Deck System
    private bool isChoosingPeekDiscard = false;
    private bool peekDiscardConfirmed = false;
    private CardData selectedPeekDiscardCard = null;
    private bool isPeekDiscardMultiSelectMode = false;
    private int requiredPeekDiscardCount = 0;
    private List<CardData> selectedPeekDiscardCards = new List<CardData>();
    private List<int> selectedPeekDiscardIndices = new List<int>();
    private List<CardData> currentPeekCards = new List<CardData>();
    private List<CardData> currentPeekSelectableCards = new List<CardData>();
    private CardEffect currentPeekEffect;
    private bool hasCurrentPeekEffect = false;

    // 🔥 Return Equip From Graveyard System
    private bool cardAdditionInProgress = false;
    private bool cardAdditionComplete = false;
    private bool isChoosingGraveyardEquip = false;
    private CardData selectedGraveyardEquip = null;
    private bool graveyardEquipConfirmed = false;

    // 🔥 Mulligan System
    private int playerMulliganLeft = 1;
    private int enemyMulliganLeft = 1;
    private bool playerFirstTurn = false; // true = ผู้เล่นเริ่มต้น

    // 🎴 Deck Visualization
    private List<GameObject> playerDeckVisuals = new List<GameObject>();
    private List<GameObject> enemyDeckVisuals = new List<GameObject>();

    // 🪦 Graveyard System (เก็บการ์ดที่ถูกทำลาย/discard)
    private List<CardData> playerGraveyard = new List<CardData>();
    private List<CardData> enemyGraveyard = new List<CardData>();

    // 🎯 Target Selection System (สำหรับ Spell ที่ต้องเลือกเป้าหมาย)
    private bool isSelectingTarget = false;
    private List<BattleCardUI> availableTargets = new List<BattleCardUI>();
    private List<BattleCardUI> selectedTargets = new List<BattleCardUI>();
    private System.Action<List<BattleCardUI>> onTargetSelected = null;

    // 🔔 Battle Log
    private readonly List<string> battleLog = new List<string>();
    private const int battleLogLimit = 200;

    // 📊 Battle Statistics Tracking
    public BattleStatistics currentBattleStats = new BattleStatistics();
    public static BattleStatistics LastBattleStats { get; private set; } // เก็บสถิติเกมล่าสุดสำหรับเข้าถึงจากภายนอก

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

        // ผูกปุ่ม Pause / Resume / Quit / Log ถ้าตั้งใน Inspector
        if (pauseButton)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPausePressed);
        }

        if (resumeButton)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumePressed);
        }

        if (quitBattleButton)
        {
            quitBattleButton.onClick.RemoveAllListeners();
            quitBattleButton.onClick.AddListener(OnQuitBattlePressed);
        }

        if (logButton)
        {
            logButton.onClick.RemoveAllListeners();
            logButton.onClick.AddListener(OnToggleLogPanel);
        }

        // ผูกปุ่มปิด Log Panel
        if (logCloseButton)
        {
            logCloseButton.onClick.RemoveAllListeners();
            logCloseButton.onClick.AddListener(OnToggleLogPanel);
        }

        // ผูกปุ่มปิด Hand Reveal Panel
        if (handRevealCloseButton)
        {
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(CloseHandRevealPanel);
        }
    }

    void Start()
    {
        state = BattleState.START;

        // 📊 เริ่มต้นสถิติใหม่
        currentBattleStats.Initialize();

        // ตั้งค่าพาเนล pause/log/handreveal ให้ปิดไว้ก่อน
        if (pausePanel) pausePanel.SetActive(false);
        if (logPanel) logPanel.SetActive(false);
        if (handRevealPanel) handRevealPanel.SetActive(false);
        SetupLogPanelAppearance();
        UpdateLogText();
        
        // 👁️ เปิด raycasts บน enemyHandArea เพื่อให้การ์ดที่ reveal คลิกได้
        if (enemyHandArea != null)
        {
            var cg = enemyHandArea.GetComponent<CanvasGroup>();
            if (cg == null) cg = enemyHandArea.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            Debug.Log("✅ Set enemyHandArea.CanvasGroup.blocksRaycasts = true");
        }
        
        StartCoroutine(SetupBattle());
    }

    void Update()
    {
        // กด ESC เพื่อสลับ Pause/Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null && pausePanel.activeSelf)
            {
                // หากเปิด Pause อยู่ ให้ Resume และปิด Log/Graveyard
                OnResumePressed();
                CloseAllGraveyardPanels();
            }
            else
            {
                // หากยังไม่ Pause ให้เปิด Pause
                OnPausePressed();
            }
        }
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

        // 🎴 สร้างการแสดงผลเด็ค
        CreateDeckVisualization();

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
            StartCoroutine(DrawEnemyCard(4));

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
        yield return StartCoroutine(DrawEnemyCard(4));

        // 5. Mulligan Phase (ผู้เล่นเลือกก่อนเสมอ)
        yield return StartCoroutine(PlayerMulliganPhase());

        // 🔥 DEBUG: ตรวจสอบว่าหลัง Mulligan บอทเหลือกี่ใบ
        int enemyHandBeforeTurn = enemyHandArea != null ? enemyHandArea.childCount : 0;
        Debug.Log($"🤖 [SETUP DONE] บอทเหลือการ์ด {enemyHandBeforeTurn} ใบก่อนเทิร์นแรก");

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

        for (int i = 0; i < n && slotIndex < slots.Length && cardsDrawn < n; i++)
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

            if (cardPrefab)
            {
                // 🔥 สร้างการ์ดโดยตรงใน targetSlot (ไม่แสดงที่ deck position)
                GameObject cardObj = Instantiate(cardPrefab, targetSlot);
                BattleCardUI ui = cardObj.GetComponent<BattleCardUI>();
                if (ui == null) continue;

                ui.Setup(d);
                ui.parentAfterDrag = targetSlot;
                cardObj.transform.localPosition = Vector3.zero;
                cardObj.transform.localScale = Vector3.one;

                Debug.Log($"✅ {ui.name} เข้า slot โดยตรง!");

                // พักระหว่างการ์ด
                yield return new WaitForSeconds(0.5f);
                slotIndex++;
            }
        }

        // 🎴 อัพเดทการแสดงผลเด็ค
        UpdateDeckVisualization();
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
            playerMulliganConfirmButton.onClick.AddListener(OnPlayerMulliganConfirm);
        }
    }

    void HidePlayerMulliganUI()
    {
        // 🔥 ปิด Panel หลักของ mulligan
        if (muliganPanel != null)
        {
            muliganPanel.SetActive(false);
            Debug.Log("✅ ปิด muliganPanel");
        }

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

        try
        {
            // 🔥 Get ONLY direct children (not nested)
            var cardsInHand = handArea.GetComponentsInChildren<BattleCardUI>(includeInactive: false)
                .Where(c => c.transform.parent == handArea).ToArray();
            
            if (cardsInHand.Length == 0)
            {
                Debug.Log("⚠️ ไม่มีการ์ดในมือ");
                return;
            }

            // 🔥 Filter out null/destroyed cards
            cardsInHand = System.Array.FindAll(cardsInHand, c => c != null && c.gameObject != null);
            if (cardsInHand.Length == 0)
            {
                Debug.LogWarning("⚠️ ArrangeCardsInHand: No valid cards after null-check");
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
                    if (card == null || card.gameObject == null) continue;
                    
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
                    // 🟣 ตั้งสีขาว (ยกเว้นถ้าการ์ดสูญเสีย category)
                    if (img && !card.hasLostCategory) img.color = Color.white;

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
                if (card == null || card.gameObject == null) continue;
                
                if (card.transform.parent != handArea)
                    card.transform.SetParent(handArea, false);

                float xPos = startX + count * (cardWidth + spacing);
                card.transform.localPosition = new Vector3(xPos, 0, 0);
                card.transform.localScale = Vector3.one;
                card.transform.localRotation = Quaternion.identity;

                var img = card.GetComponent<Image>();
                // 🟣 ตั้งสีขาว (ยกเว้นถ้าการ์ดสูญเสีย category)
                if (img && !card.hasLostCategory) img.color = Color.white;
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
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ EXCEPTION in ArrangeCardsInHand: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // 🔥 จัดมือบอทให้มองเห็นได้ (เหมือนผู้เล่น แต่ปิดการโต้ตอบ)
    void ArrangeEnemyHand()
    {
        if (enemyHandArea == null) return;

        try
        {
            // 🔥 Get ONLY direct children (not nested)
            var cards = enemyHandArea.GetComponentsInChildren<BattleCardUI>(includeInactive: false)
                .Where(c => c.transform.parent == enemyHandArea).ToArray();
            
            if (cards.Length == 0)
            {
                Debug.Log("🎴 ArrangeEnemyHand: No cards to arrange");
                return;
            }

            // 🔥 Filter out null/destroyed cards
            cards = System.Array.FindAll(cards, c => c != null && c.gameObject != null);
            if (cards.Length == 0)
            {
                Debug.LogWarning("⚠️ ArrangeEnemyHand: No valid cards after null-check");
                return;
            }

            Debug.Log($"🎴 ArrangeEnemyHand: {cards.Length} valid direct children found");

            var layout = enemyHandArea.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            if (useHandLayoutGroup && layout != null)
            {
                layout.enabled = true;
                layout.spacing = handSpacing;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                foreach (var card in cards)
                {
                    if (card == null || card.gameObject == null) continue;
                    
                    if (card.transform.parent != enemyHandArea)
                        card.transform.SetParent(enemyHandArea, false);

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

                    var img = card.GetComponent<Image>();
                    // 🟣 ตั้งสีขาว (ยกเว้นถ้าการ์ดสูญเสีย category)
                    if (img && !card.hasLostCategory) img.color = Color.white;

                    var cg = card.GetComponent<CanvasGroup>();
                    if (cg)
                    {
                        bool allowClick = IsCardRevealed(card.GetData());
                        cg.interactable = allowClick;
                        cg.blocksRaycasts = allowClick;
                        cg.alpha = 1f;
                    }
                }

                var rect = enemyHandArea as RectTransform;
                if (rect)
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                    Canvas.ForceUpdateCanvases();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ EXCEPTION in ArrangeEnemyHand: {ex.Message}\n{ex.StackTrace}");
        }
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
            AddBattleLog("Player deck empty - LOSE");
            StartCoroutine(EndBattle(false));
            return;
        }

        state = BattleState.PLAYERTURN;
        turnCount++;

        // 📊 บันทึกสถิติ: จำนวนเทิร์น
        currentBattleStats.turnsPlayed = turnCount;

        maxPP = Mathf.Clamp(turnCount, 1, 10);
        currentPP = maxPP;

        ResetAllMonstersAttackState();

        if (turnText) turnText.text = "YOUR TURN";
        if (endTurnButton) endTurnButton.SetActive(true);
        if (takeDamageButton) takeDamageButton.SetActive(false);

        // กฎจั่ว: ถ้ามือ >= 5 จั่ว 1, ถ้ามือน้อยกว่า 5 จั่วให้ครบ 5
        int handCount = handArea != null ? handArea.GetComponentsInChildren<BattleCardUI>().Length : 0;
        int drawAmount = handCount >= 5 ? 1 : Mathf.Max(0, 5 - handCount);

        AddBattleLog($"\n=== PLAYER TURN {turnCount} START === HP:{currentHP}/{maxHP} | PP:{currentPP}/{maxPP} | Draw:{drawAmount}");
        DrawCard(drawAmount);
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

        Debug.Log($"🎴 โหลดเด็คที่เลือก: index {selectedIndex} (PlayerPrefs 'SelectedDeckIndex')");

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
                Debug.Log($"🎴 LoadDeck: id={id}, cardName={card.cardName}, atk={card.atk}, hp={card.hp}");
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

    CardData GetCardDataById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (cardLookupCache == null || cardLookupCache.Count == 0)
        {
            CardData[] allCards = Resources.LoadAll<CardData>("GameContent/Cards");
            if (allCards == null || allCards.Length == 0)
            {
                Debug.LogWarning("⚠️ ไม่พบการ์ดใน Resources/GameContent/Cards สำหรับ lookup");
                return null;
            }

            cardLookupCache = allCards.ToDictionary(c => c.card_id, c => c);
            cardNameLookupCache = allCards
                .Where(c => !string.IsNullOrEmpty(c.cardName))
                .ToDictionary(c => c.cardName, c => c);
        }

        if (cardLookupCache.TryGetValue(id, out CardData card))
        {
            return card;
        }

        return null;
    }

    CardData GetCardDataByName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName)) return null;

        if (cardNameLookupCache == null || cardNameLookupCache.Count == 0)
        {
            CardData[] allCards = Resources.LoadAll<CardData>("GameContent/Cards");
            if (allCards == null || allCards.Length == 0)
            {
                Debug.LogWarning("⚠️ ไม่พบการ์ดใน Resources/GameContent/Cards สำหรับ lookup");
                return null;
            }

            cardLookupCache = allCards.ToDictionary(c => c.card_id, c => c);
            cardNameLookupCache = allCards
                .Where(c => !string.IsNullOrEmpty(c.cardName))
                .ToDictionary(c => c.cardName, c => c);
        }

        if (cardNameLookupCache.TryGetValue(cardName, out CardData card))
        {
            return card;
        }

        return null;
    }

    CardData ResolveCardData(CardData data)
    {
        if (data == null) return null;

        // ถ้า stats เป็น 0 ให้ลองดึงจาก Resources ด้วย card_id
        if (data.atk == 0 && data.hp == 0)
        {
            Debug.LogWarning($"⚠️ ResolveCardData: stats=0 for cardName={data.cardName}, card_id={data.card_id}");

            if (!string.IsNullOrEmpty(data.card_id))
            {
                var resolved = GetCardDataById(data.card_id);
                if (resolved != null) return resolved;
            }

            if (!string.IsNullOrEmpty(data.cardName))
            {
                var resolvedByName = GetCardDataByName(data.cardName);
                if (resolvedByName != null) return resolvedByName;
            }
        }

        return data;
    }

    CardData ResolveCardData(BattleCardUI ui)
    {
        if (ui == null) return null;
        return ResolveCardData(ui.GetData());
    }

    public void OnEndTurnButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        if (endTurnButton) endTurnButton.SetActive(false);
        StartCoroutine(ProcessPlayerEndTurn());
    }

    IEnumerator ProcessPlayerEndTurn()
    {
        // 🎯 ทริกเกอร์เอฟเฟกต์ตอนจบเทิร์นของการ์ดฝั่งผู้เล่น
        yield return StartCoroutine(ResolveTurnEndEffectsForSide(isPlayerSide: true));

        if (isEnding || state == BattleState.WON || state == BattleState.LOST) yield break;

        // 🎮 นับ Control duration ตอนจบเทิร์นผู้เล่น
        ProcessControlDurationsForAllEquips();
        StartCoroutine(EnemyTurn());
    }

    IEnumerator ResolveTurnEndEffectsForSide(bool isPlayerSide)
    {
        Transform[] monsterSlots = isPlayerSide ? playerMonsterSlots : enemyMonsterSlots;
        Transform[] equipSlots = isPlayerSide ? playerEquipSlots : enemyEquipSlots;

        List<BattleCardUI> cardsOnField = new List<BattleCardUI>();
        CollectFieldCards(monsterSlots, cardsOnField);
        CollectFieldCards(equipSlots, cardsOnField);

        foreach (BattleCardUI card in cardsOnField)
        {
            if (card == null || card.gameObject == null) continue;
            if (card.GetData() == null) continue;

            yield return StartCoroutine(ResolveEffects(card, EffectTrigger.OnTurnEnd, isPlayer: isPlayerSide));

            if (isEnding || state == BattleState.WON || state == BattleState.LOST)
            {
                yield break;
            }
        }
    }

    void CollectFieldCards(Transform[] slots, List<BattleCardUI> output)
    {
        if (slots == null || output == null) return;

        foreach (Transform slot in slots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI card = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (card != null)
            {
                output.Add(card);
            }
        }
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
        if (freeSlot != null) StartCoroutine(PayCostAndSummon(cardUI, freeSlot, data.cost));
    }

    public void TrySummonCard(BattleCardUI cardUI, CardSlot targetSlot)
    {
        if (isMulliganPhase) return;
        if (state != BattleState.PLAYERTURN) return;

        CardData data = cardUI.GetData();

        // Spell ไม่ควรถูกลากลงสนาม ใช้ได้เฉพาะกดเล่นจากมือ
        if (data.type == CardType.Spell) return;

        CardType summonType = data.type == CardType.Token ? CardType.Monster : data.type;
        if (summonType != targetSlot.allowedType) return;
        if (targetSlot.transform.childCount > 0) return;
        if (currentPP < data.cost) return;

        StartCoroutine(PayCostAndSummon(cardUI, targetSlot.transform, data.cost));
    }

    IEnumerator PayCostAndSummon(BattleCardUI cardUI, Transform parentSlot, int cost)
    {
        currentPP -= cost;

        // 📊 บันทึกสถิติ: PP ใช้ไป + การ์ดที่เล่น
        currentBattleStats.totalPPSpent += cost;
        currentBattleStats.RecordCardPlayed(cardUI.GetData());

        cardUI.transform.SetParent(parentSlot);
        cardUI.transform.localPosition = Vector3.zero;

        cardUI.isOnField = true;
        // 🔥 EquipSpell ไม่มี Summoning Sickness
        if (cardUI.GetData().type != CardType.EquipSpell)
        {
            cardUI.hasAttacked = true; // Monster ต้องรอเทิร์นถัดไป
            // 🔥 เช็คว่ามีสกิล Rush หรือไม่ (โจมตีได้ทันที)
            bool hasRush = HasActiveRush(cardUI);
            if (hasRush)
            {
                cardUI.hasAttacked = false; // สามารถโจมตีได้ทันทีในเทิร์นนี้
                AddBattleLog($"💨 <color=cyan>{cardUI.GetData().cardName}</color> มีสกิล Rush! สามารถโจมตีได้ทันที");
            }
        }
        cardUI.UpdateCardSize(); // 🔥 ปรับขนาดการ์ดบนสนาม 

        // 🔥 แก้: ตรวจสอบให้แน่ใจว่าการ์ดแสดงหน้าไม่ใช่หลัง
        var cardImage = cardUI.GetComponent<Image>();
        if (cardImage != null && cardUI.GetData() != null && cardUI.GetData().artwork != null)
        {
            cardImage.sprite = cardUI.GetData().artwork; // แสดงหน้าการ์ด
            // 🔥 EquipSpell ไม่มืด
            cardImage.color = (cardUI.GetData().type == CardType.EquipSpell) ? Color.white : Color.gray;
        }
        // แสดงกรอบเมื่อการ์ดหงายหน้า
        cardUI.SetFrameVisible(true);

        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");

        // บันทึก log พร้อมแสดง ATK/HP
        CardData playedCard = cardUI.GetData();
        AddBattleLog($"Player plays {playedCard.cardName} ({playedCard.type}) ATK:{playedCard.atk} HP:{playedCard.hp} cost {cost}");
        Debug.Log($"🔥 PlayCard Debug: cardName={playedCard.cardName}, atk={playedCard.atk}, hp={playedCard.hp}");

        // 🔥 ทริกเกอร์ OnDeploy Effects (รอให้เสร็จก่อนไป)
        yield return StartCoroutine(ResolveEffects(cardUI, EffectTrigger.OnDeploy, isPlayer: true));

        UpdateUI();
    }

    // ใช้การ์ดเวทย์แล้วทิ้ง (รองรับเอฟเฟกต์เวทย์)
    void CastSpellCard(BattleCardUI cardUI)
    {
        if (cardUI == null || cardUI.GetData() == null) return;

        // 🔍 ตรวจสอบว่าเวทย์นี้สามารถใช้ได้หรือไม่
        if (!CanCastSpell(cardUI.GetData(), isPlayer: true))
        {
            Debug.Log($"⚠️ ไม่สามารถใช้เวทย์ได้: {cardUI.GetData().cardName} (ไม่มีเป้าหมายที่เหมาะสม)");
            if (AudioManager.Instance) AudioManager.Instance.PlaySFX("Denied");
            return; // ยกเลิกการใช้เวทย์
        }

        currentPP -= cardUI.GetCost();

        // 📊 บันทึกสถิติ: PP ใช้ไป + การ์ดที่เล่น + Spell Cast
        currentBattleStats.totalPPSpent += cardUI.GetCost();
        currentBattleStats.RecordCardPlayed(cardUI.GetData());
        currentBattleStats.spellsCast++;

        AddBattleLog($"Player casts {cardUI.GetData().cardName}");

        // 🎇 ลงสนามการ์ดเวทย์ก่อน (แสดงให้เห็นบนสนาม)
        StartCoroutine(PlaySpellCardAnimation(cardUI, isPlayer: true));
    }

    /// <summary>ตรวจสอบว่าเวทย์สามารถใช้ได้หรือไม่ (ต้องมีเป้าหมายที่เหมาะสม)</summary>
    bool CanCastSpell(CardData spellData, bool isPlayer)
    {
        if (spellData == null || spellData.effects == null) return true; // ไม่มี effect ถือว่า OK

        // ตรวจสอบแต่ละ effect ว่า มีเป้าหมายหรือไม่
        foreach (var effect in spellData.effects)
        {
            if (effect.trigger != EffectTrigger.OnDeploy) continue;

            switch (effect.action)
            {
                case ActionType.Destroy:
                case ActionType.ModifyStat:
                case ActionType.ZeroStats:
                    // ต้องมีเป้าหมาย (กำลังโจมตี)
                    List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);
                    if (targets.Count == 0)
                    {
                        Debug.Log($"🚫 Effect {effect.action} ไม่มีเป้าหมาย!");
                        return false;
                    }
                    break;

                case ActionType.HealHP:
                    // HealHP ใช้ได้เสมอ (ไม่ต้องมีเป้าหมายอื่น)
                    break;

                case ActionType.DiscardDeck:
                case ActionType.RevealHand:
                case ActionType.RevealHandMultiple:
                case ActionType.PeekDiscardTopDeck:
                    // เทพอกพ/ดูมือ is ok ก็ว่า ok (ต้องการ discard/reveal ได้)
                    break;

                case ActionType.BypassIntercept:
                    // กรณีเวทย์เลือกมอนสเตอร์ฝ่ายเรา ต้องมีเป้าหมายให้เลือก
                    if (spellData.type != CardType.Monster
                        && effect.targetType == TargetType.Self
                        && effect.targetCardTypeFilter == EffectCardTypeFilter.Monster)
                    {
                        List<BattleCardUI> bypassTargets = GetTargetCards(effect, isPlayer);
                        int monsterTargetCount = bypassTargets.Count(t => t != null && t.GetData() != null && t.GetData().type == CardType.Monster);
                        if (monsterTargetCount == 0)
                        {
                            Debug.Log($"🚫 Effect {effect.action} ไม่มีมอนสเตอร์ฝั่งตัวเองให้เลือก!");
                            return false;
                        }
                    }
                    break;

                case ActionType.ReturnEquipFromGraveyard:
                    if (!HasEquipInGraveyard(isPlayer))
                    {
                        Debug.Log($"🚫 Effect {effect.action} ไม่มี Equip Spell ในสุสาน!");
                        return false;
                    }
                    break;

                    // effect อื่นๆ ถือว่า OK
            }
        }

        return true;
    }

    /// <summary>เล่นอนิเมชั่นการ์ดเวทย์และเอฟเฟกต์</summary>
    IEnumerator PlaySpellCardAnimation(BattleCardUI cardUI, bool isPlayer)
    {
        CardData spellData = cardUI.GetData();

        // 🎇 แสดงแจ้งเตือนเวทย์
        StartCoroutine(ShowSpellUsageNotification(spellData, isPlayer));

        // โชว์การ์ดบนสนามสักครู่ (เหมือนลงสนาม)
        Canvas canvas = FindObjectOfType<Canvas>();
        Vector3 originalPos = cardUI.transform.position;
        Vector3 targetPos = isPlayer ? playerSpot.position : enemySpot.position;

        if (canvas != null)
        {
            cardUI.transform.SetParent(canvas.transform, worldPositionStays: true);
        }

        // บิน ไปตรงกลาง
        float animDuration = 0.4f;
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            cardUI.transform.position = Vector3.Lerp(originalPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 🔥 ทริกเกอร์ OnDeploy Effects สำหรับเวทย์ (รอให้เสร็จก่อน)
        yield return StartCoroutine(ResolveEffects(cardUI, EffectTrigger.OnDeploy, isPlayer));

        yield return new WaitForSeconds(0.2f);

        // 🪦 ส่งเวทย์ลงสุสาน (Spell ใช้ไปแล้ว)
        SendToGraveyard(spellData, isPlayer);
        Destroy(cardUI.gameObject);
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");
        UpdateUI();
    }

    /// <summary>บอทใช้เวทย์ พร้อมแสดงภาพและเอฟเฟกต์</summary>
    IEnumerator BotCastSpell(BattleCardUI spellCard)
    {
        if (spellCard == null || spellCard.GetData() == null) yield break;

        CardData spellData = spellCard.GetData();

        // 🔍 ตรวจสอบว่าเวทย์นี้สามารถใช้ได้หรือไม่
        if (!CanCastSpell(spellData, isPlayer: false))
        {
            Debug.Log($"⚠️ บอทไม่สามารถใช้เวทย์: {spellData.cardName} (ไม่มีเป้าหมาย)");
            Destroy(spellCard.gameObject);
            yield break; // ยกเลิก
        }

        Debug.Log($"🎇 บอทใช้เวทย์: {spellData.cardName}");
        AddBattleLog($"Bot casts {spellData.cardName}");

        // 🎇 ลงสนามการ์ดเวทย์ก่อน
        Canvas canvas = FindObjectOfType<Canvas>();
        Vector3 originalPos = spellCard.transform.position;
        Vector3 targetPos = enemySpot.position;

        if (canvas != null)
        {
            spellCard.transform.SetParent(canvas.transform, worldPositionStays: true);
        }

        // แสดงแจ้งเตือนเวทย์
        StartCoroutine(ShowSpellUsageNotification(spellData, isPlayer: false));

        // บิน ไปตรงกลาง
        float animDuration = 0.4f;
        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            spellCard.transform.position = Vector3.Lerp(originalPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 🔥 ทริกเกอร์เอฟเฟกต์ (รอให้เสร็จก่อน)
        Debug.Log($"🔵 BOT SPELL: Starting ResolveEffects...");
        yield return StartCoroutine(ResolveEffects(spellCard, EffectTrigger.OnDeploy, isPlayer: false));
        Debug.Log($"🔵 BOT SPELL: ResolveEffects COMPLETED!");

        yield return new WaitForSeconds(0.2f);

        // 🪦 ส่งเวทย์ลงสุสาน (Spell ใช้ไปแล้ว)
        if (spellCard != null && spellCard.gameObject != null)
        {
            SendToGraveyard(spellData, isPlayer: false);
            Destroy(spellCard.gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ spellCard already destroyed after ResolveEffects");
            if (spellData != null)
                SendToGraveyard(spellData, isPlayer: false);
        }

        yield return new WaitForSeconds(0.2f);

        // ปล่อยเวทย์ลงไปสุสาน
        if (spellCard != null && spellCard.gameObject != null)
        {
            Destroy(spellCard.gameObject);
        }
        
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");
        Debug.Log($"🔵 BOT SPELL: Calling UpdateUI()...");
        UpdateUI();
        Debug.Log($"🔵 BOT SPELL: COMPLETELY FINISHED!");
    }

    /// <summary>แสดงแจ้งเตือนเวทย์ที่ถูกใช้</summary>
    IEnumerator ShowSpellUsageNotification(CardData spellData, bool isPlayer)
    {
        // สร้างกล่องแสดงเวทย์แบบ popup
        if (cardDetailView != null)
        {
            cardDetailView.Open(spellData);

            string casterName = isPlayer ? "คุณ" : "บอท";
            string spellMsg = $"🎇 {casterName} ใช้เวทย์: {spellData.cardName}";
            Debug.Log(spellMsg);

            // แสดง popup 2-3 วินาที
            yield return new WaitForSeconds(2f);

            // ปิด detail view โดยการคลิกอื่น หรือให้มันปิดเองตามระยะเวลา
            // (ถ้า cardDetailView มีปุ่มปิด)
        }
    }

    // --------------------------------------------------------
    // ⚔️ PLAYER ATTACK
    // --------------------------------------------------------

    public void OnPlayerAttack(BattleCardUI attacker)
    {
        if (state != BattleState.PLAYERTURN) return;

        attacker.attacksThisTurn++;

        // ถ้าโจมตีครบครั้งแล้ว ให้ปิดการโจมตี
        if (attacker.attacksThisTurn >= attacker.GetMaxAttacksPerTurn())
        {
            attacker.hasAttacked = true;
        }

        // 🟣 เปลี่ยนสีเป็นเทาหลังโจมตี (ยกเว้นถ้าสูญเสีย category)
        if (!attacker.hasLostCategory)
        {
            attacker.GetComponent<Image>().color = Color.gray;
        }

        int attackDamage = attacker.GetModifiedATK(isPlayerAttack: true); // 🔥 ใช้ ModifiedATK แทน
        AddBattleLog($"Player attacks with {attacker.GetData().cardName} (ATK:{attackDamage}) [{attacker.attacksThisTurn}/{attacker.GetMaxAttacksPerTurn()}]");

        StartCoroutine(ProcessPlayerAttack(attacker));
    }

    IEnumerator ProcessPlayerAttack(BattleCardUI attacker)
    {
        Vector3 startPos = attacker.transform.position;
        int damage = attacker.GetModifiedATK(isPlayerAttack: true); // 🔥 ใช้ ModifiedATK


        // 🔥 ทริกเกอร์ OnStrike Effects (ก่อนเช็คการกัน)
        yield return StartCoroutine(ResolveEffects(attacker, EffectTrigger.OnStrike, isPlayer: true));

        // ถ้าการ์ดถูกทำลายระหว่าง OnStrike ให้หยุด
        if (attacker == null || attacker.GetData() == null) yield break;

        // 🚀 ตรวจสอบว่าการ์ดนี้ข้ามการกันได้หรือไม่ (ต้องเช็คว่ามีโล่บอทที่สามารถกันได้หรือไม่)
        bool canBypassAll = false;
        if (attacker.canBypassIntercept)
        {
            Debug.Log($"🔍 Player attacker has BypassIntercept. Checking bot shields...");
            // เช็คว่ามีโล่บอทที่สามารถกันได้หรือไม่
            bool hasInterceptableShield = false;
            foreach (Transform equipSlot in enemyEquipSlots)
            {
                if (equipSlot.childCount > 0)
                {
                    var shield = equipSlot.GetChild(0).GetComponent<BattleCardUI>();
                    if (shield != null && shield.GetData() != null && !shield.cannotIntercept)
                    {
                        Debug.Log($"  → Checking bot shield: {shield.GetData().cardName} (Cost={shield.GetData().cost}, MainCat={shield.GetData().mainCategory})");
                        // ถ้าโล่นี้ไม่ถูกข้าม = สามารถกันได้
                        bool isBypassed = CanAttackerBypassShield(attacker, shield);
                        Debug.Log($"     Result: isBypassed={isBypassed}");
                        if (!isBypassed)
                        {
                            hasInterceptableShield = true;
                            Debug.Log($"✅ Found interceptable bot shield: {shield.GetData().cardName}");
                            break;
                        }
                    }
                }
            }
            // ถ้าไม่มีโล่ที่สามารถกันได้เลย = ข้ามการกันทั้งหมด
            canBypassAll = !hasInterceptableShield;
            Debug.Log($"📊 hasInterceptableShield={hasInterceptableShield}, canBypassAll={canBypassAll}");
        }

        // 🔥 ถ้าข้ามการกันได้ทั้งหมด โจมตีตรงไปที่บอทโดยตรง
        if (canBypassAll)
        {
            Debug.Log($"🚀 {attacker.GetData().cardName} bypasses intercept - direct damage!");
            AddBattleLog($"{attacker.GetData().cardName} bypasses intercept - {damage} direct damage");

            // 📊 บันทึกสถิติ: การข้ามการกัน
            currentBattleStats.interceptionsBlocked++;

            // พุ่งไป
            yield return StartCoroutine(MoveToTarget(attacker.transform, enemySpot.position, 0.3f));

            EnemyTakeDamage(damage);

            // ทริกเกอร์ OnStrikeHit
            yield return StartCoroutine(ResolveEffects(attacker, EffectTrigger.OnStrikeHit, isPlayer: true));

            // รีเซ็ต bypass status หลังโจมตี
            attacker.canBypassIntercept = false;
            attacker.bypassCostThreshold = 0;
            attacker.bypassAllowedMainCat = MainCategory.General;
            attacker.bypassAllowedSubCat = SubCategory.General;
            ClearMarkedInterceptPunish(attacker);

            yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));

            if (enemyCurrentHP <= 0)
            {
                Debug.Log("🎉 ศัตรูตายแล้ว -> Win!");
                StartCoroutine(EndBattle(true));
            }

            UpdateUI();
            yield break;
        }

        // พุ่งไป (เร็วขึ้น 0.3 วินาที)
        yield return StartCoroutine(MoveToTarget(attacker.transform, enemySpot.position, 0.3f));

        // 🛡️ เช็คว่ามีการ์ดที่ต้องกันบังคับหรือไม่
        bool hasMustIntercept = HasMustInterceptCard(false); // defenderIsPlayer = false (บอท)

        BattleCardUI botShield = null;

        if (hasMustIntercept)
        {
            // หาการ์ดที่ mustIntercept = true
            botShield = GetMustInterceptCard(false);
            if (botShield != null)
            {
                Debug.Log($"🛡️ {botShield.GetData().cardName} is forced to intercept!");
                AddBattleLog($"{botShield.GetData().cardName} is forced to intercept");

                // รีเซ็ต mustIntercept หลังการกัน
                botShield.mustIntercept = false;
            }
        }
        else
        {
            // ✅ ให้ผู้เล่นเลือกโล่ฝั่งบอทว่าจะออกมากัน (ถ้ามี)
            List<BattleCardUI> selectableShields = new List<BattleCardUI>();
            Debug.Log($"🔍 Checking bot shields for interception. Attacker: {attacker.GetData().cardName}, canBypass: {attacker.canBypassIntercept}");

            foreach (Transform slot in enemyEquipSlots)
            {
                if (slot.childCount > 0)
                {
                    var s = slot.GetChild(0).GetComponent<BattleCardUI>();
                    if (s != null && s.GetData() != null && !s.cannotIntercept)
                    {
                        Debug.Log($"  → Checking shield: {s.GetData().cardName} (Cost={s.GetData().cost})");
                        // ตรวจสอบว่าผู้โจมตี canBypassIntercept นี้ข้ามกันได้หรือไม่
                        bool isBypassed = CanAttackerBypassShield(attacker, s);
                        Debug.Log($"     Result: isBypassed={isBypassed}, will be added={!isBypassed}");
                        if (!isBypassed)
                        {
                            selectableShields.Add(s);
                        }
                    }
                }
            }

            Debug.Log($"✅ Total selectable shields: {selectableShields.Count}");

            if (selectableShields.Count > 0)
            {
                // ✅ บอทเลือกกันเอง (ไม่ให้ผู้เล่นเลือกแทน)
                var attackerData = attacker != null ? attacker.GetData() : null;
                if (attackerData != null)
                {
                    // เลือกโล่ที่ประเภทตรงก่อนเพื่อทำลายมอนสเตอร์ผู้เล่น
                    botShield = selectableShields.FirstOrDefault(s => s != null && IsInterceptTypeMatched(attacker, s, blockerIsPlayer: false));
                }

                // ถ้าไม่มีประเภทตรง ให้เลือกโล่ตัวแรกที่ใช้ได้
                if (botShield == null)
                {
                    botShield = selectableShields.FirstOrDefault(s => s != null && s.GetData() != null);
                }
            }
        }

        if (botShield != null)
        {
            TryResolveMarkedInterceptPunish(attacker, botShield, attackerIsPlayer: true);

            Debug.Log($"🛡️ บอทกันด้วย {botShield.GetData().cardName} ({botShield.GetData().subCategory})");
            AddBattleLog($"Bot blocks with {botShield.GetData().cardName} ({botShield.GetData().subCategory})");
            if (AudioManager.Instance) AudioManager.Instance.PlaySFX("Block");

            // 🔥 ตรวจสอบ null ก่อนเช็คประเภท
            if (attacker == null || attacker.GetData() == null || botShield.GetData() == null)
            {
                Debug.LogWarning("ProcessPlayerAttack: null card data detected!");
                yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
                yield break;
            }

            CardData attackerData = attacker.GetData();
            CardData shieldData = botShield.GetData();
            bool match = IsInterceptTypeMatched(attacker, botShield, blockerIsPlayer: false);

            if (match)
            {
                TryResolveInterceptHeal(botShield, attacker, blockerIsPlayer: false, isTypeMatched: true);

                // ประเภทตรง → ทำลายทั้งคู่
                ShowDamagePopupString("Double KO!", attacker.transform);
                AddBattleLog($"  SubCategory match ({shieldData.subCategory}) - Both destroyed");
                DestroyCardToGraveyard(attacker);
                DestroyCardToGraveyard(botShield);
                Debug.Log($"✅ บอทกันได้! ประเภทตรงกัน ({shieldData.subCategory}) - ทั้งคู่ทำลาย ไม่เสีย HP");
            }
            else
            {
                // ประเภทต่างกัน → ทำลายเฉพาะโล่
                AddBattleLog($"  SubCategory mismatch ({attackerData.subCategory} vs {shieldData.subCategory}) - Shield broken, {damage} damage passes");
                ShowDamagePopupString("Shield Break!", botShield.transform);
                DestroyCardToGraveyard(botShield);
                Debug.Log($"✅ บอทกันได้! ประเภทต่างกัน ({attackerData.subCategory} ≠ {shieldData.subCategory}) - โล่แตก ไม่เสีย HP");
            }

            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
        }
        else
        {
            Debug.Log($"💥 ไม่มีโล่ -> บอทรับดาเมจ {damage}");
            EnemyTakeDamage(damage);

            // 🔥 ทริกเกอร์ OnStrikeHit Effects (หลังโจมตีสำเร็จ) (รอให้เสร็จก่อน)
            yield return StartCoroutine(ResolveEffects(attacker, EffectTrigger.OnStrikeHit, isPlayer: true));

            yield return StartCoroutine(MoveToTarget(attacker.transform, startPos, 0.25f));
        }

        ClearMarkedInterceptPunish(attacker);

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

        AddBattleLog($"\n=== BOT TURN {turnCount} START === HP:{enemyCurrentHP}/{enemyMaxHP} | PP:{enemyCurrentPP}/{enemyMaxPP}");

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

        // กฎจั่วบอท: ถ้ามือ >= 5 จั่ว 1, ถ้ามือน้อยกว่า 5 จั่วให้ครบ 5
        int enemyHandCount = enemyHandArea != null ? enemyHandArea.childCount : 0;
        int enemyDrawAmount = enemyHandCount >= 5 ? 1 : Mathf.Max(0, 5 - enemyHandCount);
        Debug.Log($"🤖 บอทจั่วการ์ด: มือปัจจุบัน {enemyHandCount} ใบ, จั่ว {enemyDrawAmount} ใบ");
        if (enemyDrawAmount > 0)
            yield return StartCoroutine(DrawEnemyCard(enemyDrawAmount));

        // 🔥 รีเซ็ตสถานะโจมตีของมอนสเตอร์บอททั้งหมด
        ResetAllEnemyMonstersAttackState();

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(BotSummonPhase());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(BotAttackPhase());
        yield return new WaitForSeconds(0.5f);

        if (isEnding || state == BattleState.WON || state == BattleState.LOST) yield break;

        // 🎯 ทริกเกอร์เอฟเฟกต์ตอนจบเทิร์นของการ์ดฝั่งบอท
        yield return StartCoroutine(ResolveTurnEndEffectsForSide(isPlayerSide: false));

        if (isEnding || state == BattleState.WON || state == BattleState.LOST) yield break;

        // 🎮 นับ Control duration ตอนจบเทิร์นบอท
        ProcessControlDurationsForAllEquips();

        // 🏁 จบเทิร์นบอท - ซ่อนการ์ดที่ reveal กลับไป
        EndEnemyTurn();

        if (state != BattleState.LOST) StartPlayerTurn();
    }

    IEnumerator BotSummonPhase()
    {
        if (enemyHandArea == null) yield break;

        // ลิสต์การ์ดในมือบอท (เฉพาะที่ยังไม่ลงสนาม)
        var handCards = enemyHandArea.GetComponentsInChildren<BattleCardUI>();

        // 🎇 ลองใช้เวทย์ก่อน
        var spellCard = System.Array.Find(handCards, c => c != null && c.GetData() != null && c.GetData().type == CardType.Spell && enemyCurrentPP >= c.GetData().cost);
        if (spellCard != null && CanCastSpell(spellCard.GetData(), isPlayer: false))
        {
            yield return StartCoroutine(BotCastSpell(spellCard));
            enemyCurrentPP -= spellCard.GetData().cost;
            // 🔥 ลบ return ออก เพื่อให้บอทสามารถเล่นการ์ดอื่นต่อได้หลังใช้เวทย์
        }

        // 🔥 ลอง Monster (สามารถ Sacrifice ได้ถ้าช่องเต็ม)
        Transform freeMonSlot = GetFreeSlot(CardType.Monster, false);
        var bestMonster = System.Array.Find(handCards, c => c != null && c.GetData() != null && c.GetData().type == CardType.Monster && enemyCurrentPP >= c.GetData().cost);

        if (bestMonster != null)
        {
            if (freeMonSlot != null)
            {
                // มีช่องว่าง → ลงปกติ
                yield return StartCoroutine(AnimateBotPlayCard(bestMonster, freeMonSlot));
                enemyCurrentPP -= bestMonster.GetData().cost;
            }
            else
            {
                // ไม่มีช่องว่าง → ลอง Sacrifice
                BotTrySacrifice(bestMonster, CardType.Monster);
            }
        }

        // 🔥 ลอง EquipSpell (สามารถ Sacrifice ได้ถ้าช่องเต็ม)
        Transform freeEqSlot = GetFreeSlot(CardType.EquipSpell, false);
        var bestEquip = System.Array.Find(handCards, c => c != null && c.GetData() != null && c.GetData().type == CardType.EquipSpell && enemyCurrentPP >= c.GetData().cost);

        if (bestEquip != null)
        {
            if (freeEqSlot != null)
            {
                // มีช่องว่าง → ลงปกติ
                yield return StartCoroutine(AnimateBotPlayCard(bestEquip, freeEqSlot));
                enemyCurrentPP -= bestEquip.GetData().cost;
            }
            else
            {
                // ไม่มีช่องว่าง → ลอง Sacrifice
                BotTrySacrifice(bestEquip, CardType.EquipSpell);
            }
        }
    }

    // 🔥 บอทลองสังเวยการ์ดเก่าเพื่อลงการ์ดใหม่
    void BotTrySacrifice(BattleCardUI newCard, CardType cardType)
    {
        if (newCard == null || newCard.GetData() == null) return;

        Transform[] slots = (cardType == CardType.Monster) ? enemyMonsterSlots : enemyEquipSlots;
        if (slots == null || slots.Length == 0) return;

        CardData newData = newCard.GetData();
        int costDiff = 0;
        BattleCardUI weakestCard = null;
        Transform weakestSlot = null;

        // หาการ์ดที่อ่อนแอที่สุดบนสนาม
        foreach (var slot in slots)
        {
            if (slot.childCount > 0)
            {
                var oldCard = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (oldCard != null && oldCard.GetData() != null)
                {
                    CardData oldData = oldCard.GetData();

                    // บอทจะสังเวยถ้า:
                    // 1. การ์ดใหม่แรงกว่า (ATK+HP มากกว่า)
                    // 2. หรือ cost ต่างกันไม่เกิน 2 PP
                    int newPower = newData.atk + newData.hp;
                    int oldPower = oldData.atk + oldData.hp;
                    int diff = newData.cost - oldData.cost;

                    if (newPower > oldPower && diff <= 2 && enemyCurrentPP >= Mathf.Max(0, diff))
                    {
                        if (weakestCard == null || oldPower < (weakestCard.GetData().atk + weakestCard.GetData().hp))
                        {
                            weakestCard = oldCard;
                            weakestSlot = slot;
                            costDiff = diff;
                        }
                    }
                }
            }
        }

        // ถ้าพบการ์ดที่จะสังเวยได้
        if (weakestCard != null && weakestSlot != null)
        {
            int costToPay = Mathf.Max(0, costDiff);
            if (enemyCurrentPP >= costToPay)
            {
                Debug.Log($"🤖 บอทสังเวย {weakestCard.GetData().cardName} เพื่อลง {newData.cardName} (cost diff: {costToPay})");
                StartCoroutine(BotPerformSacrifice(newCard, weakestCard, weakestSlot, costToPay));
            }
        }
    }

    // 🔥 บอททำการสังเวย
    IEnumerator BotPerformSacrifice(BattleCardUI newCard, BattleCardUI oldCard, Transform targetSlot, int costToPay)
    {
        CardData newData = newCard.GetData();
        CardData oldData = oldCard.GetData();

        // จ่าย PP
        enemyCurrentPP -= costToPay;
        Debug.Log($"🤖 บอท Sacrifice: {oldData.cardName} → {newData.cardName}, Cost: {costToPay}, PP: {enemyCurrentPP}");

        // ทำลายการ์ดเก่า
        DestroyCardToGraveyard(oldCard);

        yield return new WaitForSeconds(0.2f);

        // ลงการ์ดใหม่
        yield return StartCoroutine(AnimateBotPlayCard(newCard, targetSlot));

        AddBattleLog($"Bot sacrificed {oldData.cardName} to play {newData.cardName}");
        UpdateUI();
    }

    IEnumerator AnimateBotPlayCard(BattleCardUI ui, Transform slot)
    {
        if (ui == null || ui.transform == null || slot == null) yield break;

        // ย้ายไปอยู่ระดับ Canvas เพื่อบิน
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            ui.transform.SetParent(canvas.transform, worldPositionStays: true);
        }

        Vector3 startPos = ui.transform.position;
        Vector3 endPos = slot.position;
        Vector3 startScale = ui.transform.localScale;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = 1f - Mathf.Pow(1f - t, 3);

            ui.transform.position = Vector3.Lerp(startPos, endPos, easeT);
            ui.transform.localScale = Vector3.Lerp(startScale, Vector3.one, easeT);
            yield return null;
        }

        // Snap เข้า slot
        ui.transform.SetParent(slot, worldPositionStays: false);
        ui.transform.localPosition = Vector3.zero;
        ui.transform.localScale = Vector3.one;

        ui.isOnField = true;
        // 🔥 EquipSpell ไม่มี Summoning Sickness
        if (ui.GetData().type != CardType.EquipSpell)
        {
            ui.hasAttacked = true; // Monster ต้องรอเทิร์นถัดไป

            // 🔥 เช็คว่ามีสกิล Rush หรือไม่ (Bot)
            bool hasRush = HasActiveRush(ui);
            if (hasRush)
            {
                ui.hasAttacked = false;
                AddBattleLog($"💨 <color=red>Bot's {ui.GetData().cardName}</color> มีสกิล Rush!");
            }
        }
        ui.UpdateCardSize(); // 🔥 ปรับขนาดการ์ดบนสนาม
        var img = ui.GetComponent<Image>();
        if (img)
        {
            // 🔥 แก้: ตรวจสอบให้แน่ใจว่าการ์ดแสดงหน้าไม่ใช่หลัง
            if (ui.GetData() != null && ui.GetData().artwork != null)
            {
                img.sprite = ui.GetData().artwork; // แสดงหน้าการ์ด
            }
            // 🔥 EquipSpell ไม่มืด
            img.color = (ui.GetData().type == CardType.EquipSpell) ? Color.white : Color.gray;
        }
        // แสดงกรอบเมื่อการ์ดหงายหน้า
        ui.SetFrameVisible(true);

        // 🔥 อนุญาตให้คลิกดูรายละเอียดการ์ดบนสนามบอท
        var cg = ui.GetComponent<CanvasGroup>();
        if (cg)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        }

        Debug.Log($"🤖 บอทลงการ์ด: {ui.GetData()?.cardName} (ห้ามตีเทิร์นนี้)");

        // 🔥 ทริกเกอร์ OnDeploy Effects สำหรับบอท (เหมือนผู้เล่น)
        if (ui != null && ui.GetData() != null)
        {
            yield return StartCoroutine(ResolveEffects(ui, EffectTrigger.OnDeploy, isPlayer: false));
            Debug.Log($"✅ บอททริกเกอร์ OnDeploy effects: {ui.GetData().cardName}");
        }

        // ตรวจสอบจำนวนการ์ดหลังลง
        int handAfter = enemyHandArea != null ? enemyHandArea.childCount : 0;
        Debug.Log($"🤖 มือบอทหลังลงการ์ด: {handAfter} ใบ");

        // จัดมือใหม่หลังลงการ์ด
        ArrangeEnemyHand();
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
                while (monster != null && monster.CanAttackNow())
                {
                    currentAttackerBot = monster;

                    // นับจำนวนครั้งที่โจมตี
                    monster.attacksThisTurn++;
                    if (monster.attacksThisTurn >= monster.GetMaxAttacksPerTurn())
                    {
                        monster.hasAttacked = true;
                    }

                    // 🟣 เปลี่ยนสีเป็นเทาหลังโจมตี (ยกเว้นถ้าสูญเสีย category)
                    if (!monster.hasLostCategory)
                    {
                        monster.GetComponent<Image>().color = Color.gray;
                    }

                    Vector3 startPos = monster.transform.position;
                    // กัน Error: ถ้าลืมลาก PlayerSpot ให้วิ่งไปที่ (0,0,0)
                    Vector3 targetPos = (playerSpot != null) ? playerSpot.position : Vector3.zero;

                    Debug.Log($"🚨 บอทใช้ {monster.GetData().cardName} โจมตี!");

                    // 🔥 ทริกเกอร์ OnStrike Effects (ก่อนเช็คการกัน)
                    yield return StartCoroutine(ResolveEffects(monster, EffectTrigger.OnStrike, isPlayer: false));

                    // ถ้าการ์ดถูกทำลายระหว่าง OnStrike ให้ข้าม
                    if (monster == null || monster.GetData() == null) continue;

                    // 🚀 ตรวจสอบว่าบอทสามารถข้ามการกันได้หรือไม่ (ต้องเช็คว่ามีโล่ที่สามารถกันได้หรือไม่)
                    bool canBypassAll = false;
                    if (monster.canBypassIntercept)
                    {
                        Debug.Log($"🔍 Bot has BypassIntercept. Checking player shields...");
                        // เช็คว่ามีโล่ที่สามารถกันได้หรือไม่
                        bool hasInterceptableShield = false;
                        foreach (Transform equipSlot in playerEquipSlots)
                        {
                            if (equipSlot.childCount > 0)
                            {
                                var shield = equipSlot.GetChild(0).GetComponent<BattleCardUI>();
                                if (shield != null && shield.GetData() != null && !shield.cannotIntercept)
                                {
                                    Debug.Log($"  → Checking player shield: {shield.GetData().cardName} (Cost={shield.GetData().cost}, MainCat={shield.GetData().mainCategory})");
                                    // ถ้าโล่นี้ไม่ถูกข้าม = สามารถกันได้
                                    bool isBypassed = CanAttackerBypassShield(monster, shield);
                                    Debug.Log($"     Result: isBypassed={isBypassed}");
                                    if (!isBypassed)
                                    {
                                        hasInterceptableShield = true;
                                        Debug.Log($"✅ Found interceptable shield: {shield.GetData().cardName}");
                                        break;
                                    }
                                }
                            }
                        }
                        // ถ้าไม่มีโล่ที่สามารถกันได้เลย = ข้ามการกันทั้งหมด
                        canBypassAll = !hasInterceptableShield;
                        Debug.Log($"📊 hasInterceptableShield={hasInterceptableShield}, canBypassAll={canBypassAll}");
                    }

                    // 1. พุ่งมา (เร็วขึ้น 0.3 วินาที)
                    yield return StartCoroutine(MoveToTarget(monster.transform, targetPos, 0.3f));

                    // 🔥 ถ้าบอทข้ามการกันได้ทั้งหมด โจมตีตรงไปที่ผู้เล่นโดยตรง
                    if (canBypassAll)
                    {
                        int botDamage = monster.GetModifiedATK(isPlayerAttack: false); // 🔥 ใช้ ModifiedATK
                        Debug.Log($"🚀 Bot {monster.GetData().cardName} bypasses intercept - direct damage!");
                        AddBattleLog($"Bot {monster.GetData().cardName} bypasses intercept - {botDamage} direct damage");

                        yield return new WaitForSeconds(0.2f);
                        PlayerTakeDamage(botDamage);

                        // 🔥 ทริกเกอร์ OnStrikeHit Effects (หลังโจมตีสำเร็จ - ข้ามการกัน)
                        yield return StartCoroutine(ResolveEffects(monster, EffectTrigger.OnStrikeHit, isPlayer: false));

                        // รีเซ็ต bypass status
                        monster.canBypassIntercept = false;
                        monster.bypassCostThreshold = 0;
                        monster.bypassAllowedMainCat = MainCategory.General;
                        monster.bypassAllowedSubCat = SubCategory.General;
                        ClearMarkedInterceptPunish(monster);

                        // ดึงกลับ
                        if (monster != null && monster.gameObject != null && monster.transform != null)
                        {
                            yield return StartCoroutine(MoveToTarget(monster.transform, startPos, 0.25f));
                            if (monster != null && monster.transform != null)
                            {
                                monster.transform.localPosition = Vector3.zero;
                            }
                        }

                        if (state == BattleState.LOST) yield break;
                        continue; // ไปยังมอนสเตอร์ตัวถัดไป
                    }

                    // 2. เช็คว่ามีการ์ดที่ต้องกันบังคับหรือไม่
                    bool hasMustIntercept = HasMustInterceptCard(true); // defenderIsPlayer = true
                    bool playerHasShield = HasEquipInSlots(playerEquipSlots);

                    // 🛡️ ถ้ามีการ์ดที่ต้องกันบังคับ ให้กันอัตโนมัติ
                    if (hasMustIntercept)
                    {
                        BattleCardUI forcedShield = GetMustInterceptCard(true);
                        if (forcedShield != null)
                        {
                            TryResolveMarkedInterceptPunish(monster, forcedShield, attackerIsPlayer: false);

                            Debug.Log($"🛡️ {forcedShield.GetData().cardName} is forced to intercept bot's attack!");
                            AddBattleLog($"{forcedShield.GetData().cardName} forced to block {monster.GetData().cardName}");

                            // ประมวลผลการกัน
                            CardData attackerData = monster.GetData();
                            CardData shieldData = forcedShield.GetData();
                            bool match = IsInterceptTypeMatched(monster, forcedShield, blockerIsPlayer: true);

                            if (match)
                            {
                                TryResolveInterceptHeal(forcedShield, monster, blockerIsPlayer: true, isTypeMatched: true);

                                // ประเภทตรง → ทำลายทั้งคู่
                                ShowDamagePopupString("Double KO!", monster.transform);
                                AddBattleLog($"  SubCategory match ({shieldData.subCategory}) - Both destroyed");
                                DestroyCardToGraveyard(monster);
                                DestroyCardToGraveyard(forcedShield);
                                Debug.Log($"✅ กันได้! ประเภทตรงกัน ({shieldData.subCategory}) - ทั้งคู่ทำลาย");
                            }
                            else
                            {
                                // ประเภทต่าง → โล่แตก แต่ผู้เล่นไม่เสีย HP
                                ShowDamagePopupString("Shield Break!", forcedShield.transform);
                                AddBattleLog($"  SubCategory mismatch ({attackerData.subCategory} vs {shieldData.subCategory}) - Shield broken");
                                DestroyCardToGraveyard(forcedShield);
                                Debug.Log($"✅ กันได้! ประเภทต่างกัน ({attackerData.subCategory} ≠ {shieldData.subCategory}) - โล่แตก ไม่เสีย HP");
                            }

                            // รีเซ็ต mustIntercept
                            forcedShield.mustIntercept = false;

                            playerHasMadeChoice = true;
                        }
                    }
                    // ต้องมีโล่ และ มีปุ่ม ถึงจะหยุดถาม (ถ้าลืมลากปุ่ม จะตีเลยกันค้าง)
                    else if (playerHasShield && takeDamageButton != null)
                    {
                        state = BattleState.DEFENDER_CHOICE;
                        playerHasMadeChoice = false;
                        currentAttackerBot = monster; // เก็บผู้โจมตีปัจจุบัน

                        // 🔥 Highlight โล่ที่สามารถกันได้
                        HighlightInterceptableShields(monster);

                        takeDamageButton.SetActive(true);
                        if (turnText) turnText.text = "DEFEND!";

                        yield return new WaitUntil(() => playerHasMadeChoice);

                        // 🔥 ปิด Highlight ทั้งหมด
                        ClearAllShieldHighlights();

                        if (takeDamageButton) takeDamageButton.SetActive(false);
                    }
                    else
                    {
                        // ตีเลย
                        if (playerHasShield && takeDamageButton == null) Debug.LogError("⚠️ ลืกลากปุ่ม TakeDamageButton!");

                        yield return new WaitForSeconds(0.2f);
                        if (monster != null)
                        {
                            int botDamage = monster.GetModifiedATK(isPlayerAttack: false); // 🔥 ใช้ ModifiedATK
                            PlayerTakeDamage(botDamage);

                            // 🔥 ทริกเกอร์ OnStrikeHit Effects (หลังโจมตีสำเร็จ - ไม่ถูกกัน)
                            yield return StartCoroutine(ResolveEffects(monster, EffectTrigger.OnStrikeHit, isPlayer: false));
                        }
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

                    if (monster != null)
                    {
                        ClearMarkedInterceptPunish(monster);
                    }

                    if (state == BattleState.LOST) yield break;
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

    public BattleCardUI GetCurrentAttacker()
    {
        return currentAttackerBot;
    }

    /// <summary>ฟังก์ชันสำหรับให้ BattleCardUI เช็คว่าโล่สามารถกันได้หรือไม่ (public wrapper)</summary>
    public bool CanBypassShield(BattleCardUI attacker, BattleCardUI shield)
    {
        return CanAttackerBypassShield(attacker, shield);
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

        // 🛡️ ถ้ามีการ์ดที่ต้องกันบังคับ ให้กันอัตโนมัติแทนการข้าม
        if (HasMustInterceptCard(true))
        {
            BattleCardUI forcedShield = GetMustInterceptCard(true);
            if (forcedShield != null)
            {
                Debug.Log($"🛡️ {forcedShield.GetData().cardName} is forced to intercept (cannot skip)!");
                OnPlayerSelectBlocker(forcedShield);
                return;
            }
        }

        PlayerTakeDamage(currentAttackerBot.GetData().atk);

        // 🔥 ทริกเกอร์ OnStrikeHit Effects สำหรับบอท (หลังโจมตีสำเร็จ - ผู้เล่นไม่กัน)
        StartCoroutine(ResolveEffects(currentAttackerBot, EffectTrigger.OnStrikeHit, isPlayer: false));

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

        // 🚫 เช็คว่าโล่ถูกปิดการกันหรือไม่
        if (myShield.cannotIntercept)
        {
            Debug.LogWarning($"🚫 {myShield.GetData().cardName} ถูกปิดการกัน! ไม่สามารถใช้กันได้");
            ShowDamagePopupString("Cannot Block!", myShield.transform);
            return; // ไม่อนุญาตให้กัน
        }

        // 🚫 ตรวจสอบว่าโล่นี้ถูก bypass หรือไม่ (ใช้ฟังก์ชันเดียวกับที่เช็คตอน highlight)
        if (currentAttackerBot.canBypassIntercept)
        {
            bool isBypassed = CanAttackerBypassShield(currentAttackerBot, myShield);
            if (isBypassed)
            {
                Debug.LogWarning($"🚫 {myShield.GetData().cardName} ถูกข้าม (Bypassed) - ไม่สามารถกันได้!");
                ShowDamagePopupString("Bypassed!", myShield.transform);
                return;
            }
        }

        // 🛡️ ถ้ามีการ์ดที่ต้องกันบังคับ ห้ามเลือกการ์ดอื่น
        if (HasMustInterceptCard(true) && !myShield.mustIntercept)
        {
            Debug.LogWarning("🛡️ มีการ์ดที่ต้องกันบังคับอยู่ ต้องเลือกการ์ดที่มี mustIntercept เท่านั้น!");
            ShowDamagePopupString("Must Block!", myShield.transform);
            return; // ไม่อนุญาตให้กันด้วยใบอื่น
        }

        CardData attackerData = currentAttackerBot.GetData();
        CardData shieldData = myShield.GetData();

        TryResolveMarkedInterceptPunish(currentAttackerBot, myShield, attackerIsPlayer: false);

        Debug.Log($"🛡️ ตรวจสอบการกัน: โจมตี={attackerData.cardName} ({attackerData.subCategory}), โล่={shieldData.cardName} ({shieldData.subCategory})");

        // 📊 บันทึกสถิติ: การกันสำเร็จ
        currentBattleStats.interceptionsSuccessful++;

        bool match = IsInterceptTypeMatched(currentAttackerBot, myShield, blockerIsPlayer: true);

        if (match)
        {
            TryResolveInterceptHeal(myShield, currentAttackerBot, blockerIsPlayer: true, isTypeMatched: true);

            ShowDamagePopupString("Double KO!", currentAttackerBot.transform);
            DestroyCardToGraveyard(currentAttackerBot);
            DestroyCardToGraveyard(myShield);
            Debug.Log($"✅ กันได้! ประเภทตรงกัน ({attackerData.subCategory}) - ทั้งคู่ทำลาย ไม่เสีย HP");
        }
        else
        {
            ShowDamagePopupString("Shield Break!", myShield.transform);
            DestroyCardToGraveyard(myShield);

            // 🔥 ประเภทไม่ตรง → โล่แตก แต่ไม่เสีย HP (ปกป้องสำเร็จ)
            Debug.Log($"✅ กันได้! ประเภทต่างกัน ({attackerData.subCategory} ≠ {shieldData.subCategory}) - โล่แตก แต่ไม่เสีย HP");
        }

        // 🛡️ รีเซ็ต mustIntercept หลังการกัน
        if (myShield.mustIntercept)
        {
            myShield.mustIntercept = false;
            Debug.Log($"🔄 Reset mustIntercept for {shieldData.cardName}");
        }

        // 🔥 เซ็ตแล้วหลัง logic กันค้าง
        playerHasMadeChoice = true;
        if (takeDamageButton) takeDamageButton.SetActive(false);
    }

    void TryResolveMarkedInterceptPunish(BattleCardUI attacker, BattleCardUI blocker, bool attackerIsPlayer)
    {
        if (attacker == null || blocker == null) return;
        if (attacker.markedInterceptTarget == null || attacker.markedInterceptMillCount <= 0) return;
        if (attacker.markedInterceptTarget != blocker) return;

        CardEffect millEffect = new CardEffect
        {
            targetType = TargetType.EnemyDeck,
            value = attacker.markedInterceptMillCount
        };

        ApplyDiscardDeck(attacker, millEffect, attackerIsPlayer);

        if (attacker.GetData() != null && blocker.GetData() != null)
        {
            AddBattleLog($"{blocker.GetData().cardName} intercepted marked attacker -> mill {attacker.markedInterceptMillCount}");
        }

        ClearMarkedInterceptPunish(attacker);
    }

    void TryResolveInterceptHeal(BattleCardUI blocker, BattleCardUI attacker, bool blockerIsPlayer, bool isTypeMatched)
    {
        if (!isTypeMatched)
        {
            Debug.Log("ℹ️ InterceptHeal skipped: type mismatch");
            return;
        }
        if (blocker == null || attacker == null)
        {
            Debug.Log("ℹ️ InterceptHeal skipped: blocker/attacker is null");
            return;
        }

        CardData blockerData = blocker.GetData();
        CardData attackerData = attacker.GetData();
        if (blockerData == null || attackerData == null || blockerData.effects == null)
        {
            Debug.Log("ℹ️ InterceptHeal skipped: blockerData/attackerData/effects missing");
            return;
        }

        bool attackerIsPlayer = IsCardOwnedByPlayer(attacker);
        int interceptedAtk = attacker.GetModifiedATK(isPlayerAttack: attackerIsPlayer);
        if (interceptedAtk <= 0)
        {
            interceptedAtk = Mathf.Max(0, attackerData.atk);
        }

        bool healApplied = false;
        bool foundHealEffect = false;

        foreach (var effect in blockerData.effects)
        {
            bool isInterceptHeal =
                effect.action == ActionType.HealHP
                && (effect.trigger == EffectTrigger.OnIntercept || effect.trigger == EffectTrigger.Continuous);

            if (!isInterceptHeal) continue;
            foundHealEffect = true;

            if (IsEffectSuppressedByOpponentContinuousAura(blocker, effect, EffectTrigger.OnIntercept, blockerIsPlayer))
            {
                Debug.Log($"ℹ️ InterceptHeal suppressed: {blockerData.cardName}");
                continue;
            }

            if (effect.targetCardTypeFilter != EffectCardTypeFilter.Any)
            {
                if (effect.targetCardTypeFilter == EffectCardTypeFilter.Monster && attackerData.type != CardType.Monster) continue;
                if (effect.targetCardTypeFilter == EffectCardTypeFilter.Token && attackerData.type != CardType.Token) continue;
                if (effect.targetCardTypeFilter == EffectCardTypeFilter.EquipSpell && attackerData.type != CardType.EquipSpell) continue;
                if (effect.targetCardTypeFilter == EffectCardTypeFilter.Spell && attackerData.type != CardType.Spell) continue;
            }

            if (effect.targetMainCat != MainCategory.General && attackerData.mainCategory != effect.targetMainCat)
            {
                continue;
            }

            SubCategory attackerSubCategory = attacker.GetModifiedSubCategory();
            if (effect.targetSubCat != SubCategory.General && attackerSubCategory != effect.targetSubCat)
            {
                continue;
            }

            if (effect.useExcludeFilter)
            {
                bool excludedByMain = effect.excludeMainCat != MainCategory.General && attackerData.mainCategory == effect.excludeMainCat;
                bool excludedBySub = effect.excludeSubCat != SubCategory.General && attackerSubCategory == effect.excludeSubCat;
                if (excludedByMain || excludedBySub)
                {
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(effect.targetCardNameFilter))
            {
                if (string.IsNullOrWhiteSpace(attackerData.cardName)) continue;

                bool nameMatch = string.Equals(
                    attackerData.cardName.Trim(),
                    effect.targetCardNameFilter.Trim(),
                    System.StringComparison.OrdinalIgnoreCase
                );

                if (!nameMatch) continue;
            }

            int healAmount = interceptedAtk;
            if (healAmount <= 0)
            {
                Debug.Log($"ℹ️ InterceptHeal skipped: intercepted ATK <= 0 ({interceptedAtk})");
                continue;
            }

            bool healSelf = effect.targetType == TargetType.Self || effect.targetType == TargetType.AllGlobal;
            bool healEnemy = effect.targetType == TargetType.EnemyPlayer;
            bool targetSupported = healSelf || healEnemy;
            if (!targetSupported) continue;

            bool healPlayerSide = healSelf ? blockerIsPlayer : !blockerIsPlayer;

            if (healPlayerSide)
            {
                int before = currentHP;
                currentHP = Mathf.Min(maxHP, currentHP + healAmount);
                AddBattleLog($"{blockerData.cardName} intercept-heals Player {healAmount} HP ({before} -> {currentHP})");
                if (before == currentHP)
                {
                    Debug.Log($"ℹ️ InterceptHeal capped: Player HP already full ({currentHP}/{maxHP})");
                }
            }
            else
            {
                int before = enemyCurrentHP;
                enemyCurrentHP = Mathf.Min(enemyMaxHP, enemyCurrentHP + healAmount);
                AddBattleLog($"{blockerData.cardName} intercept-heals Bot {healAmount} HP ({before} -> {enemyCurrentHP})");
                if (before == enemyCurrentHP)
                {
                    Debug.Log($"ℹ️ InterceptHeal capped: Bot HP already full ({enemyCurrentHP}/{enemyMaxHP})");
                }
            }

            ShowDamagePopupString($"+{healAmount} HP", blocker.transform);
            Debug.Log($"💚 Intercept Heal: {blockerData.cardName} healed {healAmount} (from intercepted {attackerData.cardName})");
            UpdateUI();
            healApplied = true;
        }

        if (!foundHealEffect)
        {
            Debug.Log($"ℹ️ InterceptHeal skipped: no HealHP(OnIntercept/Continuous) effect on {blockerData.cardName}");
        }
        else if (!healApplied)
        {
            Debug.Log($"ℹ️ InterceptHeal had heal effect but no branch applied for {blockerData.cardName}");
        }
    }

    void ClearMarkedInterceptPunish(BattleCardUI attacker)
    {
        if (attacker == null) return;
        attacker.markedInterceptTarget = null;
        attacker.markedInterceptMillCount = 0;
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
        CardType resolvedType = type == CardType.Token ? CardType.Monster : type;

        Transform[] slots = isPlayer
            ? (resolvedType == CardType.Monster ? playerMonsterSlots : playerEquipSlots)
            : (resolvedType == CardType.Monster ? enemyMonsterSlots : enemyEquipSlots);

        foreach (Transform t in slots) if (t.childCount == 0) return t;
        return null;
    }

    // 🔥 ตรวจสอบว่าการ์ดอยู่ในพื้นที่ผู้เล่นหรือไม่
    public bool IsCardInPlayerArea(BattleCardUI card)
    {
        if (card == null || card.transform.parent == null) return false;
        Transform parent = card.transform.parent;

        // เช็คว่าอยู่ใน Monster Slots ของผู้เล่น
        if (playerMonsterSlots != null)
        {
            foreach (var slot in playerMonsterSlots)
            {
                if (parent == slot) return true;
            }
        }

        // เช็คว่าอยู่ใน Equip Slots ของผู้เล่น
        if (playerEquipSlots != null)
        {
            foreach (var slot in playerEquipSlots)
            {
                if (parent == slot) return true;
            }
        }

        return false;
    }

    BattleCardUI GetBestEnemyEquip(SubCategory cat)
    {
        // 🧠 บอทตัดสินใจว่าจะกันหรือไม่
        if (!ShouldBotBlock(cat))
        {
            Debug.Log($"🤖 บอทตัดสินใจปล่อยให้โจมตีเข้า!");
            return null; // ไม่กัน
        }

        // 🔥 หาโล่ที่ตรงประเภทก่อน (กันได้ดีที่สุด) และไม่ถูกปิดการกัน และไม่มี bypass
        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var s = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (s != null && s.GetData() != null && !s.cannotIntercept && !s.canBypassIntercept
                    && (s.GetModifiedSubCategory() == cat || DoesBlockerAlwaysMatchTypeOnIntercept(s, blockerIsPlayer: false)))
                {
                    Debug.Log($"🛡️ บอทเลือกกันด้วย {s.GetData().cardName} (ประเภทตรง)");
                    return s;
                }
            }
        }

        // ถ้าไม่มีโล่ตรงประเภท เลือกโล่ตัวแรกที่มี (และไม่ถูกปิดการกัน แลด bypass)
        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var s = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (s != null && s.GetData() != null && !s.cannotIntercept && !s.canBypassIntercept)
                {
                    Debug.Log($"🛡️ บอทเลือกกันด้วย {s.GetData().cardName} (ประเภทต่าง)");
                    return s;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 🧠 ตัดสินใจว่าบอทจะกันหรือไม่ (มีกลยุทธ์)
    /// </summary>
    bool ShouldBotBlock(SubCategory attackerCategory)
    {
        // คำนวณเปอร์เซ็นต์ HP ของบอท
        float hpPercent = (float)enemyCurrentHP / enemyMaxHP;

        // 1. HP ต่ำมาก (< 30%) → กันแน่นอน (100%)
        if (hpPercent < 0.3f)
        {
            Debug.Log($"🩸 HP ต่ำมาก ({hpPercent:P0}) → กันแน่นอน!");
            return true;
        }

        // 2. HP ปานกลาง (30-60%) → กัน 70% ของเวลา
        if (hpPercent < 0.6f)
        {
            bool shouldBlock = Random.value < 0.7f;
            Debug.Log($"⚠️ HP ปานกลาง ({hpPercent:P0}) → กัน {(shouldBlock ? "✓" : "✗")}");
            return shouldBlock;
        }

        // 3. HP สูง (> 60%) → กันเฉพาะบางครั้ง (40%)
        // แต่ถ้ามีโล่ที่ตรงประเภท เพิ่มโอกาสเป็น 60%
        bool hasMatchingShield = false;
        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var s = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (s != null && s.GetData() != null
                    && (s.GetModifiedSubCategory() == attackerCategory || DoesBlockerAlwaysMatchTypeOnIntercept(s, blockerIsPlayer: false)))
                {
                    hasMatchingShield = true;
                    break;
                }
            }
        }

        float blockChance = hasMatchingShield ? 0.6f : 0.4f;
        bool willBlock = Random.value < blockChance;
        Debug.Log($"💚 HP สูง ({hpPercent:P0}) → โอกาสกัน {blockChance:P0} → {(willBlock ? "กัน ✓" : "ปล่อยเข้า ✗")}");
        return willBlock;
    }

    bool HasEquipInSlots(Transform[] slots)
    {
        foreach (Transform t in slots) if (t.childCount > 0) return true;
        return false;
    }

    void ResetAllMonstersAttackState()
    {
        // 🔥 รีเซ็ต Monster ผู้เล่น
        foreach (Transform slot in playerMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c)
                {
                    c.hasAttacked = false;
                    c.attacksThisTurn = 0; // รีเซ็ตจำนวนครั้งที่โจมตี
                    c.canBypassIntercept = false; // รีเซ็ต Bypass ตอนเริ่มเทิร์น
                    c.bypassCostThreshold = 0;
                    c.bypassAllowedMainCat = MainCategory.General;
                    c.bypassAllowedSubCat = SubCategory.General;

                    // 🕒 ลดจำนวนเทิร์น category loss และคืน category ถ้าหมดเวลา
                    c.ProcessCategoryLossDuration();

                    // 🔥 แก้: ตรวจสอบ Image ก่อน และให้แน่ใจว่าแสดงหน้าการ์ด
                    var img = c.GetComponent<Image>();
                    if (img != null && !c.hasLostCategory) // 🟣 ห้ามทับสีม่วง
                    {
                        img.color = Color.white; // คืนสี
                        // ตรวจสอบว่าแสดงหน้าการ์ดถูกต้อง
                        if (c.GetData() != null && c.GetData().artwork != null)
                        {
                            img.sprite = c.GetData().artwork;
                        }
                    }
                }
            }
        }

        // 🔥 รีเซ็ต Equip ผู้เล่น
        foreach (Transform slot in playerEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c)
                {
                    c.cannotIntercept = false; // รีเซ็ตการปิดการกัน (สำหรับ DisableIntercept)
                                               // mustIntercept จะรีเซ็ตหลังการกันสำเร็จ ไม่ต้องรีเซ็ตทุกเทิร์น

                    // 🕒 ลดจำนวนเทิร์น category loss และคืน category ถ้าหมดเวลา
                    c.ProcessCategoryLossDuration();
                }
            }
        }
    }

    // 🔥 รีเซ็ตสถานะมอนสเตอร์บอท (เอาไว้ใช้ตอนเริ่มเทิร์นบอท)
    void ResetAllEnemyMonstersAttackState()
    {
        // 🔥 รีเซ็ต Monster บอท
        foreach (Transform slot in enemyMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c)
                {
                    c.hasAttacked = false;
                    c.attacksThisTurn = 0; // รีเซ็ตจำนวนครั้งที่โจมตี
                    c.canBypassIntercept = false; // รีเซ็ต Bypass
                    c.bypassCostThreshold = 0;
                    c.bypassAllowedMainCat = MainCategory.General;
                    c.bypassAllowedSubCat = SubCategory.General;

                    // 🕒 ลดจำนวนเทิร์น category loss และคืน category ถ้าหมดเวลา
                    c.ProcessCategoryLossDuration();

                    // 🟣 คืนสี (ยกเว้นถ้าสูญเสีย category)
                    if (!c.hasLostCategory)
                    {
                        c.GetComponent<Image>().color = Color.white;
                    }
                }
            }
        }

        // 🔥 รีเซ็ต Equip บอท
        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c)
                {
                    c.cannotIntercept = false; // รีเซ็ตการปิดการกัน

                    // 🕒 ลดจำนวนเทิร์น category loss และคืน category ถ้าหมดเวลา
                    c.ProcessCategoryLossDuration();
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
            AddBattleLog($"Player tried to draw {n} but only {deckList.Count} left - LOSE");
            StartCoroutine(EndBattle(false));
            yield break;
        }

        // 📊 บันทึกสถิติ: การ์ดที่จั่ว
        currentBattleStats.cardsDrawn += n;

        AddBattleLog($"Player draws {n} card(s) | Deck: {deckList.Count}");

        Transform targetParent = parentOverride != null ? parentOverride : handArea;

        // 🔴 Debug: เช็ค handArea และ cardPrefab
        if (!handArea) Debug.LogError("❌ handArea ยังไม่ถูกตั้งค่า!");
        if (!cardPrefab) Debug.LogError("❌ cardPrefab ยังไม่ถูกตั้งค่า!");
        if (!targetParent) Debug.LogError("❌ targetParent เป็น null!");

        for (int i = 0; i < n; i++)
        {
            CardData d = deckList[0];
            deckList.RemoveAt(0);

            if (targetParent && cardPrefab)
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

        // 🎴 อัพเดทการแสดงผลเด็ค
        UpdateDeckVisualization();
    }

    IEnumerator DrawEnemyCard(int n)
    {
        if (enemyDeckList.Count < n)
        {
            Debug.LogWarning("⚠️ Deck empty while drawing (enemy)");
            AddBattleLog($"Bot tried to draw {n} but only {enemyDeckList.Count} left - BOT LOSE");
            StartCoroutine(EndBattle(true));
            yield break;
        }

        AddBattleLog($"Bot draws {n} card(s) | Deck: {enemyDeckList.Count}");

        // 👁️ เช็คว่าผู้เล่นมีสกิล [Cont.] RevealHand หรือไม่
        bool shouldRevealDrawnCard = HasPlayerContinuousRevealHandEffect();

        for (int i = 0; i < n; i++)
        {
            CardData cardData = enemyDeckList[0];
            enemyDeckList.RemoveAt(0);

            // 👁️ ถ้าผู้เล่นมีสกิล RevealHand ให้แสดงการ์ดที่บอทจั่ว
            if (shouldRevealDrawnCard)
            {
                // รอให้การ์ดเข้ามือก่อน (animation + setup)
                yield return new WaitForSeconds(0.5f);
            }

            if (cardPrefab && enemyHandArea)
            {
                // หาตำแหน่งเด็คบอท
                Vector3 startPos = Vector3.zero;

                if (enemyDeckPileTransform != null)
                {
                    startPos = enemyDeckPileTransform.position;
                }
                else if (deckPileTransform != null)
                {
                    // ใช้เด็คผู้เล่นถ้าไม่มีเด็คบอท
                    startPos = deckPileTransform.position;
                }
                else
                {
                    startPos = new Vector3(500, 0, 0); // default position ขวาบน
                }

                // สร้างการ์ดจริง (ต้องเพื่อให้บอทรู้ว่าการ์ดนั้นคืออะไร)
                GameObject cardObj = Instantiate(cardPrefab, startPos, Quaternion.identity);
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    cardObj.transform.SetParent(canvas.transform, worldPositionStays: true);
                }

                BattleCardUI ui = cardObj.GetComponent<BattleCardUI>();
                if (ui != null)
                {
                    cardObj.transform.localScale = Vector3.zero;
                    ui.Setup(cardData);

                    // ซ่อนรูปการ์ดแสดงแค่หลังการ์ด
                    var img = cardObj.GetComponent<Image>();
                    if (img != null)
                    {
                        // เปลี่ยนไปแสดงหลังการ์ด (cardBackPrefab sprite)
                        if (cardBackPrefab != null)
                        {
                            var backImg = cardBackPrefab.GetComponent<Image>();
                            if (backImg != null && backImg.sprite != null)
                                img.sprite = backImg.sprite;
                        }
                    }
                    // ซ่อนกรอบสำหรับการ์ดหลัง
                    ui.SetFrameVisible(false);

                    // อนิเมชั่นบินไปมือบอท
                    float duration = 0.3f;
                    float elapsed = 0f;
                    Vector3 endPos = enemyHandArea.position;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / duration;
                        float easeT = 1f - Mathf.Pow(1f - t, 3);

                        cardObj.transform.position = Vector3.Lerp(startPos, endPos, easeT);
                        cardObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT);

                        yield return null;
                    }

                    // Snap เข้ามือบอท
                    cardObj.transform.SetParent(enemyHandArea, false);
                    cardObj.transform.localScale = Vector3.one;

                    // ตั้งค่าการ์ดบอท
                    var cg = cardObj.GetComponent<CanvasGroup>();
                    if (cg == null) cg = cardObj.AddComponent<CanvasGroup>();
                    cg.blocksRaycasts = false; // ซ่อนมือบอทปกติจะไม่ให้คลิก
                    cg.interactable = true;

                    var le = cardObj.GetComponent<LayoutElement>();
                    if (le == null) le = cardObj.AddComponent<LayoutElement>();
                    le.preferredWidth = handCardPreferredSize.x;
                    le.preferredHeight = handCardPreferredSize.y;

                    // 👁️ ถ้ามีสกิล RevealHand ให้แสดงหน้าการ์ดตลอดเทิร์นนี้
                    if (shouldRevealDrawnCard)
                    {
                        // แสดงหน้าการ์ด (ไม่พลิก) - แสดงพิศเลยตลอดเทิร์น
                        var revealImg = cardObj.GetComponent<Image>();
                        if (revealImg != null && cardData.artwork != null)
                        {
                            revealImg.sprite = cardData.artwork; // แสดงหน้าการ์ดจริง
                            revealImg.raycastTarget = true; // ✅ บังคับให้รับเมาส์ click
                            Debug.Log($"🖼️ Set raycast target to TRUE for {cardData.cardName}");
                        }
                        ui.SetFrameVisible(true); // แสดงกรอบ
                        cg.blocksRaycasts = true; // 🔥 อนุญาตให้คลิกการ์ดที่ถูก reveal
                        
                        // บันทึกการ์ดที่ reveal เพื่อจัดที่ออกมือ
                        revealedEnemyCards[cardData.card_id] = cardData;
                        AddBattleLog($"👁️ [RevealHand] Enemy drew: {cardData.cardName}");
                        Debug.Log($"👁️ Added to revealedEnemyCards: {cardData.cardName} (id={cardData.card_id}), Total revealed: {revealedEnemyCards.Count}");
                    }
                }

                // พักเล็กน้อยระหว่างการ์ด
                yield return new WaitForSeconds(0.1f);
            }
        }

        ArrangeEnemyHand();

        // 🎴 อัพเดทการแสดงผลเด็ค
        UpdateDeckVisualization();
    }

    void ShuffleList(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData t = list[i];
            int r = Random.Range(i, list.Count);
            list[i] = list[r];
            list[r] = t;
        }
    }

    void PlayerTakeDamage(int d)
    {
        currentHP = Mathf.Max(0, currentHP - d);

        // 📊 บันทึกสถิติ: ดาเมจที่ได้รับ
        currentBattleStats.totalDamageTaken += d;

        AddBattleLog($"Player takes {d} damage | HP: {currentHP + d} -> {currentHP}");

        // Safe Check
        if (playerSpot) ShowDamagePopupString($"-{d}", playerSpot);
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("Damage");
        StartCoroutine(ScreenShake(0.15f, 6f));

        UpdateUI();

        if (currentHP <= 0)
        {
            Debug.Log("LOSE (HP=0)");
            AddBattleLog("Player HP reaches 0 - LOSE");
            StartCoroutine(EndBattle(false));
        }
    }

    void EnemyTakeDamage(int d)
    {
        enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - d);

        // 📊 บันทึกสถิติ: ดาเมจที่ทำให้ศัตรู
        currentBattleStats.totalDamageDealt += d;

        AddBattleLog($"Bot takes {d} damage | HP: {enemyCurrentHP + d} -> {enemyCurrentHP}");

        if (enemySpot) ShowDamagePopupString($"-{d}", enemySpot);
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("Damage");
        StartCoroutine(ScreenShake(0.12f, 5f));
        UpdateUI();

        if (enemyCurrentHP <= 0)
        {
            Debug.Log("WIN (enemy HP=0)");
            AddBattleLog("Bot HP reaches 0 - WIN");
            StartCoroutine(EndBattle(true));
        }
    }

    // DAILY QUEST UPDATE
    void CheckBattleDailyQuests(BattleStatistics stats)
    {
        Debug.Log("[CheckBattleDailyQuests] Called with stats=" + (stats != null ? "valid" : "NULL"));

        if (DailyQuestManager.Instance == null)
        {
            Debug.LogWarning("❌ [CheckBattleDailyQuests] DailyQuestManager.Instance is NULL! Cannot update quests.");
            return;
        }

        Debug.Log("🏆 Checking Daily Quests from Battle Stats...");

        Debug.Log($"📊 Battle Stats: Victory={stats.victory}, CardsPlayed={stats.totalCardsPlayed}, DamageDealt={stats.totalDamageDealt}");
        //ไม่สนแพ้ชนะ แค่จบเกมก็นับ 1
        DailyQuestManager.Instance.UpdateProgress(QuestType.Stage, 1, "play_1");

        // เล่นชนะ 1 ครั้ง
        if (stats.victory)
        {
            DailyQuestManager.Instance.UpdateProgress(QuestType.Stage, 1, "win_1");
        }

        // เควสใช้การ์ด
        if (stats.totalCardsPlayed > 0)
        {
            DailyQuestManager.Instance.UpdateProgress(QuestType.Stage, stats.totalCardsPlayed, "use_card_20");
        }

        // เควสทำดาเมจ
        if (stats.totalDamageDealt > 0)
        {
            DailyQuestManager.Instance.UpdateProgress(QuestType.Stage, stats.totalDamageDealt, "damage_50");
        }
    }

    IEnumerator EndBattle(bool playerWin)
    {
        if (isEnding) yield break;

        isEnding = true;
        state = playerWin ? BattleState.WON : BattleState.LOST;

        // 📊 บันทึกสถิติสุดท้าย
        int deckRemaining = deckList != null ? deckList.Count : 0;
        int handSize = handArea != null ? handArea.childCount : 0;
        currentBattleStats.Finalize(playerWin, currentHP, enemyCurrentHP, turnCount, deckRemaining, handSize);

        // เก็บสถิติไว้ให้เข้าถึงได้จากภายนอก
        LastBattleStats = currentBattleStats;

        // อัพเดทเควสรายวัน
        Debug.Log("[EndBattle] About to call CheckBattleDailyQuests with stats...");
        CheckBattleDailyQuests(LastBattleStats);
        Debug.Log("[EndBattle] CheckBattleDailyQuests completed.");

        // 💾 บันทึกลงประวัติ
        if (BattleHistory.Instance != null)
        {
            BattleHistory.Instance.AddBattleResult(currentBattleStats);
            Debug.Log($"💾 Battle result saved to history (Total: {BattleHistory.Instance.GetTotalBattles()})");
        }

        // ⭐ ตรวจสอบและบันทึกดาว (ถ้าชนะ และมี StageID)
        if (playerWin)
        {
            string currentStageID = PlayerPrefs.GetString("CurrentStageID", "");
            if (!string.IsNullOrEmpty(currentStageID))
            {
                int starsEarned = CalculateStarsForCurrentStage(currentBattleStats, currentStageID);
                Debug.Log($"Earned {starsEarned}/3 stars for stage {currentStageID}");
                Debug.Log($"[DEBUG] Stats - Victory: {currentBattleStats.victory}, Turns: {currentBattleStats.turnsPlayed}, Spells: {currentBattleStats.spellsCast}");

                // บันทึกลง GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CompleteStage(currentStageID, starsEarned, currentBattleStats);
                }
            }
        }

        // แสดงสถิติใน Console
        Debug.Log("\n" + currentBattleStats.GetSummary());

        // เพิ่มสถิติใน Battle Log
        AddBattleLog("\n=== BATTLE STATISTICS ===");
        AddBattleLog($"Result: {(playerWin ? "VICTORY" : "DEFEAT")} | Turns: {turnCount}");
        AddBattleLog($"Final HP: Player {currentHP}/{maxHP} | Enemy {enemyCurrentHP}/{enemyMaxHP}");
        AddBattleLog($"Cards Played: {currentBattleStats.totalCardsPlayed} (M:{currentBattleStats.monsterCardsPlayed} S:{currentBattleStats.spellCardsPlayed} E:{currentBattleStats.equipCardsPlayed})");
        AddBattleLog($"Damage: Dealt {currentBattleStats.totalDamageDealt} | Taken {currentBattleStats.totalDamageTaken}");
        if (currentBattleStats.perfectVictory) AddBattleLog("🏆 Perfect Victory!");
        if (currentBattleStats.quickVictory) AddBattleLog("⚡ Quick Victory!");

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
        if (!damagePopupPrefab || !pos) return;

        var go = Instantiate(damagePopupPrefab);
        var canvas = FindObjectOfType<Canvas>();
        if (canvas) go.transform.SetParent(canvas.transform, false);

        var rect = go.transform as RectTransform;
        var worldPos = pos.position;
        if (rect)
        {
            rect.position = Camera.main ? Camera.main.WorldToScreenPoint(worldPos) : worldPos;
        }
        else
        {
            go.transform.position = worldPos;
        }

        var popup = go.GetComponent<DamagePopup>();
        if (popup) popup.Setup(t);
    }

    void UpdateUI()
    {
        // ใส่ ? กัน Error
        if (playerHPBar) playerHPBar.value = currentHP;
        if (enemyHPBar) enemyHPBar.value = enemyCurrentHP;
        if (ppText) ppText.text = $"{currentPP}/{maxPP} PP";
        if (enemyPPText) enemyPPText.text = $"{enemyCurrentPP}/{enemyMaxPP} PP";
        if (playerHPText) playerHPText.text = $"{currentHP}/{maxHP}";
        if (enemyHPText) enemyHPText.text = $"{enemyCurrentHP}/{enemyMaxHP}";

        // 🎴 อัพเดทจำนวนการ์ดในเด็ค
        UpdateDeckCountUI();
        UpdateGraveyardCountUI();
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
        // 🔥 EquipSpell ไม่มี Summoning Sickness
        if (newData.type != CardType.EquipSpell)
        {
            newCard.hasAttacked = true; // Monster ต้องรอเทิร์นถัดไป

            // 🔥 เช็คว่ามีสกิล Rush หรือไม่ (Sacrifice)
            bool hasRush = HasActiveRush(newCard);
            if (hasRush)
            {
                newCard.hasAttacked = false;
                AddBattleLog($"💨 <color=cyan>{newData.cardName}</color> มีสกิล Rush! สามารถโจมตีได้ทันที");
            }
        }
        // 🟣 ตั้งสี (ยกเว้นถ้าสูญเสีย category)
        if (!newCard.hasLostCategory)
        {
            newCard.GetComponent<Image>().color = Color.white; // ไม่เป็นสีเทา
        }
        newCard.UpdateCardSize(); // 🔥 ปรับขนาดการ์ดบนสนาม
        // แสดงกรอบเมื่อการ์ดหงายหน้า
        newCard.SetFrameVisible(true);

        // 🪦 ส่งการ์ดเก่าลงสุสาน
        DestroyCardToGraveyard(oldCard);

        // เล่นเสียง
        if (AudioManager.Instance) AudioManager.Instance.PlaySFX("CardSelect");

        // 🔥 ทริกเกอร์ OnDeploy สำหรับการ์ดที่สังเวย (เหมือนการลงปกติ)
        StartCoroutine(ResolveEffects(newCard, EffectTrigger.OnDeploy, isPlayer: true));

        AddBattleLog($"Player sacrificed {oldData.cardName} to play {newData.cardName}");
        UpdateUI();
        Debug.Log($"✅ Sacrifice สำเร็จ!");
    }

    // --------------------------------------------------------
    // 🎴 DECK VISUALIZATION SYSTEM
    // --------------------------------------------------------

    void CreateDeckVisualization()
    {
        // สร้างหลังการ์ดซ้อนกันสำหรับเด็คผู้เล่น
        if (deckPileTransform != null && cardBackPrefab != null)
        {
            ClearDeckVisualization(playerDeckVisuals);
            int cardsToShow = Mathf.Min(deckList.Count, deckVisualizationCount);

            for (int i = 0; i < cardsToShow; i++)
            {
                GameObject cardBack = Instantiate(cardBackPrefab, deckPileTransform);
                RectTransform rect = cardBack.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(140, 200);
                    // ซ้อนกันเล็กน้อย (offset ตาม index)
                    rect.anchoredPosition = new Vector2(i * 2f, -i * 2f);
                }
                cardBack.transform.SetAsFirstSibling(); // การ์ดล่างสุดอยู่ข้างหลัง
                playerDeckVisuals.Add(cardBack);
            }
        }

        // สร้างหลังการ์ดซ้อนกันสำหรับเด็คบอท
        if (enemyDeckPileTransform != null && cardBackPrefab != null)
        {
            ClearDeckVisualization(enemyDeckVisuals);
            int cardsToShow = Mathf.Min(enemyDeckList.Count, deckVisualizationCount);

            for (int i = 0; i < cardsToShow; i++)
            {
                GameObject cardBack = Instantiate(cardBackPrefab, enemyDeckPileTransform);
                RectTransform rect = cardBack.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(140, 200);
                    rect.anchoredPosition = new Vector2(i * 2f, -i * 2f);
                }
                cardBack.transform.SetAsFirstSibling();
                enemyDeckVisuals.Add(cardBack);
            }
        }

        UpdateDeckCountUI();
    }

    void UpdateDeckVisualization()
    {
        // อัพเดทเด็คผู้เล่น
        if (deckPileTransform != null && cardBackPrefab != null)
        {
            int currentVisualCount = playerDeckVisuals.Count;
            int targetVisualCount = Mathf.Min(deckList.Count, deckVisualizationCount);

            // ถ้าเด็คลดลง ให้ลบการ์ดด้านบนออก
            while (currentVisualCount > targetVisualCount && playerDeckVisuals.Count > 0)
            {
                int lastIndex = playerDeckVisuals.Count - 1;
                if (playerDeckVisuals[lastIndex] != null)
                {
                    Destroy(playerDeckVisuals[lastIndex]);
                }
                playerDeckVisuals.RemoveAt(lastIndex);
                currentVisualCount--;
            }

            // ถ้าเด็คเพิ่มขึ้น (กรณี reshuffle หรือเพิ่มการ์ด) ให้เพิ่มการ์ด
            while (currentVisualCount < targetVisualCount)
            {
                GameObject cardBack = Instantiate(cardBackPrefab, deckPileTransform);
                RectTransform rect = cardBack.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(140, 200);
                    rect.anchoredPosition = new Vector2(currentVisualCount * 2f, -currentVisualCount * 2f);
                }
                cardBack.transform.SetAsFirstSibling();
                playerDeckVisuals.Add(cardBack);
                currentVisualCount++;
            }
        }

        // อัพเดทเด็คบอท
        if (enemyDeckPileTransform != null && cardBackPrefab != null)
        {
            int currentVisualCount = enemyDeckVisuals.Count;
            int targetVisualCount = Mathf.Min(enemyDeckList.Count, deckVisualizationCount);

            while (currentVisualCount > targetVisualCount && enemyDeckVisuals.Count > 0)
            {
                int lastIndex = enemyDeckVisuals.Count - 1;
                if (enemyDeckVisuals[lastIndex] != null)
                {
                    Destroy(enemyDeckVisuals[lastIndex]);
                }
                enemyDeckVisuals.RemoveAt(lastIndex);
                currentVisualCount--;
            }

            while (currentVisualCount < targetVisualCount)
            {
                GameObject cardBack = Instantiate(cardBackPrefab, enemyDeckPileTransform);
                RectTransform rect = cardBack.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(140, 200);
                    rect.anchoredPosition = new Vector2(currentVisualCount * 2f, -currentVisualCount * 2f);
                }
                cardBack.transform.SetAsFirstSibling();
                enemyDeckVisuals.Add(cardBack);
                currentVisualCount++;
            }
        }

        UpdateDeckCountUI();
    }

    void ClearDeckVisualization(List<GameObject> visualList)
    {
        foreach (var card in visualList)
        {
            if (card != null) Destroy(card);
        }
        visualList.Clear();
    }

    void UpdateDeckCountUI()
    {
        if (playerDeckCountText != null)
        {
            playerDeckCountText.text = deckList.Count.ToString();
        }

        if (enemyDeckCountText != null)
        {
            enemyDeckCountText.text = enemyDeckList.Count.ToString();
        }
    }

    // ========================================================
    // 🔥 EFFECT RESOLUTION SYSTEM
    // ========================================================

    /// <summary>วน effects ทั้งหมดของการ์ดตามเงื่อนไข trigger ที่กำหนด (รอให้แต่ละ effect เสร็จก่อนไปยัง effect ถัดไป)</summary>
    IEnumerator ResolveEffects(BattleCardUI sourceCard, EffectTrigger triggerType, bool isPlayer)
    {
        if (sourceCard == null) yield break;
        var cardData = sourceCard.GetData();
        if (cardData == null || cardData.effects == null || cardData.effects.Count == 0) yield break;

        foreach (var effect in cardData.effects)
        {
            if (effect.trigger == triggerType)
            {
                if (IsEffectSuppressedByOpponentContinuousAura(sourceCard, effect, triggerType, isPlayer))
                {
                    Debug.Log($"🚫 Effect suppressed: {cardData.cardName} | Trigger={triggerType} | Action={effect.action}");
                    AddBattleLog($"{cardData.cardName} ability was suppressed");
                    continue;
                }

                // 🔥 รอให้ ApplyEffect เสร็จก่อนไปยัง effect ถัดไป (สำหรับ Destroy ที่มี async target selection)
                yield return StartCoroutine(ApplyEffect(sourceCard, effect, isPlayer));
            }
        }
    }

    /// <summary>ทำการแอคชันตามประเภท effect ที่กำหนด (เป็น coroutine เพื่อรองรับ async operations)</summary>
    IEnumerator ApplyEffect(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        Debug.Log($"🔥 Apply Effect: {sourceCard.GetData().cardName} | Trigger: {effect.trigger} | Action: {effect.action} | Target: {effect.targetType} | Value: {effect.value} | MainCat: {effect.targetMainCat} | SubCat: {effect.targetSubCat}");

        switch (effect.action)
        {
            case ActionType.Destroy:
                yield return StartCoroutine(ApplyDestroy(sourceCard, effect, isPlayer));
                break;
            case ActionType.HealHP:
                yield return StartCoroutine(ApplyHeal(sourceCard, effect, isPlayer));
                break;
            case ActionType.DrawCard:
                ApplyDrawCard(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.SummonToken:
                ApplySummonToken(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.RevealHand:
                ApplyRevealHand(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.RevealHandMultiple:
                ApplyRevealHandMultiple(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.DiscardDeck:
                ApplyDiscardDeck(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.PeekDiscardTopDeck:
                yield return StartCoroutine(ApplyPeekDiscardTopDeckCoroutine(sourceCard, effect, isPlayer));
                break;
            case ActionType.MarkInterceptMillDeck:
                yield return StartCoroutine(ApplyMarkInterceptMillDeck(sourceCard, effect, isPlayer));
                break;
            case ActionType.ForceChooseDiscard:
                yield return StartCoroutine(ApplyForceChooseDiscardCoroutine(sourceCard, effect, isPlayer));
                break;
            case ActionType.DisableAttack:
                ApplyDisableAttack(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.DisableAbility:
                ApplyDisableAbility(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.ModifyStat:
                ApplyModifyStat(sourceCard, effect, isPlayer);
                yield break;
            case ActionType.ZeroStats:
                yield return StartCoroutine(ApplyZeroStats(sourceCard, effect, isPlayer));
                break;
            case ActionType.BypassIntercept:
                yield return StartCoroutine(ApplyBypassIntercept(sourceCard, effect, isPlayer));
                break;
            case ActionType.ForceIntercept:
                yield return StartCoroutine(ApplyForceIntercept(sourceCard, effect, isPlayer));
                break;
            case ActionType.DisableIntercept:
                yield return StartCoroutine(ApplyDisableIntercept(sourceCard, effect, isPlayer));
                break;
            case ActionType.RemoveCategory:
                yield return StartCoroutine(ApplyRemoveCategory(sourceCard, effect, isPlayer));
                break;
            case ActionType.ControlEquip:
                yield return StartCoroutine(ApplyControlEquip(sourceCard, effect, isPlayer));
                break;
            case ActionType.ReturnEquipFromGraveyard:
                yield return StartCoroutine(ApplyReturnEquipFromGraveyard(sourceCard, effect, isPlayer));
                break;
            default:
                Debug.LogWarning($"⚠️ Action type {effect.action} not implemented yet");
                yield break;
        }
    }

    // --- Effect Implementations ---

    /// <summary>รอให้ผู้เล่นเลือกเป้าหมาย (Coroutine wrapper สำหรับ async target selection)</summary>
    IEnumerator WaitForTargetSelection(List<BattleCardUI> targets, int selectCount)
    {
        List<BattleCardUI> result = new List<BattleCardUI>();
        bool completed = false;

        // เรียก StartSelectingTarget กับ callback ที่เซ็ต completed เป็น true
        StartSelectingTarget(targets, selectCount, (selectedCards) =>
        {
            result = selectedCards;
            completed = true;
        });

        // รอจนกว่า callback จะเรียก
        while (!completed)
        {
            yield return null;
        }

        // ส่งกลับ result ให้ caller (ผ่านทาง selectedTargets)
        selectedTargets = result;
    }

    IEnumerator ApplyDestroy(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);
        int destroyedAtkSum = 0;

        // 🔥 ตรวจสอบโหมดการทำลาย
        if (effect.destroyMode == DestroyMode.DestroyAll)
        {
            // ⚡ โหมด DestroyAll: ทำลายทั้งหมดที่ตรงเงื่อนไข (MainCategory/SubCategory) ทันที โดยไม่รอให้เลือก
            Debug.Log($"⚡ DestroyAll Mode: ทำลายการ์ด {targets.Count} ใบทันที (ตรงตามประเภท)");
            foreach (var target in targets)
            {
                if (target != null && target.GetData() != null)
                {
                    Debug.Log($"💥 Destroy (DestroyAll): {target.GetData().cardName}");
                    var targetData = ResolveCardData(target);
                    if (targetData != null) destroyedAtkSum += targetData.atk;
                    DestroyCardToGraveyard(target);
                }
            }
            Debug.Log($"✅ ApplyDestroy (DestroyAll): ทำลายการ์ด {targets.Count} ใบ");
        }
        else
        {
            // 📋 โหมด SelectTarget: ให้ผู้เล่นเลือก หรือบอทเลือกอัตโนมัติ
            int maxDestroy = effect.value > 0 ? effect.value : targets.Count;

            Debug.Log($"🎯 SelectTarget Mode: พบเป้าหมาย {targets.Count} ใบ, ต้องเลือก {maxDestroy} ใบ");

            // 🔥 ผู้เล่นต้องเลือกเป้าหมายเสมอถ้ามีเป้าหมาย
            if (isPlayer && maxDestroy > 0 && targets.Count > 0)
            {
                // 🔥 รอให้ผู้เล่นเลือกเป้าหมาย ก่อนไปยัง effect ถัดไป
                yield return StartCoroutine(WaitForTargetSelection(targets, maxDestroy));

                int destroyCount = 0;
                foreach (var target in selectedTargets)
                {
                    if (destroyCount >= maxDestroy) break;
                    if (target != null && target.GetData() != null)
                    {
                        Debug.Log($"💥 Destroy ({destroyCount + 1}/{maxDestroy}): {target.GetData().cardName}");
                        var targetData = ResolveCardData(target);
                        if (targetData != null) destroyedAtkSum += targetData.atk;
                        DestroyCardToGraveyard(target);
                        destroyCount++;
                    }
                }
                Debug.Log($"✅ ApplyDestroy (SelectTarget): ผู้เล่นเลือกทำลาย {destroyCount} ใบ");
            }
            else
            {
                // 🤖 บอททำลายโดยอัตโนมัติ
                int destroyCount = 0;
                foreach (var target in targets)
                {
                    if (destroyCount >= maxDestroy) break;
                    if (target != null && target.GetData() != null)
                    {
                        Debug.Log($"💥 Destroy ({destroyCount + 1}/{maxDestroy}): {target.GetData().cardName}");
                        var targetData = ResolveCardData(target);
                        if (targetData != null) destroyedAtkSum += targetData.atk;
                        DestroyCardToGraveyard(target);
                        destroyCount++;
                    }
                }
                Debug.Log($"✅ ApplyDestroy (SelectTarget - Bot): ทำลายการ์ด {destroyCount}/{targets.Count} ใบ (Max: {maxDestroy})");
            }
        }

        // เก็บค่า ATK ของเป้าหมายที่ถูกทำลายเพื่อใช้ในการ Heal
        lastDestroyedAtkSum = destroyedAtkSum;
    }

    IEnumerator ApplyHeal(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (sourceCard == null)
        {
            Debug.LogError("❌ ApplyHeal: sourceCard เป็น null!");
            yield break;
        }

        CardData cardData = ResolveCardData(sourceCard);
        if (cardData == null)
        {
            Debug.LogError("❌ ApplyHeal: CardData เป็น null!");
            yield break;
        }

        Debug.Log($"🔍 ApplyHeal: sourceCard.name={sourceCard.name}, cardName={cardData.cardName}, cardData.atk={cardData.atk}, cardData.hp={cardData.hp}");

        int atkValue = cardData.atk;
        int hpValue = cardData.hp;

        Debug.Log($"🔍 atkValue={atkValue}, hpValue={hpValue}, effect.value={effect.value}");

        int healAmount = effect.value;

        if (healAmount <= 0)
        {
            if (lastDestroyedAtkSum > 0)
            {
                healAmount = lastDestroyedAtkSum;
                lastDestroyedAtkSum = 0; // ใช้ครั้งเดียว
            }
            else if (atkValue > 0)
            {
                healAmount = atkValue;
            }
            else if (hpValue > 0)
            {
                healAmount = hpValue;
            }
            else
            {
                healAmount = 0; // ไม่มีข้อมูลให้ใช้
            }
        }

        Debug.Log($"🔍 Final healAmount={healAmount}");

        if (effect.targetType == TargetType.Self)
        {
            if (isPlayer)
            {
                int hpBefore = currentHP;
                currentHP = Mathf.Min(currentHP + healAmount, maxHP);
                ShowDamagePopupString($"+{healAmount} HP", sourceCard.transform);
                Debug.Log($"💚 Heal Player: {healAmount} (HP: {hpBefore} -> {currentHP}/{maxHP})");
                AddBattleLog($"Player healed {healAmount} HP ({hpBefore} -> {currentHP})");
            }
            else // Bot heals itself
            {
                int hpBefore = enemyCurrentHP;
                enemyCurrentHP = Mathf.Min(enemyCurrentHP + healAmount, enemyMaxHP);
                ShowDamagePopupString($"+{healAmount} HP", sourceCard.transform);
                Debug.Log($"💚 Heal Enemy: {healAmount} (HP: {hpBefore} -> {enemyCurrentHP}/{enemyMaxHP})");
                AddBattleLog($"Enemy healed {healAmount} HP ({hpBefore} -> {enemyCurrentHP})");
            }
        }
        else if (effect.targetType == TargetType.EnemyPlayer)
        {
            if (isPlayer) // Player heals enemy
            {
                enemyCurrentHP = Mathf.Min(enemyCurrentHP + healAmount, enemyMaxHP);
                ShowDamagePopupString($"+{healAmount} HP", sourceCard.transform);
                Debug.Log($"💚 Player heals Enemy: {healAmount}");
                AddBattleLog($"Player healed Enemy {healAmount} HP");
            }
            else // Bot heals player
            {
                currentHP = Mathf.Min(currentHP + healAmount, maxHP);
                ShowDamagePopupString($"+{healAmount} HP", sourceCard.transform);
                Debug.Log($"💚 Enemy heals Player: {healAmount}");
                AddBattleLog($"Enemy healed Player {healAmount} HP");
            }
        }

        UpdateUI();
        yield break;
    }

    void ApplySummonToken(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (string.IsNullOrEmpty(effect.tokenCardId))
        {
            Debug.LogWarning($"⚠️ SummonToken: ไม่ได้ระบุ tokenCardId ในเอฟเฟกต์");
            return;
        }

        // หา CardData ของ Token จาก ID
        CardData tokenData = GetCardDataById(effect.tokenCardId);
        if (tokenData == null)
        {
            Debug.LogWarning($"⚠️ SummonToken: ไม่พบ Token card ด้วย ID: {effect.tokenCardId}");
            return;
        }

        Debug.Log($"🔍 Token Data: {tokenData.cardName}, ATK={tokenData.atk}, HP={tokenData.hp}, Type={tokenData.type}");

        // 🔥 ตรวจสอบว่าต้องการสร้าง Token ฝั่งไหน
        bool summonOnPlayerSide = isPlayer; // Default: สร้างฝั่งเดียวกับผู้เล่นการ์ด

        if (effect.targetType == TargetType.EnemyPlayer
            || effect.targetType == TargetType.EnemyMonster
            || effect.targetType == TargetType.EnemyEquip
            || effect.targetType == TargetType.EnemyHand
            || effect.targetType == TargetType.EnemyDeck
            || effect.targetType == TargetType.AllGlobal)
        {
            summonOnPlayerSide = !isPlayer; // สร้างฝั่งตรงข้าม
        }
        else if (effect.targetType == TargetType.Self)
        {
            summonOnPlayerSide = isPlayer; // สร้างฝั่งตัวเอง
        }

        Debug.Log($"🎯 SummonToken: sourceCard from {(isPlayer ? "Player" : "Bot")}, targetType={effect.targetType}, will summon on {(summonOnPlayerSide ? "Player" : "Bot")} side");

        // จำนวน token ที่จะ summon (ใช้ effect.value ถ้าระบุ, ไม่ให้ 1)
        int tokenCount = effect.value > 0 ? effect.value : 1;

        // หาตำแหน่ง monster slot ที่ว่าง
        int summoned = 0;

        for (int i = 0; i < tokenCount; i++)
        {
            Transform freeSlot = GetFreeSlot(CardType.Monster, summonOnPlayerSide);
            if (freeSlot == null)
            {
                Debug.LogWarning($"⚠️ SummonToken: ไม่มี slot ว่างสำหรับ token {i + 1}/{tokenCount}");
                break;
            }

            // สร้าง BattleCardUI สำหรับ Token
            if (cardPrefab == null)
            {
                Debug.LogError("❌ cardPrefab ยังไม่ถูกตั้งค่า!");
                break;
            }

            // สร้าง Token โดยไม่มี parent ก่อน
            GameObject cardObj = Instantiate(cardPrefab);
            BattleCardUI tokenUI = cardObj.GetComponent<BattleCardUI>();
            if (tokenUI == null)
            {
                Debug.LogError("❌ cardPrefab ไม่มี BattleCardUI component!");
                Destroy(cardObj);
                break;
            }

            // 🔥 สำคัญที่สุด: ต้อง Setup ก่อนแล้วค่อย SetParent
            tokenUI.Setup(tokenData);
            Debug.Log($"✅ Token Setup สำเร็จ: name={tokenUI.GetData()?.cardName}, atk={tokenUI.GetData()?.atk}, hp={tokenUI.GetData()?.hp}");

            // ตั้งตำแหน่ง - SetParent หลัง Setup
            tokenUI.transform.SetParent(freeSlot, false);
            tokenUI.transform.localPosition = Vector3.zero;
            tokenUI.transform.localScale = Vector3.one;

            tokenUI.isOnField = true; // ✅ Token อยู่บนสนาม
            tokenUI.hasAttacked = true; // Summoning Sickness
            tokenUI.UpdateCardSize(); // ปรับขนาดการ์ด

            // ตั้งสีเป็นเทา (Summoning Sickness)
            var img = tokenUI.GetComponent<Image>();
            if (img != null)
            {
                if (tokenData.artwork != null)
                    img.sprite = tokenData.artwork;
                img.color = Color.gray;
                img.raycastTarget = true; // 🔥 ให้คลิกได้
            }

            // อนุญาตให้ interact
            var cg = tokenUI.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }

            summoned++;

            Debug.Log($"🎯 Token สร้างสำเร็จ: {tokenData.cardName} (Slot: {freeSlot.name}) - GetData()={tokenUI.GetData()?.cardName}, isOnField={tokenUI.isOnField}, hasAttacked={tokenUI.hasAttacked}, onPlayerSide={summonOnPlayerSide}");
            AddBattleLog($"{(summonOnPlayerSide ? "Player" : "Bot")} summons {tokenData.cardName} (Token)");
        }

        if (summoned > 0)
        {
            UpdateUI();
            Debug.Log($"✅ Token สร้างเสร็จ {summoned}/{tokenCount} ตัว บนฝั่ง {(summonOnPlayerSide ? "Player" : "Bot")}");
        }
    }

    IEnumerator ApplyMarkInterceptMillDeck(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (sourceCard == null || sourceCard.GetData() == null)
        {
            yield break;
        }

        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer)
            .Where(t => t != null && t.GetData() != null && t.GetData().type == CardType.EquipSpell && !t.cannotIntercept)
            .Where(t => !CanAttackerBypassShield(sourceCard, t))
            .ToList();

        if (targets.Count == 0)
        {
            sourceCard.markedInterceptTarget = null;
            sourceCard.markedInterceptMillCount = 0;
            Debug.Log("⚠️ MarkInterceptMillDeck: ไม่มี Equip ที่ใช้ Intercept ได้");
            yield break;
        }

        BattleCardUI chosenTarget = null;
        if (isPlayer && targets.Count > 1)
        {
            yield return StartCoroutine(WaitForTargetSelection(targets, 1));
            if (selectedTargets != null && selectedTargets.Count > 0)
            {
                chosenTarget = selectedTargets[0];
            }
            selectedTargets.Clear();
        }
        else if (!isPlayer && targets.Count > 1)
        {
            int idx = Random.Range(0, targets.Count);
            chosenTarget = targets[idx];
        }
        else
        {
            chosenTarget = targets[0];
        }

        if (chosenTarget == null || chosenTarget.GetData() == null)
        {
            sourceCard.markedInterceptTarget = null;
            sourceCard.markedInterceptMillCount = 0;
            yield break;
        }

        int millCount = effect.value > 0 ? effect.value : 2;
        sourceCard.markedInterceptTarget = chosenTarget;
        sourceCard.markedInterceptMillCount = millCount;

        ShowDamagePopupString($"Marked: {chosenTarget.GetData().cardName}", chosenTarget.transform);
        AddBattleLog($"{sourceCard.GetData().cardName} marked {chosenTarget.GetData().cardName} ({millCount} deck discard on intercept)");
        Debug.Log($"🎯 MarkInterceptMillDeck: {sourceCard.GetData().cardName} -> {chosenTarget.GetData().cardName} (mill={millCount})");
    }

    void ApplyRevealHand(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (effect.targetType == TargetType.EnemyHand)
        {
            bool targetIsPlayer = !isPlayer;
            int sourceCardCost = sourceCard != null && sourceCard.GetData() != null ? sourceCard.GetData().cost : -1;
            if (IsHandRevealBlockedByContinuousEffect(targetIsPlayer, sourceCardCost, ActionType.RevealHand))
            {
                AddBattleLog($"{(isPlayer ? "Player" : "Bot")} tried to reveal hand but it was blocked");
                return;
            }
        }

        // ผู้เล่นดูมือบอท
        if (isPlayer && effect.targetType == TargetType.EnemyHand)
        {
            if (enemyHandArea != null && enemyHandArea.childCount > 0)
            {
                var firstCard = enemyHandArea.GetChild(0).GetComponent<BattleCardUI>();
                if (firstCard != null && cardDetailView != null)
                {
                    cardDetailView.Open(firstCard.GetData());
                    Debug.Log($"👁️ Player Reveal Enemy Hand: {firstCard.GetData().cardName}");
                    AddBattleLog($"Player revealed: {firstCard.GetData().cardName}");
                }
            }
        }
        // บอทดูมือผู้เล่น
        else if (!isPlayer && effect.targetType == TargetType.EnemyHand)
        {
            if (handArea != null && handArea.childCount > 0)
            {
                var firstCard = handArea.GetChild(0).GetComponent<BattleCardUI>();
                if (firstCard != null && cardDetailView != null)
                {
                    cardDetailView.Open(firstCard.GetData());
                    Debug.Log($"👁️ Bot Reveal Player Hand: {firstCard.GetData().cardName}");
                    AddBattleLog($"Bot revealed: {firstCard.GetData().cardName}");
                }
            }
        }
    }

    /// <summary>ดูการ์ดหลายใบบนมือของฝ่ายตรงข้าม</summary>
    void ApplyRevealHandMultiple(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (effect.targetType == TargetType.EnemyHand)
        {
            bool targetIsPlayer = !isPlayer;
            int sourceCardCost = sourceCard != null && sourceCard.GetData() != null ? sourceCard.GetData().cost : -1;
            if (IsHandRevealBlockedByContinuousEffect(targetIsPlayer, sourceCardCost, ActionType.RevealHandMultiple))
            {
                AddBattleLog($"{(isPlayer ? "Player" : "Bot")} tried to reveal multiple hand cards but it was blocked");
                return;
            }
        }

        // กำหนดจำนวนการ์ดที่จะดู (ถ้าไม่ระบุ value ให้ดูทั้งหมด)
        int revealCount = effect.value > 0 ? effect.value : 99;

        // เลือกมือของฝ่ายตรงข้าม
        Transform targetHand = null;
        string targetName = "";

        if (effect.targetType == TargetType.EnemyHand)
        {
            if (isPlayer)
            {
                // ผู้เล่นใช้ -> ดูมือบอท
                targetHand = enemyHandArea;
                targetName = "มือบอท";
            }
            else
            {
                // บอทใช้ -> ดูมือผู้เล่น
                targetHand = handArea;
                targetName = "มือผู้เล่น";
            }
        }

        if (targetHand == null || targetHand.childCount == 0)
        {
            Debug.Log($"👁️ RevealHandMultiple: {targetName} ว่างเปล่า");
            AddBattleLog($"Revealed {targetName} - Empty");
            return;
        }

        // รวบรวมการ์ดที่จะแสดง
        List<CardData> cardsToReveal = new List<CardData>();
        int actualCount = Mathf.Min(revealCount, targetHand.childCount);

        for (int i = 0; i < actualCount; i++)
        {
            var cardUI = targetHand.GetChild(i).GetComponent<BattleCardUI>();
            if (cardUI != null && cardUI.GetData() != null)
            {
                cardsToReveal.Add(cardUI.GetData());
            }
        }

        // เปิด Panel แสดงการ์ด
        if (cardsToReveal.Count > 0)
        {
            ShowHandRevealPanel(cardsToReveal, targetName);
            Debug.Log($"👁️ RevealHandMultiple: แสดง {cardsToReveal.Count} ใบจาก{targetName}");
            AddBattleLog($"Revealed {cardsToReveal.Count} cards from {targetName}");
        }
    }

    void ApplyDiscardDeck(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        int discardCount = effect.value > 0 ? effect.value : 1;

        if (effect.targetType == TargetType.EnemyDeck)
        {
            // target = เด็คฝั่งตรงข้ามของผู้ใช้เอฟเฟกต์
            List<CardData> targetDeck = isPlayer ? enemyDeckList : deckList;
            bool targetOwnerIsPlayer = !isPlayer;

            int milledCount = 0;
            for (int i = 0; i < discardCount && targetDeck.Count > 0; i++)
            {
                CardData discardedCard = targetDeck[0];
                targetDeck.RemoveAt(0);
                // ส่งลงสุสาน
                SendToGraveyard(discardedCard, isPlayer: targetOwnerIsPlayer);
                milledCount++;
                Debug.Log($"🗑️ Discard Opponent Deck Card: {discardedCard.cardName}");
            }

            AddBattleLog($"{(isPlayer ? "Player" : "Bot")} milled {milledCount} card(s) from {(targetOwnerIsPlayer ? "Player" : "Bot")} deck");
            if (milledCount > 0)
            {
                PlayDeckDiscardFeedback(targetOwnerIsPlayer, milledCount);
            }
            UpdateDeckVisualization();
        }
        else if (effect.targetType == TargetType.EnemyHand)
        {
            // Discard จากมือฝั่งตรงข้าม
            Transform targetHand = isPlayer ? enemyHandArea : handArea;
            int discardedCount = 0;

            if (targetHand != null && targetHand.childCount > 0)
            {
                for (int i = 0; i < discardCount && targetHand.childCount > 0; i++)
                {
                    var card = targetHand.GetChild(0).GetComponent<BattleCardUI>();
                    if (card != null && card.GetData() != null)
                    {
                        DestroyCardToGraveyard(card);
                        discardedCount++;
                        Debug.Log($"🗑️ Discard Opponent Hand Card: {card.GetData().cardName}");
                    }
                }
            }

            AddBattleLog($"{(isPlayer ? "Player" : "Bot")} discarded {discardedCount} card(s) from opponent hand");
        }
    }

    void PlayDeckDiscardFeedback(bool targetOwnerIsPlayer, int discardCount)
    {
        if (discardCount <= 0) return;

        Transform deckAnchor = targetOwnerIsPlayer ? deckPileTransform : enemyDeckPileTransform;
        Transform graveAnchor = targetOwnerIsPlayer ? playerGraveyardArea : enemyGraveyardArea;

        if (deckAnchor != null)
        {
            ShowDamagePopupString($"Discard {discardCount}", deckAnchor);
            StartCoroutine(ShakeEffect(deckAnchor, 0.12f, 8f));
        }

        if (graveAnchor != null)
        {
            ShowDamagePopupString("To Grave", graveAnchor);
            StartCoroutine(ShakeEffect(graveAnchor, 0.12f, 6f));
        }
    }

    IEnumerator ApplyPeekDiscardTopDeckCoroutine(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (effect.targetType != TargetType.EnemyDeck)
        {
            Debug.LogWarning($"⚠️ PeekDiscardTopDeck รองรับเฉพาะ TargetType.EnemyDeck (current: {effect.targetType})");
            yield break;
        }

        List<CardData> targetDeck = isPlayer ? enemyDeckList : deckList;
        bool targetIsPlayerDeck = !isPlayer;

        if (targetDeck == null || targetDeck.Count == 0)
        {
            AddBattleLog($"{(targetIsPlayerDeck ? "Player" : "Bot")} deck is empty");
            yield break;
        }

        int peekCount;
        if (effect.value == 0)
        {
            // value = 0 => ดูทุกใบในเด็คเป้าหมาย
            peekCount = targetDeck.Count;
        }
        else
        {
            // value > 0 => ดูตามจำนวนที่กำหนด, value < 0 => ใช้ค่า default
            peekCount = effect.value > 0 ? effect.value : 3;
            peekCount = Mathf.Min(peekCount, targetDeck.Count);
        }
        int discardCount = effect.duration > 0 ? effect.duration : 1;

        List<CardData> peekedCards = targetDeck.Take(peekCount).ToList();
        if (peekedCards.Count == 0)
        {
            yield break;
        }

        List<CardData> selectableCards = peekedCards.Where(card => MatchesPeekDiscardFilter(card, effect)).ToList();
        if (selectableCards.Count == 0)
        {
            string filterText = GetPeekDiscardFilterText(effect);
            AddBattleLog($"Peeked {peekCount} cards but no selectable card matches: {filterText}");
            if (isPlayer)
            {
                yield return StartCoroutine(ShowPeekDiscardNoMatchFeedback(peekedCards, effect));
            }
            UpdateDeckVisualization();
            yield break;
        }

        discardCount = Mathf.Clamp(discardCount, 1, selectableCards.Count);

        List<CardData> chosenCards = new List<CardData>();
        List<CardData> remainingSelectable = new List<CardData>(selectableCards);

        if (isPlayer)
        {
            if (discardCount > 1)
            {
                yield return StartCoroutine(PlayerChoosePeekDiscardCardsMulti(peekedCards, remainingSelectable, effect, discardCount));
                if (selectedPeekDiscardCards != null && selectedPeekDiscardCards.Count > 0)
                {
                    chosenCards.AddRange(selectedPeekDiscardCards);
                }
            }
            else
            {
                for (int pickIndex = 0; pickIndex < discardCount && remainingSelectable.Count > 0; pickIndex++)
                {
                    yield return StartCoroutine(PlayerChoosePeekDiscardCard(peekedCards, remainingSelectable, effect, pickIndex + 1, discardCount));

                    if (selectedPeekDiscardCard == null)
                    {
                        break;
                    }

                    chosenCards.Add(selectedPeekDiscardCard);
                    remainingSelectable.Remove(selectedPeekDiscardCard);
                }
            }
        }
        else
        {
            for (int i = 0; i < discardCount && remainingSelectable.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, remainingSelectable.Count);
                CardData picked = remainingSelectable[randomIndex];
                chosenCards.Add(picked);
                remainingSelectable.RemoveAt(randomIndex);
            }
        }

        if (chosenCards.Count == 0 && selectableCards.Count > 0)
        {
            chosenCards.Add(selectableCards[0]);
        }

        int removedCount = 0;
        foreach (CardData chosenCard in chosenCards)
        {
            int removeIndex = targetDeck.IndexOf(chosenCard);
            if (removeIndex < 0)
            {
                int searchLimit = Mathf.Min(peekCount + removedCount, targetDeck.Count);
                for (int i = 0; i < searchLimit; i++)
                {
                    if (targetDeck[i] == chosenCard || targetDeck[i].card_id == chosenCard.card_id)
                    {
                        removeIndex = i;
                        break;
                    }
                }
            }

            if (removeIndex >= 0)
            {
                CardData removed = targetDeck[removeIndex];
                targetDeck.RemoveAt(removeIndex);
                SendToGraveyard(removed, isPlayer: targetIsPlayerDeck);
                removedCount++;
            }
        }

        AddBattleLog($"{(isPlayer ? "Player" : "Bot")} peeked {peekCount} and discarded {removedCount} card(s)");

        if (removedCount > 0)
        {
            PlayDeckDiscardFeedback(targetIsPlayerDeck, removedCount);
        }

        UpdateDeckVisualization();
    }

    IEnumerator PlayerChoosePeekDiscardCardsMulti(List<CardData> cards, List<CardData> selectableCards, CardEffect effect, int requiredCount)
    {
        selectedPeekDiscardCards.Clear();
        selectedPeekDiscardIndices.Clear();
        requiredPeekDiscardCount = Mathf.Max(1, requiredCount);
        peekDiscardConfirmed = false;
        isChoosingPeekDiscard = true;
        isPeekDiscardMultiSelectMode = true;

        if (cards == null || cards.Count == 0)
        {
            isChoosingPeekDiscard = false;
            isPeekDiscardMultiSelectMode = false;
            yield break;
        }

        if (handRevealPanel == null || handRevealListRoot == null)
        {
            Debug.LogWarning("⚠️ HandRevealPanel/ListRoot ไม่พร้อม ใช้การเลือกอัตโนมัติ");
            selectedPeekDiscardCards = selectableCards.Take(requiredPeekDiscardCount).ToList();
            peekDiscardConfirmed = true;
            isChoosingPeekDiscard = false;
            isPeekDiscardMultiSelectMode = false;
            yield break;
        }

        currentPeekCards = new List<CardData>(cards);
        currentPeekSelectableCards = new List<CardData>(selectableCards);
        currentPeekEffect = effect;
        hasCurrentPeekEffect = true;

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(OnPeekDiscardConfirmPressed);
            handRevealCloseButton.interactable = true;

            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg == null) closeCg = handRevealCloseButton.gameObject.AddComponent<CanvasGroup>();
            closeCg.blocksRaycasts = true;
        }

        ClearListRoot(handRevealListRoot);
        SetupHandRevealScroll();
        PopulatePeekDiscardSelectionList(currentPeekCards, currentPeekSelectableCards, selectedPeekDiscardIndices);
        UpdatePeekDiscardMultiTitle();
        handRevealPanel.SetActive(true);

        while (!peekDiscardConfirmed)
        {
            yield return null;
        }

        CloseHandRevealPanel();

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.interactable = true;
            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg != null) closeCg.blocksRaycasts = true;
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(CloseHandRevealPanel);
        }

        isChoosingPeekDiscard = false;
        isPeekDiscardMultiSelectMode = false;
    }

    void UpdatePeekDiscardMultiTitle()
    {
        if (handRevealTitleText == null) return;

        string filterText = hasCurrentPeekEffect ? GetPeekDiscardFilterText(currentPeekEffect) : "Any";
        handRevealTitleText.text = $"🔍 เลือกการ์ดทิ้ง {selectedPeekDiscardCards.Count}/{requiredPeekDiscardCount} [{filterText}] | คลิกซ้าย=เลือก/ยกเลิก, คลิกขวา=ดูรายละเอียด, ปุ่มปิด=ยืนยัน";
    }

    void OnPeekDiscardConfirmPressed()
    {
        if (!isChoosingPeekDiscard || !isPeekDiscardMultiSelectMode) return;

        if (selectedPeekDiscardCards.Count != requiredPeekDiscardCount)
        {
            ShowDamagePopupString($"เลือกให้ครบ {requiredPeekDiscardCount} ใบ", handRevealListRoot != null ? handRevealListRoot : transform);
            return;
        }

        peekDiscardConfirmed = true;
    }

    IEnumerator PlayerChoosePeekDiscardCard(List<CardData> cards, List<CardData> selectableCards, CardEffect effect, int pickNumber, int pickTotal)
    {
        selectedPeekDiscardCard = null;
        peekDiscardConfirmed = false;
        isChoosingPeekDiscard = true;

        if (cards == null || cards.Count == 0)
        {
            isChoosingPeekDiscard = false;
            yield break;
        }

        if (handRevealPanel == null || handRevealListRoot == null)
        {
            Debug.LogWarning("⚠️ HandRevealPanel/ListRoot ไม่พร้อม ใช้การเลือกใบแรกอัตโนมัติ");
            selectedPeekDiscardCard = selectableCards[0];
            peekDiscardConfirmed = true;
            isChoosingPeekDiscard = false;
            yield break;
        }

        if (handRevealTitleText != null)
        {
            handRevealTitleText.text = $"🔍 เลือกการ์ดที่จะทิ้ง ({pickNumber}/{pickTotal}) [{GetPeekDiscardFilterText(effect)}] | คลิกซ้าย=ทิ้ง, คลิกขวา=ดูรายละเอียด";
        }

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.interactable = false;

            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg == null) closeCg = handRevealCloseButton.gameObject.AddComponent<CanvasGroup>();
            closeCg.blocksRaycasts = false;
        }

        ClearListRoot(handRevealListRoot);
        SetupHandRevealScroll();
        PopulatePeekDiscardSelectionList(cards, selectableCards);
        handRevealPanel.SetActive(true);

        while (!peekDiscardConfirmed)
        {
            yield return null;
        }

        CloseHandRevealPanel();

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.interactable = true;
            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg != null) closeCg.blocksRaycasts = true;
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(CloseHandRevealPanel);
        }

        isChoosingPeekDiscard = false;
    }

    IEnumerator ShowPeekDiscardNoMatchFeedback(List<CardData> cards, CardEffect effect)
    {
        if (cards == null || cards.Count == 0)
        {
            yield break;
        }

        if (handRevealPanel == null || handRevealListRoot == null)
        {
            yield break;
        }

        if (handRevealTitleText != null)
        {
            handRevealTitleText.text = $"🔍 ไม่มีการ์ดที่เข้าเงื่อนไขทิ้ง [{GetPeekDiscardFilterText(effect)}]";
        }

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.interactable = false;

            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg == null) closeCg = handRevealCloseButton.gameObject.AddComponent<CanvasGroup>();
            closeCg.blocksRaycasts = false;
        }

        ClearListRoot(handRevealListRoot);
        SetupHandRevealScroll();
        PopulatePeekDiscardSelectionList(cards, new List<CardData>());
        handRevealPanel.SetActive(true);

        yield return new WaitForSeconds(1.25f);

        CloseHandRevealPanel();

        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.interactable = true;
            var closeCg = handRevealCloseButton.GetComponent<CanvasGroup>();
            if (closeCg != null) closeCg.blocksRaycasts = true;
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(CloseHandRevealPanel);
        }
    }

    void PopulatePeekDiscardSelectionList(List<CardData> cards, List<CardData> selectableCards, List<int> selectedIndices = null)
    {
        if (handRevealListRoot == null || cards == null || cards.Count == 0)
        {
            return;
        }

        HashSet<string> selectableIds = new HashSet<string>(
            selectableCards
                .Where(c => c != null && !string.IsNullOrEmpty(c.card_id))
                .Select(c => c.card_id)
        );

        var gridLayout = handRevealListRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = handRevealListRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(10f, 10f);
        gridLayout.cellSize = new Vector2(200f, 280f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GetHandRevealColumnCount(gridLayout.cellSize.x + gridLayout.spacing.x, 6);

        var fitter = handRevealListRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = handRevealListRoot.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int cardIndex = 0; cardIndex < cards.Count; cardIndex++)
        {
            var card = cards[cardIndex];
            if (card == null) continue;

            var item = Instantiate(cardPrefab, handRevealListRoot);
            item.name = $"PeekDeck_{card.cardName}";

            var ui = item.GetComponent<BattleCardUI>();
            if (ui != null)
            {
                ui.Setup(card);
                // ปิด interaction ของ BattleCardUI บนการ์ด preview ใน panel นี้
                // เพื่อไม่ให้คลิกขวาไปเข้าลอจิกเล่นการ์ด/โจมตี/ลาก
                ui.enabled = false;
            }

            var img = item.GetComponent<Image>();
            if (img != null)
            {
                if (card.artwork != null)
                {
                    img.sprite = card.artwork;
                    img.color = Color.white;
                }
                img.raycastTarget = true;
            }

            bool canSelect = !string.IsNullOrEmpty(card.card_id) && selectableIds.Contains(card.card_id);
            if (!canSelect && img != null)
            {
                img.color = new Color(0.65f, 0.65f, 0.65f, 0.95f);
            }

            bool isSelected = selectedIndices != null && selectedIndices.Contains(cardIndex);
            if (isSelected && img != null)
            {
                img.color = new Color(0.7f, 1f, 0.7f, 1f);
            }

            var cg = item.GetComponent<CanvasGroup>();
            if (cg == null) cg = item.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.alpha = 1f;

            var btn = item.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }

            CardData selectedCard = card;
            var trigger = item.GetComponent<EventTrigger>();
            if (trigger == null) trigger = item.AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            int displayIndex = cardIndex;

            var leftEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            leftEntry.callback.AddListener((data) =>
            {
                var pointerData = data as PointerEventData;
                if (pointerData == null) return;

                if (pointerData.button == PointerEventData.InputButton.Left)
                {
                    if (canSelect)
                    {
                        OnPeekDiscardCardSelected(selectedCard, displayIndex);
                    }
                    else
                    {
                        ShowDamagePopupString("เลือกใบนี้ไม่ได้", item.transform);
                    }
                }
                else if (pointerData.button == PointerEventData.InputButton.Right)
                {
                    if (cardDetailView != null)
                    {
                        cardDetailView.Open(selectedCard);
                    }
                }
            });
            trigger.triggers.Add(leftEntry);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(handRevealListRoot as RectTransform);
    }

    void OnPeekDiscardCardSelected(CardData card, int displayIndex = -1)
    {
        if (!isChoosingPeekDiscard || card == null) return;

        if (isPeekDiscardMultiSelectMode)
        {
            bool isSelectable = currentPeekSelectableCards.Any(c => c != null && c.card_id == card.card_id);
            if (!isSelectable)
            {
                return;
            }

            if (displayIndex < 0 || displayIndex >= currentPeekCards.Count)
            {
                return;
            }

            int existingIndex = selectedPeekDiscardIndices.IndexOf(displayIndex);
            if (existingIndex >= 0)
            {
                selectedPeekDiscardIndices.RemoveAt(existingIndex);
            }
            else
            {
                if (selectedPeekDiscardIndices.Count >= requiredPeekDiscardCount)
                {
                    ShowDamagePopupString($"เลือกได้สูงสุด {requiredPeekDiscardCount} ใบ", handRevealListRoot != null ? handRevealListRoot : transform);
                    return;
                }

                selectedPeekDiscardIndices.Add(displayIndex);
            }

            selectedPeekDiscardCards = selectedPeekDiscardIndices
                .Where(i => i >= 0 && i < currentPeekCards.Count)
                .Select(i => currentPeekCards[i])
                .ToList();

            UpdatePeekDiscardMultiTitle();
            ClearListRoot(handRevealListRoot);
            PopulatePeekDiscardSelectionList(currentPeekCards, currentPeekSelectableCards, selectedPeekDiscardIndices);
            return;
        }

        selectedPeekDiscardCard = card;
        peekDiscardConfirmed = true;
        Debug.Log($"✅ PeekDiscard selected: {card.cardName}");
    }

    bool MatchesPeekDiscardFilter(CardData card, CardEffect effect)
    {
        if (card == null)
        {
            return false;
        }

        if (effect.targetCardTypeFilter != EffectCardTypeFilter.Any)
        {
            CardType expectedType = ConvertFilterToCardType(effect.targetCardTypeFilter);
            if (card.type != expectedType)
            {
                return false;
            }
        }

        if (effect.targetMainCat != MainCategory.General && card.mainCategory != effect.targetMainCat)
        {
            return false;
        }

        if (effect.targetSubCat != SubCategory.General && card.subCategory != effect.targetSubCat)
        {
            return false;
        }

        if (effect.useExcludeFilter)
        {
            bool excludedByMain = effect.excludeMainCat != MainCategory.General && card.mainCategory == effect.excludeMainCat;
            bool excludedBySub = effect.excludeSubCat != SubCategory.General && card.subCategory == effect.excludeSubCat;
            if (excludedByMain || excludedBySub)
            {
                return false;
            }
        }

        return true;
    }

    CardType ConvertFilterToCardType(EffectCardTypeFilter filter)
    {
        switch (filter)
        {
            case EffectCardTypeFilter.Monster:
                return CardType.Monster;
            case EffectCardTypeFilter.Spell:
                return CardType.Spell;
            case EffectCardTypeFilter.EquipSpell:
                return CardType.EquipSpell;
            case EffectCardTypeFilter.Token:
                return CardType.Token;
            default:
                return CardType.Monster;
        }
    }

    string GetPeekDiscardFilterText(CardEffect effect)
    {
        List<string> parts = new List<string>();

        if (effect.targetCardTypeFilter != EffectCardTypeFilter.Any)
        {
            parts.Add(effect.targetCardTypeFilter.ToString());
        }

        if (effect.targetMainCat != MainCategory.General)
        {
            parts.Add(effect.targetMainCat.ToString());
        }

        if (effect.targetSubCat != SubCategory.General)
        {
            parts.Add(effect.targetSubCat.ToString());
        }

        if (parts.Count == 0)
        {
            return "Any";
        }

        return string.Join(" / ", parts);
    }

    IEnumerator ApplyForceChooseDiscardCoroutine(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        int count = effect.value > 0 ? effect.value : 1;
        Transform targetHandArea = null;
        string targetName = "";

        // กำหนด target (ตรงข้ามกับผู้ใช้สกิล)
        if (effect.targetType == TargetType.EnemyHand && !isPlayer)
        {
            // บอทใช้สกิล → บังคับผู้เล่นทิ้งการ์ด
            targetHandArea = handArea;
            targetName = "Player";
            Debug.Log($"🤖 BOT used skill - Player must discard {count} card(s)");
        }
        else if (effect.targetType == TargetType.EnemyHand && isPlayer)
        {
            // ผู้เล่นใช้สกิล → บังคับบอททิ้งการ์ด (AI เลือกทิ้งอัตโนมัติ)
            targetHandArea = enemyHandArea;
            targetName = "Enemy";
            Debug.Log($"👤 PLAYER used skill - Enemy discards {count} card(s) randomly");
        }

        if (targetHandArea == null || targetHandArea.childCount == 0)
        {
            Debug.Log($"⚠️ {targetName} ไม่มีการ์ดบนมือให้ทิ้ง");
            AddBattleLog($"{targetName} has no cards to discard");
            yield break;
        }

        // จำกัดจำนวนที่ต้องทิ้งไม่เกินการ์ดที่มี
        count = Mathf.Min(count, targetHandArea.childCount);

        Debug.Log($"🗑️ Force {targetName} to choose {count} card(s) to discard");
        AddBattleLog($"Force {targetName} to discard {count} card(s)");

        if (targetName == "Player")
        {
            // ให้ผู้เล่นเลือกเอง
            Debug.Log($"⏳ WAITING FOR PLAYER INPUT...");
            yield return StartCoroutine(PlayerChooseDiscard(count));
            Debug.Log($"✅ Player has made choice, continuing...");
        }
        else
        {
            // บอทเลือกอัตโนมัติ (สุ่มทิ้ง)
            Debug.Log($"🤖 Enemy auto discarding...");
            yield return StartCoroutine(EnemyAutoDiscard(count));
            Debug.Log($"✅ Enemy has discarded, continuing...");
        }
    }

    IEnumerator PlayerChooseDiscard(int count)
    {
        Debug.Log($"[PlayerChooseDiscard] ✅ STARTS");
        
        if (forceDiscardPanel == null)
        {
            Debug.LogError("❌ CRITICAL: forceDiscardPanel is NULL! Cannot show UI!");
            yield break;
        }

        requiredDiscardCount = count;
        selectedCardsToDiscard.Clear();
        isChoosingDiscard = true;
        discardConfirmed = false;

        Debug.Log($"[PlayerChooseDiscard] Panel: {(forceDiscardPanel != null ? "YES" : "NO")}, Button: {(forceDiscardConfirmButton != null ? "YES" : "NO")}");

        // เปิด Panel
        forceDiscardPanel.SetActive(true);
        yield return null; // รอให้ Panel activate
        
        Debug.Log($"[PlayerChooseDiscard] Panel activated: {forceDiscardPanel.activeSelf}");

        // ตั้งค่า UI
        if (forceDiscardTitleText) 
        {
            forceDiscardTitleText.text = $"Choose {count} card(s) to discard";
            Debug.Log($"[PlayerChooseDiscard] Title set");
        }
        UpdateForceDiscardCountUI();

        // สร้างการ์ดจากมือผู้เล่น
        PopulateForceDiscardPanel();
        yield return null; // รอให้ cards spawn
        
        Debug.Log($"[PlayerChooseDiscard] Cards populated: {forceDiscardListRoot.childCount} cards");

        // เชื่อมต่อปุ่มยืนยัน
        if (forceDiscardConfirmButton != null)
        {
            forceDiscardConfirmButton.onClick.RemoveAllListeners();
            forceDiscardConfirmButton.onClick.AddListener(() => OnForceDiscardConfirm());
            Debug.Log($"[PlayerChooseDiscard] ✅ Confirm button listener added and ready");
        }
        else
        {
            Debug.LogError("❌ forceDiscardConfirmButton is NULL!");
        }

        // รอจนกว่าผู้เล่นจะยืนยัน - WITH TIMEOUT
        float startTime = Time.time;
        float timeout = 300f; // 5 minutes timeout
        int loopCount = 0;
        
        Debug.Log($"[PlayerChooseDiscard] ⏳ WAITING FOR PLAYER - Will wait up to {timeout}s (or Click Confirm button)");

        while (!discardConfirmed)
        {
            loopCount++;
            float elapsed = Time.time - startTime;
            
            if (loopCount % 300 == 0) // Log every ~5 seconds
            {
                Debug.Log($"[PlayerChooseDiscard] STILL WAITING... {elapsed:F1}s (discardConfirmed={discardConfirmed}, selected={selectedCardsToDiscard.Count}/{requiredDiscardCount})");
            }
            
            if (elapsed > timeout)
            {
                Debug.LogError($"❌ TIMEOUT after {elapsed:F1}s: Player didn't confirm discard!");
                break;
            }
            
            yield return null;
        }

        float finalTime = Time.time - startTime;
        Debug.Log($"✅ [PlayerChooseDiscard] CONFIRMED after {finalTime:F2}s - Destroying {selectedCardsToDiscard.Count} cards");

        // ทำลายการ์ดที่เลือก
        foreach (var card in selectedCardsToDiscard)
        {
            if (card != null && card.gameObject != null)
            {
                Debug.Log($"🗑️ Player discarded: {card.GetData().cardName}");
                DestroyCardToGraveyard(card);
            }
        }

        // ปิด Panel
        forceDiscardPanel.SetActive(false);
        isChoosingDiscard = false;
        selectedCardsToDiscard.Clear();
        discardConfirmed = false;
        
        Debug.Log($"✅ [PlayerChooseDiscard] COMPLETE - Ready to continue game");
    }

    IEnumerator EnemyAutoDiscard(int count)
    {
        if (enemyHandArea == null || enemyHandArea.childCount == 0) yield break;

        for (int i = 0; i < count && enemyHandArea.childCount > 0; i++)
        {
            int randomIndex = Random.Range(0, enemyHandArea.childCount);
            var card = enemyHandArea.GetChild(randomIndex).GetComponent<BattleCardUI>();
            
            if (card != null && card.GetData() != null)
            {
                Debug.Log($"🗑️ Enemy discarded: {card.GetData().cardName}");
                DestroyCardToGraveyard(card);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    void PopulateForceDiscardPanel()
    {
        if (forceDiscardListRoot == null) 
        {
            Debug.LogError("❌ forceDiscardListRoot is NULL!");
            return;
        }

        // ลบการ์ดเก่าออก
        foreach (Transform child in forceDiscardListRoot)
        {
            Destroy(child.gameObject);
        }

        int cardCount = handArea.childCount;
        Debug.Log($"[PopulateForceDiscardPanel] Creating {cardCount} card copies...");

        // สร้างการ์ดใหม่จากมือผู้เล่น
        foreach (Transform cardTransform in handArea)
        {
            BattleCardUI originalCard = cardTransform.GetComponent<BattleCardUI>();
            if (originalCard == null) continue;

            GameObject cardCopy = Instantiate(cardPrefab, forceDiscardListRoot);
            BattleCardUI cardUI = cardCopy.GetComponent<BattleCardUI>();
            
            if (cardUI != null)
            {
                cardUI.Setup(originalCard.GetData());
                cardUI.SetReferenceCard(originalCard); // เก็บ reference ของการ์ดจริง

                // เพิ่ม button เพื่อเลือก/ยกเลิก
                Button btn = cardCopy.GetComponent<Button>();
                if (btn == null) btn = cardCopy.AddComponent<Button>();
                
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnForceDiscardCardClick(cardUI));
                
                Debug.Log($"  - Card Created: {originalCard.GetData().cardName} (clickable: {btn.interactable})");
            }
        }
        
        Debug.Log($"[PopulateForceDiscardPanel] Done - {forceDiscardListRoot.childCount} cards ready");
    }

    public void OnForceDiscardCardClick(BattleCardUI cardUI)
    {
        if (!isChoosingDiscard) 
        {
            Debug.LogWarning("❌ OnForceDiscardCardClick called but not choosing!");
            return;
        }

        BattleCardUI originalCard = cardUI.GetReferenceCard();
        if (originalCard == null) 
        {
            Debug.LogError("❌ No reference card!");
            return;
        }

        Debug.Log($"[OnForceDiscardCardClick] Clicked: {originalCard.GetData().cardName}");

        // Toggle selection
        if (selectedCardsToDiscard.Contains(originalCard))
        {
            selectedCardsToDiscard.Remove(originalCard);
            cardUI.SetHighlight(false);
            Debug.Log($"  → DESELECTED: {originalCard.GetData().cardName}");
        }
        else
        {
            if (selectedCardsToDiscard.Count < requiredDiscardCount)
            {
                selectedCardsToDiscard.Add(originalCard);
                cardUI.SetHighlight(true);
                Debug.Log($"  → SELECTED: {originalCard.GetData().cardName} ({selectedCardsToDiscard.Count}/{requiredDiscardCount})");
            }
            else
            {
                Debug.Log($"⚠️ Max cards selected already!");
                ShowDamagePopupString($"Max {requiredDiscardCount} selected", cardUI.transform);
                return;
            }
        }

        UpdateForceDiscardCountUI();
    }

    void UpdateForceDiscardCountUI()
    {
        if (forceDiscardCountText)
        {
            forceDiscardCountText.text = $"{selectedCardsToDiscard.Count}/{requiredDiscardCount}";
        }

        // เปิด/ปิดปุ่มยืนยันตามจำนวนที่เลือก
        if (forceDiscardConfirmButton)
        {
            bool canConfirm = (selectedCardsToDiscard.Count == requiredDiscardCount);
            forceDiscardConfirmButton.interactable = canConfirm;
            
            // ปิด raycast ด้วยเมื่อยังไม่ครบ
            CanvasGroup canvasGroup = forceDiscardConfirmButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = forceDiscardConfirmButton.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = canConfirm;
            
            Debug.Log($"[UpdateForceDiscardCountUI] {selectedCardsToDiscard.Count}/{requiredDiscardCount} - Button: {(canConfirm ? "🟢 ACTIVE" : "🔴 DISABLED")}");
        }
    }

    public void OnForceDiscardConfirm()
    {
        Debug.Log($"[OnForceDiscardConfirm] Called! isChoosingDiscard: {isChoosingDiscard}");
        
        if (!isChoosingDiscard) 
        {
            Debug.LogWarning("⚠️ Not in discard phase!");
            return;
        }
        
        if (selectedCardsToDiscard.Count != requiredDiscardCount)
        {
            Debug.LogError($"❌ SELECTION ERROR: Need {requiredDiscardCount} but selected {selectedCardsToDiscard.Count}!");
            ShowDamagePopupString($"Select {requiredDiscardCount} cards!", transform);
            return;
        }

        Debug.Log($"✅ [OnForceDiscardConfirm] CONFIRMED! Setting discardConfirmed = true");
        discardConfirmed = true;
    }

    void ApplyDisableAttack(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        Debug.LogWarning($"⚠️ DisableAttack: ต้องสร้าง Debuff system ก่อน");
    }

    void ApplyDisableAbility(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        Debug.LogWarning($"⚠️ DisableAbility: ต้องสร้าง Debuff system ก่อน");
    }

    void ApplyModifyStat(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);

        foreach (var target in targets)
        {
            if (target != null && target.GetData() != null)
            {
                // 🔥 หากใช้ value ได้แสดงว่า value คือพลังที่ต้องลด
                // ถ้า value = 0 หรือติดค่าตามจำนวนสุสาน ให้คำนวณจากสุสาน
                int graveyardBoost = GetGraveyardCount(!isPlayer); // นับสุสานของฝ่ายตรงข้าม

                target.GetData().atk = Mathf.Max(0, target.GetData().atk - graveyardBoost);
                target.GetData().cost = 0;
                Debug.Log($"⚠️ ModifyStat: {target.GetData().cardName} ATK->{target.GetData().atk} (Graveyard boost: {graveyardBoost}) Cost->0");
            }
        }
    }

    IEnumerator ApplyZeroStats(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);

        // 🔥 ถ้ามีหลายเป้าหมาย ให้ผู้เล่นเลือก 1 ตัว
        if (targets.Count > 1)
        {
            Debug.Log($"🎯 ZeroStats: มี {targets.Count} เป้าหมาย ให้เลือก 1 ตัว");

            // Highlight เป้าหมายที่เลือกได้
            foreach (var t in targets)
            {
                t.SetHighlight(true);
            }

            // รอให้ผู้เล่นเลือก
            yield return StartCoroutine(WaitForTargetSelection(targets, selectCount: 1));

            // ลบ Highlight
            foreach (var t in targets)
            {
                t.SetHighlight(false);
            }

            // ใช้เป้าหมายที่เลือก
            if (selectedTargets != null && selectedTargets.Count > 0)
            {
                targets = selectedTargets;
            }
            else
            {
                Debug.Log("⚠️ ไม่ได้เลือกเป้าหมาย ยกเลิก ZeroStats");
                yield break;
            }
        }

        // ตั้งค่า Cost = 0 และ ATK = 0
        foreach (var target in targets)
        {
            if (target != null && target.GetData() != null)
            {
                target.GetData().cost = 0;
                target.GetData().atk = 0;
                Debug.Log($"💀 ZeroStats: {target.GetData().cardName} → Cost=0, ATK=0");
                AddBattleLog($"  {target.GetData().cardName} nullified! (Cost=0, ATK=0)");
            }
        }
    }

    void ApplyDrawCard(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        int drawCount = effect.value > 0 ? effect.value : 1;

        // ค่า default: ฝั่งผู้ใช้การ์ด
        bool drawForPlayer = isPlayer;

        if (effect.targetType == TargetType.EnemyPlayer
            || effect.targetType == TargetType.EnemyHand
            || effect.targetType == TargetType.EnemyDeck)
        {
            drawForPlayer = !isPlayer;
        }
        else if (effect.targetType == TargetType.Self)
        {
            drawForPlayer = isPlayer;
        }

        if (drawForPlayer)
        {
            DrawCard(drawCount);
            AddBattleLog($"Player draws {drawCount} card(s) (effect)");
        }
        else
        {
            StartCoroutine(DrawEnemyCard(drawCount));
            AddBattleLog($"Bot draws {drawCount} card(s) (effect)");
        }

        UpdateUI();
    }

    /// <summary>ให้การ์ดข้ามการกัน (Bypass Intercept)
    /// - โหมดเดิม: ติดให้ sourceCard ใบเดียว โดย value = threshold (-1 = ข้ามทั้งหมด)
    /// - โหมดเลือก: ถ้าเป็น Spell/Equip และ targetType = Self จะเลือก Monster ฝั่งตัวเอง x ใบ (x = value) แล้วข้ามการกันทั้งหมดในเทิร์นนี้
    /// </summary>
    IEnumerator ApplyBypassIntercept(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        if (sourceCard == null || sourceCard.GetData() == null)
        {
            Debug.LogError("❌ ApplyBypassIntercept: sourceCard เป็น null!");
            yield break;
        }

        CardData sourceData = sourceCard.GetData();

        bool isSelectableSelfMonsterSkill =
            sourceData.type != CardType.Monster &&
            effect.targetType == TargetType.Self &&
            effect.targetCardTypeFilter == EffectCardTypeFilter.Monster;

        if (isSelectableSelfMonsterSkill)
        {
            List<BattleCardUI> targets = GetTargetCards(effect, isPlayer)
                .Where(t => t != null && t.GetData() != null && t.GetData().type == CardType.Monster)
                .ToList();

            if (targets.Count == 0)
            {
                Debug.Log("⚠️ ApplyBypassIntercept: ไม่มี Monster ฝั่งตัวเองให้เลือก");
                yield break;
            }

            int selectCount = effect.value > 0 ? effect.value : 1;
            selectCount = Mathf.Clamp(selectCount, 1, targets.Count);

            List<BattleCardUI> selected = new List<BattleCardUI>();

            if (isPlayer && targets.Count > selectCount)
            {
                yield return StartCoroutine(WaitForTargetSelection(targets, selectCount));
                if (selectedTargets != null && selectedTargets.Count > 0)
                {
                    selected.AddRange(selectedTargets.Where(t => t != null && t.GetData() != null && t.GetData().type == CardType.Monster));
                }
                selectedTargets.Clear();
            }
            else if (!isPlayer && targets.Count > selectCount)
            {
                List<BattleCardUI> pool = new List<BattleCardUI>(targets);
                for (int i = 0; i < selectCount && pool.Count > 0; i++)
                {
                    int idx = Random.Range(0, pool.Count);
                    selected.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }
            }
            else
            {
                selected.AddRange(targets.Take(selectCount));
            }

            foreach (var target in selected)
            {
                target.canBypassIntercept = true;
                target.bypassCostThreshold = -1;
                target.bypassAllowedMainCat = MainCategory.General;
                target.bypassAllowedSubCat = SubCategory.General;

                ShowDamagePopupString("Bypass!", target.transform);
                AddBattleLog($"{target.GetData().cardName} cannot be intercepted this turn");
                Debug.Log($"🚀 ApplyBypassIntercept(Selected): {target.GetData().cardName} can bypass all intercept this turn");
            }

            yield break;
        }

        int costThreshold = effect.value;
        MainCategory allowedMainCat = effect.bypassAllowedMainCat;
        SubCategory allowedSubCat = effect.bypassAllowedSubCat;

        // ตั้ง bypass ให้กับ sourceCard เท่านั้น
        sourceCard.canBypassIntercept = true;
        sourceCard.bypassCostThreshold = costThreshold;
        sourceCard.bypassAllowedMainCat = allowedMainCat;
        sourceCard.bypassAllowedSubCat = allowedSubCat;

        string thresholdText = costThreshold == -1 ? "all" : (costThreshold == 0 ? "nothing" : $"cost < {costThreshold}");
        string categoryText = "";
        if (allowedMainCat != MainCategory.General)
            categoryText = $" (except {allowedMainCat})";
        else if (allowedSubCat != SubCategory.General)
            categoryText = $" (except {allowedSubCat})";

        Debug.Log($"🚀 {sourceCard.GetData().cardName} gained Bypass Intercept ({thresholdText}{categoryText})!");
        AddBattleLog($"{sourceCard.GetData().cardName} gained Bypass Intercept ({thresholdText}{categoryText})");
        yield break;
    }

    /// <summary>บังคับให้การ์ดต้องกันการโจมตี (Force Intercept)</summary>
    IEnumerator ApplyForceIntercept(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);
        if (targets == null || targets.Count == 0) yield break;

        int selectCount = effect.value;

        // ถ้าไม่กำหนดจำนวน ให้บังคับทั้งหมดทันที (ไม่ต้องเลือก)
        if (selectCount <= 0)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.mustIntercept = true;
                    Debug.Log($"🛡️ {target.GetData().cardName} must intercept next attack!");
                    AddBattleLog($"{target.GetData().cardName} must intercept (forced)");
                }
            }
            yield break;
        }

        selectCount = Mathf.Clamp(selectCount, 1, targets.Count);

        // ผู้เล่น: ให้เลือกเป้าหมาย (เช่น เลือก Equip ที่จะถูกบังคับกัน)
        if (isPlayer && targets.Count > 0)
        {
            yield return StartCoroutine(WaitForTargetSelection(targets, selectCount));

            foreach (var target in selectedTargets)
            {
                if (target != null)
                {
                    target.mustIntercept = true;
                    Debug.Log($"🛡️ {target.GetData().cardName} must intercept next attack!");
                    AddBattleLog($"{target.GetData().cardName} must intercept (forced)");
                }
            }

            selectedTargets.Clear();
            yield break;
        }

        // บอทหรือกรณีไม่ต้องเลือก: เลือกอัตโนมัติ
        int applied = 0;
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.mustIntercept = true;
                Debug.Log($"🛡️ {target.GetData().cardName} must intercept next attack!");
                AddBattleLog($"{target.GetData().cardName} must intercept (forced)");
                applied++;
                if (applied >= selectCount) break;
            }
        }
    }

    /// <summary>ปิดการกัน (Disable Intercept) ของการ์ด Equip ฝ่ายตรงข้ามในเทิร์นนี้</summary>
    IEnumerator ApplyDisableIntercept(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);
        if (targets == null || targets.Count == 0) yield break;

        int selectCount = effect.value;

        // ถ้าไม่กำหนดจำนวน ให้ปิดการกันทั้งหมดทันที (ไม่ต้องเลือก)
        if (selectCount <= 0)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    target.cannotIntercept = true;
                    Debug.Log($"🚫 {target.GetData().cardName} cannot intercept this turn!");
                    AddBattleLog($"{target.GetData().cardName} cannot intercept (disabled)");
                }
            }
            yield break;
        }

        selectCount = Mathf.Clamp(selectCount, 1, targets.Count);

        // ผู้เล่น: ให้เลือกเป้าหมาย (เช่น เลือก Equip ที่จะถูกปิดการกัน)
        if (isPlayer && targets.Count > 0)
        {
            yield return StartCoroutine(WaitForTargetSelection(targets, selectCount));

            foreach (var target in selectedTargets)
            {
                if (target != null)
                {
                    target.cannotIntercept = true;
                    Debug.Log($"🚫 {target.GetData().cardName} cannot intercept this turn!");
                    AddBattleLog($"{target.GetData().cardName} cannot intercept (disabled)");
                }
            }

            selectedTargets.Clear();
            yield break;
        }

        // บอทหรือกรณีไม่ต้องเลือก: เลือกอัตโนมัติ
        int applied = 0;
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.cannotIntercept = true;
                Debug.Log($"🚫 {target.GetData().cardName} cannot intercept this turn!");
                AddBattleLog($"{target.GetData().cardName} cannot intercept (disabled)");
                applied++;
                if (applied >= selectCount) break;
            }
        }
    }

    /// <summary>ลบ Category ของการ์ดเป้าหมาย (ทำให้การ์ดเป็น General)
    /// value = 0: ทำทุกใบของฝ่ายตรงข้าม | value >= 1: เลือกตามจำนวนนั้น
    /// duration = 0: ตลอด | duration >= 1: จำนวนเทิร์น</summary>
    IEnumerator ApplyRemoveCategory(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);

        if (targets.Count == 0)
        {
            Debug.Log("⚠️ RemoveCategory: ไม่มีเป้าหมาย");
            yield break;
        }

        // 🔥 value = 0 → ทำทุกใบ, value >= 1 → เลือกตามจำนวน
        bool removeAll = (effect.value == 0);
        int selectCount = removeAll ? targets.Count : Mathf.Clamp(effect.value, 1, targets.Count);

        // 🕒 duration = 0 → ตลอด, duration >= 1 → จำนวนเทิร์น
        int duration = effect.duration;
        string durationText = (duration == 0) ? "permanent" : $"{duration} turn(s)";

        Debug.Log($"🎯 RemoveCategory: value={effect.value}, removeAll={removeAll}, targets={targets.Count}, selectCount={selectCount}, duration={duration} ({durationText})");

        // 🔥 ถ้าเป็นโหมดทำทุกใบ (value = 0) → ไม่ต้องเลือก ทำทันที
        if (removeAll)
        {
            Debug.Log($"⚡ RemoveCategory All: ลบประเภทการ์ด {targets.Count} ใบทันที ({durationText})");
            foreach (var target in targets)
            {
                if (target != null && target.GetData() != null)
                {
                    SubCategory originalCat = target.GetModifiedSubCategory();
                    target.RemoveSubCategory(duration);
                    ShowDamagePopupString("Lost Category!", target.transform);
                    Debug.Log($"🔴 {target.GetData().cardName} lost its category! ({originalCat} → General) for {durationText}");
                    AddBattleLog($"{target.GetData().cardName} lost its category ({originalCat} → General) for {durationText}");
                }
            }
            yield break;
        }

        // ผู้เล่น: ให้เลือกเป้าหมาย EquipSpell ที่จะสูญเสียประเภท
        if (isPlayer && targets.Count > 0)
        {
            yield return StartCoroutine(WaitForTargetSelection(targets, selectCount));

            foreach (var target in selectedTargets)
            {
                if (target != null && target.GetData() != null)
                {
                    SubCategory originalCat = target.GetModifiedSubCategory();
                    target.RemoveSubCategory(duration);
                    ShowDamagePopupString("Lost Category!", target.transform);
                    Debug.Log($"🔴 {target.GetData().cardName} lost its category! ({originalCat} → General) for {durationText}");
                    AddBattleLog($"{target.GetData().cardName} lost its category ({originalCat} → General) for {durationText}");
                }
            }

            selectedTargets.Clear();
            yield break;
        }

        // บอทหรือกรณีไม่ต้องเลือก: เลือกอัตโนมัติ
        int applied = 0;
        foreach (var target in targets)
        {
            if (target != null && target.GetData() != null)
            {
                SubCategory originalCat = target.GetModifiedSubCategory();
                target.RemoveSubCategory(duration);
                ShowDamagePopupString("Lost Category!", target.transform);
                Debug.Log($"🔴 {target.GetData().cardName} lost its category! ({originalCat} → General) for {durationText}");
                AddBattleLog($"{target.GetData().cardName} lost its category ({originalCat} → General) for {durationText}");
                applied++;
                if (applied >= selectCount) break;
            }
        }
    }

    /// <summary>เช็คว่าฝั่งป้องกันมีการ์ดที่ต้องกันบังคับหรือไม่</summary>
    /// <summary>หาการ์ดแรกที่มี mustIntercept = true (เช็คเฉพาะ EquipSlots เท่านั้น)</summary>
    /// <summary>ควบคุม (Control) Equip Spell ของฝ่ายตรงข้าม x ใบ เป็นเวลา x เทิร์น</summary>
    IEnumerator ApplyControlEquip(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = GetTargetCards(effect, isPlayer);

        if (targets.Count == 0)
        {
            Debug.Log("⚠️ ControlEquip: ไม่มีเป้าหมาย");
            yield break;
        }

        // 🔥 value = 0 → ควบคุมทั้งหมด, value >= 1 → เลือกตามจำนวน
        bool controlAll = (effect.value == 0);
        int selectCount = controlAll ? targets.Count : Mathf.Clamp(effect.value, 1, targets.Count);

        // 🕐 duration = 0 → ตลอด, duration >= 1 → จำนวนเทิร์น
        int duration = effect.duration;
        string durationText = (duration == 0) ? "permanent" : $"{duration} turn(s)";

        Debug.Log($"🎮 ControlEquip: value={effect.value}, controlAll={controlAll}, targets={targets.Count}, selectCount={selectCount}, duration={duration} ({durationText})");

        // 🔥 ถ้าเป็นผู้เล่นต้องเลือกเป้าหมาย (เลือก Equip ที่จะควบคุม)
        if (isPlayer && targets.Count > selectCount)
        {
            Debug.Log($"🎯 ControlEquip: ผู้เล่นต้องเลือก {selectCount} ใบจาก {targets.Count} เป้าหมาย");

            // Highlight เป้าหมายที่เลือกได้
            foreach (var t in targets)
            {
                t.SetHighlight(true);
            }

            // รอให้ผู้เล่นเลือก
            yield return StartCoroutine(WaitForTargetSelection(targets, selectCount));

            // ลบ Highlight
            foreach (var t in targets)
            {
                t.SetHighlight(false);
            }

            // ใช้เป้าหมายที่เลือก
            if (selectedTargets != null && selectedTargets.Count > 0)
            {
                targets = selectedTargets;
            }
            else
            {
                Debug.Log("⚠️ ไม่ได้เลือกเป้าหมาย ยกเลิก ControlEquip");
                yield break;
            }
        }

        // ควบคุมการ์ดแต่ละใบ และย้ายไปยังช่อง Equip ของฝ่ายตัวเอง
        int controlled = 0;
        foreach (var target in targets)
        {
            if (controlled >= selectCount) break;
            if (target == null || target.GetData() == null) continue;

            // ตรวจสอบว่ามีช่องว่างในช่อง Equip ของฝ่ายตัวเองหรือไม่
            Transform freeSlot = GetFreeSlot(CardType.EquipSpell, isPlayer: isPlayer);

            if (freeSlot != null)
            {
                // เก็บตำแหน่งเดิมและเจ้าของดั้งเดิม
                Transform originalSlot = target.transform.parent;
                bool originalOwner = IsCardOwnedByPlayer(target);
                
                // ควบคุมการ์ด
                target.isControlled = true;
                target.controlledTurnsRemaining = (duration == 0) ? -1 : duration;
                target.originalEquipSlot = originalSlot;
                target.originalOwnerIsPlayer = originalOwner;

                // ย้ายการ์ดไปยังช่อง Equip ของฝ่ายตัวเอง
                target.transform.SetParent(freeSlot, worldPositionStays: false);
                target.transform.localPosition = Vector3.zero;
                target.transform.localScale = Vector3.one;

                ShowDamagePopupString("Controlled!", target.transform);
                Debug.Log($"🎮 ControlEquip: {target.GetData().cardName} ถูกควบคุม! ({durationText}) - ย้ายไป {freeSlot.name}");
                AddBattleLog($"  {target.GetData().cardName} is controlled! ({durationText})");
                controlled++;
            }
            else
            {
                Debug.LogWarning($"⚠️ ControlEquip: ไม่มีช่อง Equip ว่างส าหรับการ์ด {target.GetData().cardName}");
            }
        }

        if (controlled > 0)
        {
            UpdateUI();
            Debug.Log($"✅ ControlEquip: ควบคุมการ์ด {controlled}/{selectCount} ใบ เป็นเวลา {durationText}");
        }
    }

    /// <summary>คืนการ์ด Equip ที่ถูกควบคุมกลับไปยังตำแหน่งเดิม</summary>
    public void ReturnControlledEquip(BattleCardUI card)
    {
        if (card == null || !card.isControlled) return;

        Transform originalSlot = card.originalEquipSlot;
        
        if (originalSlot == null)
        {
            Debug.LogWarning($"⚠️ ReturnControlledEquip: {(card.GetData() != null ? card.GetData().cardName : "Unknown")} - originalSlot เป็น null");
            return;
        }

        // ตรวจสอบว่าช่องเดิมยังว่างหรือไม่
        if (originalSlot.childCount > 0)
        {
            // ช่องเดิมถูกใช้แล้ว ให้หาช่องว่างใหม่ของฝ่ายเจ้าของเดิม
            Transform freeSlot = GetFreeSlot(CardType.EquipSpell, isPlayer: card.originalOwnerIsPlayer);
            if (freeSlot == null)
            {
                Debug.LogWarning($"⚠️ ReturnControlledEquip: ไม่มีช่อง Equip ว่างสำหรับการ์ด");
                return;
            }
            originalSlot = freeSlot;
        }

        // คืนข้อมูลการควบคุม
        card.isControlled = false;
        card.controlledTurnsRemaining = 0;
        card.originalEquipSlot = null;

        // ย้ายการ์ดกลับ
        card.transform.SetParent(originalSlot, worldPositionStays: false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;

        ShowDamagePopupString("Control Ended", card.transform);
        Debug.Log($"✅ ReturnControlledEquip: {(card.GetData() != null ? card.GetData().cardName : "Unknown")} - คืนการควบคุมแล้ว");
        AddBattleLog($"{(card.GetData() != null ? card.GetData().cardName : "Card")} is no longer controlled (returned)");

        UpdateUI();
    }

    void ProcessControlDurationsForAllEquips()
    {
        foreach (Transform slot in playerEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c) c.ProcessControlDuration();
            }
        }

        foreach (Transform slot in enemyEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var c = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (c) c.ProcessControlDuration();
            }
        }
    }

    BattleCardUI GetMustInterceptCard(bool defenderIsPlayer)
    {
        Transform[] equipSlots = defenderIsPlayer ? playerEquipSlots : enemyEquipSlots;

        // หาจากอุปกรณ์เท่านั้น (ไม่เช็คมอนสเตอร์)
        foreach (var slot in equipSlots)
        {
            if (slot != null && slot.childCount > 0)
            {
                var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (card != null && card.mustIntercept)
                {
                    return card;
                }
            }
        }

        return null;
    }

    /// <summary>เช็คว่าฝั่งป้องกันมีการ์ดที่ต้องกันบังคับหรือไม่ (เช็คเฉพาะ EquipSlots เท่านั้น)</summary>
    bool HasMustInterceptCard(bool defenderIsPlayer)
    {
        Transform[] equipSlots = defenderIsPlayer ? playerEquipSlots : enemyEquipSlots;

        // เช็คอุปกรณ์เท่านั้น (ไม่เช็คมอนสเตอร์)
        foreach (var slot in equipSlots)
        {
            if (slot != null && slot.childCount > 0)
            {
                var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (card != null && card.mustIntercept)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // --- Helper Functions ---

    List<BattleCardUI> GetTargetCards(CardEffect effect, bool isPlayer)
    {
        List<BattleCardUI> targets = new List<BattleCardUI>();

        Debug.Log($"🎯 GetTargetCards: TargetType={effect.targetType}, isPlayer={isPlayer}, MainCat={effect.targetMainCat}, SubCat={effect.targetSubCat}");

        switch (effect.targetType)
        {
            case TargetType.Self:
                // Target ฝั่งตัวเอง (ทั้ง Monster และ Equip)
                Transform[] selfMonsterSlots = isPlayer ? playerMonsterSlots : enemyMonsterSlots;
                Transform[] selfEquipSlots = isPlayer ? playerEquipSlots : enemyEquipSlots;

                if (selfMonsterSlots != null)
                {
                    foreach (var slot in selfMonsterSlots)
                    {
                        if (slot != null && slot.childCount > 0)
                        {
                            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                            if (card != null && card.GetData() != null && MatchesCategory(card.GetData(), effect))
                            {
                                targets.Add(card);
                                Debug.Log($"✅ เพิ่มการ์ดเป้าหมาย (Self/Monster): {card.GetData().cardName}");
                            }
                        }
                    }
                }

                if (selfEquipSlots != null)
                {
                    foreach (var slot in selfEquipSlots)
                    {
                        if (slot != null && slot.childCount > 0)
                        {
                            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                            if (card != null && card.GetData() != null && MatchesCategory(card.GetData(), effect))
                            {
                                targets.Add(card);
                                Debug.Log($"✅ เพิ่มการ์ดเป้าหมาย (Self/Equip): {card.GetData().cardName}");
                            }
                        }
                    }
                }
                break;

            case TargetType.EnemyMonster:
                // isPlayer = true → เป้าหมายคือมอนสเตอร์ของศัตรู (บอท)
                // isPlayer = false (บอท) → เป้าหมายคือมอนสเตอร์ของผู้เล่น
                Transform[] targetMonsterSlots = isPlayer ? enemyMonsterSlots : playerMonsterSlots;

                if (targetMonsterSlots != null)
                {
                    foreach (var slot in targetMonsterSlots)
                    {
                        if (slot != null && slot.childCount > 0)
                        {
                            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                            if (card != null && card.GetData() != null)
                            {
                                Debug.Log($"🔍 ตรวจสอบการ์ด: {card.GetData().cardName} (Main={card.GetData().mainCategory}, Sub={card.GetData().subCategory})");
                                if (MatchesCategory(card.GetData(), effect))
                                {
                                    targets.Add(card);
                                    Debug.Log($"✅ เพิ่มการ์ดเป้าหมาย: {card.GetData().cardName}");
                                }
                            }
                        }
                    }
                }
                break;

            case TargetType.EnemyEquip:
                Transform[] targetEquipSlots = isPlayer ? enemyEquipSlots : playerEquipSlots;

                if (targetEquipSlots != null)
                {
                    foreach (var slot in targetEquipSlots)
                    {
                        if (slot != null && slot.childCount > 0)
                        {
                            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                            if (card != null && card.GetData() != null)
                            {
                                Debug.Log($"🔍 ตรวจสอบการ์ด: {card.GetData().cardName} (Main={card.GetData().mainCategory}, Sub={card.GetData().subCategory})");
                                if (MatchesCategory(card.GetData(), effect))
                                {
                                    targets.Add(card);
                                    Debug.Log($"✅ เพิ่มการ์ดเป้าหมาย: {card.GetData().cardName}");
                                }
                            }
                        }
                    }
                }
                break;

            case TargetType.AllGlobal:
                // AllGlobal = ทำลายทั้งหมด Monster ที่ตรงเงื่อนไข (ไม่รวม Equip)
                Transform[] mons = isPlayer ? enemyMonsterSlots : playerMonsterSlots;

                if (mons != null)
                {
                    foreach (var slot in mons)
                    {
                        if (slot != null && slot.childCount > 0)
                        {
                            var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                            if (card != null && card.GetData() != null && MatchesCategory(card.GetData(), effect))
                            {
                                targets.Add(card);
                                Debug.Log($"✅ เพิ่มการ์ดเป้าหมาย (AllGlobal/Monster): {card.GetData().cardName}");
                            }
                        }
                    }
                }
                break;
        }

        Debug.Log($"🎯 พบการ์ดเป้าหมาย {targets.Count} ใบ");
        return targets;
    }

    /// <summary>เช็คว่าการ์ดใบนี้เป็นของผู้เล่นหรือบอทด้วยตำแหน่งที่วาง</summary>
    bool IsCardOwnedByPlayer(BattleCardUI card)
    {
        if (card == null) return true;

        Transform parent = card.transform.parent;

        if (handArea != null && parent == handArea) return true;
        if (enemyHandArea != null && parent == enemyHandArea) return false;

        if (playerMonsterSlots != null)
        {
            foreach (var slot in playerMonsterSlots)
                if (slot == parent) return true;
        }

        if (playerEquipSlots != null)
        {
            foreach (var slot in playerEquipSlots)
                if (slot == parent) return true;
        }

        if (enemyMonsterSlots != null)
        {
            foreach (var slot in enemyMonsterSlots)
                if (slot == parent) return false;
        }

        if (enemyEquipSlots != null)
        {
            foreach (var slot in enemyEquipSlots)
                if (slot == parent) return false;
        }

        // ค่า default ให้เป็นผู้เล่นเพื่อไม่พลาดการคำนวณเอฟเฟ็กต์ฝั่งเรา
        return true;
    }

    bool HasActiveRush(BattleCardUI card)
    {
        return HasActiveContinuousAction(card, ActionType.Rush);
    }

    public bool HasActiveContinuousAction(BattleCardUI sourceCard, ActionType action)
    {
        if (sourceCard == null || sourceCard.GetData() == null) return false;

        CardData sourceData = sourceCard.GetData();
        if (sourceData.effects == null || sourceData.effects.Count == 0) return false;

        bool sourceIsPlayer = IsCardOwnedByPlayer(sourceCard);

        foreach (CardEffect effect in sourceData.effects)
        {
            if (effect.trigger != EffectTrigger.Continuous) continue;
            if (effect.action != action) continue;

            if (IsEffectSuppressedByOpponentContinuousAura(sourceCard, effect, EffectTrigger.Continuous, sourceIsPlayer))
            {
                Debug.Log($"🚫 {action} suppressed: {sourceData.cardName}");
                continue;
            }

            return true;
        }

        return false;
    }

    bool IsEffectSuppressedByOpponentContinuousAura(BattleCardUI sourceCard, CardEffect pendingEffect, EffectTrigger triggerType, bool sourceIsPlayer)
    {
        if (sourceCard == null || sourceCard.GetData() == null) return false;

        Transform[] opponentMonsterLine = sourceIsPlayer ? enemyMonsterSlots : playerMonsterSlots;
        Transform[] opponentEquipLine = sourceIsPlayer ? enemyEquipSlots : playerEquipSlots;

        return HasContinuousDisableAbilitySuppression(opponentMonsterLine, sourceCard, pendingEffect, triggerType)
            || HasContinuousDisableAbilitySuppression(opponentEquipLine, sourceCard, pendingEffect, triggerType);
    }

    bool HasContinuousDisableAbilitySuppression(Transform[] sourceSlots, BattleCardUI sourceCard, CardEffect pendingEffect, EffectTrigger triggerType)
    {
        if (sourceSlots == null || sourceCard == null || sourceCard.GetData() == null) return false;

        foreach (Transform slot in sourceSlots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI auraCard = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (auraCard == null || auraCard.GetData() == null) continue;

            CardData auraData = auraCard.GetData();
            if (auraData.effects == null || auraData.effects.Count == 0) continue;

            foreach (CardEffect aura in auraData.effects)
            {
                if (aura.trigger != EffectTrigger.Continuous) continue;
                if (aura.action != ActionType.DisableAbility) continue;

                if (!DoesDisableAbilityAuraMatch(aura, sourceCard, pendingEffect, triggerType)) continue;

                Debug.Log($"🚫 [Cont.DisableAbility] {sourceCard.GetData().cardName} blocked by {auraData.cardName} | Trigger={triggerType} Action={pendingEffect.action}");
                return true;
            }
        }

        return false;
    }

    bool DoesDisableAbilityAuraMatch(CardEffect aura, BattleCardUI sourceCard, CardEffect pendingEffect, EffectTrigger triggerType)
    {
        CardData sourceData = sourceCard.GetData();
        if (sourceData == null) return false;

        if (aura.disableAbilityTriggerFilter != EffectTrigger.None && aura.disableAbilityTriggerFilter != triggerType)
        {
            return false;
        }

        if (aura.disableAbilityActionFilter != ActionType.None && aura.disableAbilityActionFilter != pendingEffect.action)
        {
            return false;
        }

        bool targetScopeMatch = false;
        switch (aura.targetType)
        {
            case TargetType.EnemyMonster:
                targetScopeMatch = sourceData.type == CardType.Monster || sourceData.type == CardType.Token;
                break;
            case TargetType.EnemyEquip:
                targetScopeMatch = sourceData.type == CardType.EquipSpell;
                break;
            case TargetType.EnemyPlayer:
            case TargetType.AllGlobal:
                targetScopeMatch = true;
                break;
            case TargetType.EnemyHand:
                targetScopeMatch = pendingEffect.targetType == TargetType.EnemyHand;
                break;
            case TargetType.EnemyDeck:
                targetScopeMatch = pendingEffect.targetType == TargetType.EnemyDeck;
                break;
            default:
                targetScopeMatch = false;
                break;
        }

        if (!targetScopeMatch) return false;

        if (aura.targetCardTypeFilter != EffectCardTypeFilter.Any)
        {
            bool typeMatch = false;
            switch (aura.targetCardTypeFilter)
            {
                case EffectCardTypeFilter.Monster:
                    typeMatch = sourceData.type == CardType.Monster;
                    break;
                case EffectCardTypeFilter.Spell:
                    typeMatch = sourceData.type == CardType.Spell;
                    break;
                case EffectCardTypeFilter.EquipSpell:
                    typeMatch = sourceData.type == CardType.EquipSpell;
                    break;
                case EffectCardTypeFilter.Token:
                    typeMatch = sourceData.type == CardType.Token;
                    break;
            }

            if (!typeMatch) return false;
        }

        if (aura.targetMainCat != MainCategory.General && sourceData.mainCategory != aura.targetMainCat)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(aura.targetCardNameFilter))
        {
            if (string.IsNullOrWhiteSpace(sourceData.cardName)) return false;

            bool nameMatch = string.Equals(
                sourceData.cardName.Trim(),
                aura.targetCardNameFilter.Trim(),
                System.StringComparison.OrdinalIgnoreCase
            );

            if (!nameMatch)
            {
                return false;
            }
        }

        SubCategory sourceSubCategory = sourceCard.GetModifiedSubCategory();
        if (aura.targetSubCat != SubCategory.General && sourceSubCategory != aura.targetSubCat)
        {
            return false;
        }

        if (aura.useExcludeFilter)
        {
            bool excludedByMain = aura.excludeMainCat != MainCategory.General && sourceData.mainCategory == aura.excludeMainCat;
            bool excludedBySub = aura.excludeSubCat != SubCategory.General && sourceSubCategory == aura.excludeSubCat;
            if (excludedByMain || excludedBySub)
            {
                return false;
            }
        }

        if (aura.value > 0 && sourceData.cost > aura.value)
        {
            return false;
        }

        return true;
    }

    bool IsHandRevealBlockedByContinuousEffect(bool protectedSideIsPlayer, int sourceCardCost = -1, ActionType incomingRevealAction = ActionType.RevealHand)
    {
        Transform[] ownMonsterSlots = protectedSideIsPlayer ? playerMonsterSlots : enemyMonsterSlots;
        Transform[] ownEquipSlots = protectedSideIsPlayer ? playerEquipSlots : enemyEquipSlots;

        if (incomingRevealAction == ActionType.RevealHand
            && (HasProtectDrawnCardsAura(ownMonsterSlots, protectedSideIsPlayer)
                || HasProtectDrawnCardsAura(ownEquipSlots, protectedSideIsPlayer)))
        {
            return true;
        }

        if (incomingRevealAction == ActionType.RevealHandMultiple
            && (HasProtectRevealHandMultipleAura(ownMonsterSlots, protectedSideIsPlayer)
                || HasProtectRevealHandMultipleAura(ownEquipSlots, protectedSideIsPlayer)))
        {
            return true;
        }

        return HasHandRevealSuppressionAura(ownMonsterSlots, sourceCardCost)
            || HasHandRevealSuppressionAura(ownEquipSlots, sourceCardCost);
    }

    bool HasProtectDrawnCardsAura(Transform[] sourceSlots, bool sourceIsPlayer)
    {
        if (sourceSlots == null) return false;

        foreach (Transform slot in sourceSlots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI auraCard = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (auraCard == null || auraCard.GetData() == null) continue;

            CardData auraData = auraCard.GetData();
            if (auraData.effects == null || auraData.effects.Count == 0) continue;

            foreach (CardEffect aura in auraData.effects)
            {
                if (aura.trigger != EffectTrigger.Continuous) continue;
                if (aura.action != ActionType.ProtectDrawnCards) continue;

                if (IsEffectSuppressedByOpponentContinuousAura(auraCard, aura, EffectTrigger.Continuous, sourceIsPlayer))
                {
                    continue;
                }

                Debug.Log($"🔒 Drawn cards are protected by {auraData.cardName}");
                return true;
            }
        }

        return false;
    }

    bool HasProtectRevealHandMultipleAura(Transform[] sourceSlots, bool sourceIsPlayer)
    {
        if (sourceSlots == null) return false;

        foreach (Transform slot in sourceSlots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI auraCard = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (auraCard == null || auraCard.GetData() == null) continue;

            CardData auraData = auraCard.GetData();
            if (auraData.effects == null || auraData.effects.Count == 0) continue;

            foreach (CardEffect aura in auraData.effects)
            {
                if (aura.trigger != EffectTrigger.Continuous) continue;
                if (aura.action != ActionType.ProtectRevealHandMultiple) continue;

                if (IsEffectSuppressedByOpponentContinuousAura(auraCard, aura, EffectTrigger.Continuous, sourceIsPlayer))
                {
                    continue;
                }

                Debug.Log($"🔒 RevealHandMultiple is protected by {auraData.cardName}");
                return true;
            }
        }

        return false;
    }

    bool HasHandRevealSuppressionAura(Transform[] sourceSlots, int sourceCardCost = -1)
    {
        if (sourceSlots == null) return false;

        foreach (Transform slot in sourceSlots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI auraCard = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (auraCard == null || auraCard.GetData() == null) continue;

            CardData auraData = auraCard.GetData();
            if (auraData.effects == null || auraData.effects.Count == 0) continue;

            foreach (CardEffect aura in auraData.effects)
            {
                if (aura.trigger != EffectTrigger.Continuous || aura.action != ActionType.DisableAbility) continue;

                bool targetsRevealZone = aura.targetType == TargetType.EnemyHand || aura.targetType == TargetType.EnemyPlayer || aura.targetType == TargetType.AllGlobal;
                if (!targetsRevealZone) continue;

                bool allowsRevealAction = aura.disableAbilityActionFilter == ActionType.None
                    || aura.disableAbilityActionFilter == ActionType.RevealHand
                    || aura.disableAbilityActionFilter == ActionType.RevealHandMultiple;
                if (!allowsRevealAction) continue;

                bool allowsContinuousTrigger = aura.disableAbilityTriggerFilter == EffectTrigger.None
                    || aura.disableAbilityTriggerFilter == EffectTrigger.Continuous
                    || aura.disableAbilityTriggerFilter == EffectTrigger.OnDeploy;

                bool costMatch = aura.value <= 0 || (sourceCardCost >= 0 && sourceCardCost <= aura.value);
                if (!costMatch) continue;

                if (allowsContinuousTrigger)
                {
                    Debug.Log($"🔒 Hand reveal blocked by {auraData.cardName}");
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsMonsterAttackBlockedByContinuousEffect(BattleCardUI attacker)
    {
        if (attacker == null || attacker.GetData() == null) return false;

        CardData attackerData = attacker.GetData();
        if (attackerData.type != CardType.Monster && attackerData.type != CardType.Token) return false;

        bool attackerIsPlayer = IsCardOwnedByPlayer(attacker);

        Transform[] enemyMonsterLine = attackerIsPlayer ? enemyMonsterSlots : playerMonsterSlots;
        Transform[] enemyEquipLine = attackerIsPlayer ? enemyEquipSlots : playerEquipSlots;

        bool blockedByMonsterLine = HasContinuousDisableAttackFromLine(enemyMonsterLine, attackerData);
        if (blockedByMonsterLine) return true;

        bool blockedByEquipLine = HasContinuousDisableAttackFromLine(enemyEquipLine, attackerData);
        return blockedByEquipLine;
    }

    bool HasContinuousDisableAttackFromLine(Transform[] sourceSlots, CardData attackerData)
    {
        if (sourceSlots == null || attackerData == null) return false;

        foreach (Transform slot in sourceSlots)
        {
            if (slot == null || slot.childCount == 0) continue;

            BattleCardUI sourceCard = slot.GetChild(0).GetComponent<BattleCardUI>();
            if (sourceCard == null || sourceCard.GetData() == null) continue;

            CardData sourceData = sourceCard.GetData();
            if (sourceData.effects == null || sourceData.effects.Count == 0) continue;

            foreach (CardEffect effect in sourceData.effects)
            {
                if (effect.trigger != EffectTrigger.Continuous) continue;
                if (effect.action != ActionType.DisableAttack) continue;
                if (effect.targetType != TargetType.EnemyMonster) continue;

                bool categoryMatch = true;
                if (effect.targetMainCat != MainCategory.General)
                {
                    categoryMatch = attackerData.mainCategory == effect.targetMainCat;
                }
                if (categoryMatch && effect.targetSubCat != SubCategory.General)
                {
                    categoryMatch = attackerData.subCategory == effect.targetSubCat;
                }

                if (!categoryMatch) continue;

                if (!string.IsNullOrWhiteSpace(effect.targetCardNameFilter))
                {
                    if (string.IsNullOrWhiteSpace(attackerData.cardName)) continue;

                    bool nameMatch = string.Equals(
                        attackerData.cardName.Trim(),
                        effect.targetCardNameFilter.Trim(),
                        System.StringComparison.OrdinalIgnoreCase
                    );

                    if (!nameMatch) continue;
                }

                // value <= 0 หมายถึงไม่จำกัด cost, value > 0 หมายถึง cost ต้อง <= value
                bool costMatch = effect.value <= 0 || attackerData.cost <= effect.value;
                if (!costMatch) continue;

                Debug.Log($"🚫 [Cont.DisableAttack] {attackerData.cardName} ถูกห้ามโจมตีโดย {sourceData.cardName}");
                return true;
            }
        }

        return false;
    }

    /// <summary>ทำลายการ์ดบนสนาม/มือ พร้อมส่งลงสุสาน และอัปเดตตัวนับ</summary>
    void DestroyCardToGraveyard(BattleCardUI card)
    {
        if (card == null) return;

        var cardData = card.GetData();
        if (cardData == null) return;

        // 🎮 ถ้ากำลังถูกควบคุม ให้ใช้เจ้าของดั้งเดิม ไม่ใช่เจ้าของขณะนี้
        bool ownerIsPlayer = card.isControlled ? card.originalOwnerIsPlayer : IsCardOwnedByPlayer(card);
        string cardType = (cardData.type == CardType.EquipSpell) ? "EQUIP" : "MONSTER";

        // 📊 บันทึกสถิติ: การ์ดที่ถูกทำลาย
        if (ownerIsPlayer)
        {
            currentBattleStats.playerCardsDestroyed++;
            if (cardData.type == CardType.Monster)
            {
                currentBattleStats.playerMonstersLost++;
            }
        }
        else
        {
            currentBattleStats.enemyCardsDestroyed++;
            if (cardData.type == CardType.Monster)
            {
                currentBattleStats.monstersDefeated++;
            }
        }

        Debug.Log($"💥 DestroyCardToGraveyard: {cardData.cardName} ({cardType}) -> {(ownerIsPlayer ? "Player" : "Bot")} Graveyard");
        AddBattleLog($"Card destroyed: {cardData.cardName} ({cardType})");

        SendToGraveyard(cardData, ownerIsPlayer);
        Destroy(card.gameObject);
        UpdateGraveyardCountUI();
    }

    bool MatchesCategory(CardData cardData, CardEffect effect)
    {
        if (effect.useExcludeFilter)
        {
            bool excludedByMain = effect.excludeMainCat != MainCategory.General && cardData.mainCategory == effect.excludeMainCat;
            bool excludedBySub = effect.excludeSubCat != SubCategory.General && cardData.subCategory == effect.excludeSubCat;

            if (excludedByMain || excludedBySub)
            {
                Debug.Log($"❌ Excluded target: {cardData.cardName} (Main={cardData.mainCategory}, Sub={cardData.subCategory})");
                return false;
            }
        }

        // 🔥 ถ้าเป็น General ทั้งคู่ = ทุกการ์ด
        if (effect.targetMainCat == MainCategory.General && effect.targetSubCat == SubCategory.General)
            return true;

        // 🔥 ถ้าระบุ MainCategory ต้องตรงกัน
        if (effect.targetMainCat != MainCategory.General && cardData.mainCategory != effect.targetMainCat)
        {
            Debug.Log($"❌ MainCategory ไม่ตรง: card={cardData.mainCategory} vs effect={effect.targetMainCat}");
            return false;
        }

        // 🔥 ถ้าระบุ SubCategory ต้องตรงกัน (และ MainCategory ต้องตรงด้วย)
        if (effect.targetSubCat != SubCategory.General)
        {
            // ตรวจสอบว่า SubCategory ตรงกัน
            if (cardData.subCategory != effect.targetSubCat)
            {
                Debug.Log($"❌ SubCategory ไม่ตรง: card={cardData.subCategory} vs effect={effect.targetSubCat}");
                return false;
            }

            // ถ้าระบุ SubCategory แต่ MainCategory เป็น General = ยอมรับทุก MainCategory ที่มี SubCategory นี้
            if (effect.targetMainCat == MainCategory.General)
            {
                Debug.Log($"✅ SubCategory ตรง ({cardData.subCategory}) ไม่สนใจ MainCategory");
                return true;
            }
        }

        Debug.Log($"✅ Category ตรง: Main={cardData.mainCategory}, Sub={cardData.subCategory}");
        return true;
    }

    // ========================================================
    // 🪦 GRAVEYARD SYSTEM
    // ========================================================

    /// <summary>ส่งการ์ดลงสุสาน</summary>
    void SendToGraveyard(CardData cardData, bool isPlayer)
    {
        if (cardData == null) return;

        if (isPlayer)
        {
            playerGraveyard.Add(cardData);
            string cardType = (cardData.type == CardType.EquipSpell) ? "EQUIP" : "MONSTER";
            Debug.Log($"🪦 Player Graveyard +1: {cardData.cardName} ({cardType}) | Total: {playerGraveyard.Count}");
            AddBattleLog($"  {cardData.cardName} -> Player Graveyard");
        }
        else
        {
            enemyGraveyard.Add(cardData);
            string cardType = (cardData.type == CardType.EquipSpell) ? "EQUIP" : "MONSTER";
            Debug.Log($"🪦 Bot Graveyard +1: {cardData.cardName} ({cardType}) | Total: {enemyGraveyard.Count}");
            AddBattleLog($"  {cardData.cardName} -> Bot Graveyard");
        }

        UpdateGraveyardCountUI();
    }

    // 🔥 Helper ฟังก์ชันสำหรับสกิล GraveyardATK
    public int GetPlayerGraveyardCount() => playerGraveyard.Count;
    public int GetEnemyGraveyardCount() => enemyGraveyard.Count;

    /// <summary>ดึงการ์ดจากสุสาน (เพื่อเรียกกลับมา)</summary>
    CardData RestoreFromGraveyard(int index, bool isPlayer)
    {
        if (isPlayer)
        {
            if (index >= 0 && index < playerGraveyard.Count)
            {
                CardData card = playerGraveyard[index];
                playerGraveyard.RemoveAt(index);
                Debug.Log($"✨ Restore from Player Graveyard: {card.cardName} (remaining: {playerGraveyard.Count})");
                return card;
            }
        }
        else
        {
            if (index >= 0 && index < enemyGraveyard.Count)
            {
                CardData card = enemyGraveyard[index];
                enemyGraveyard.RemoveAt(index);
                Debug.Log($"✨ Restore from Enemy Graveyard: {card.cardName} (remaining: {enemyGraveyard.Count})");
                return card;
            }
        }
        return null;
    }

    /// <summary>นับจำนวนการ์ดในสุสาน</summary>
    int GetGraveyardCount(bool isPlayer)
    {
        return isPlayer ? playerGraveyard.Count : enemyGraveyard.Count;
    }

    /// <summary>ดึงการ์ดใหม่ล่าสุดจากสุสาน (index สุดท้าย)</summary>
    CardData GetLastGraveyardCard(bool isPlayer)
    {
        var graveyard = isPlayer ? playerGraveyard : enemyGraveyard;
        if (graveyard.Count > 0)
        {
            return graveyard[graveyard.Count - 1];
        }
        return null;
    }

    bool HasEquipInGraveyard(bool isPlayer)
    {
        var graveyard = isPlayer ? playerGraveyard : enemyGraveyard;
        return graveyard.Any(card => card != null && card.type == CardType.EquipSpell);
    }

    IEnumerator ApplyReturnEquipFromGraveyard(BattleCardUI sourceCard, CardEffect effect, bool isPlayer)
    {
        Debug.Log($"🎯 ApplyReturnEquipFromGraveyard: source={sourceCard?.GetData()?.cardName ?? "Unknown"}, player={isPlayer}");

        if (effect.targetType != TargetType.Self)
        {
            Debug.LogWarning("⚠️ ReturnEquipFromGraveyard only supports TargetType.Self");
            yield break;
        }

        if (!HasEquipInGraveyard(isPlayer))
        {
            Debug.Log($"⚠️ ไม่มี Equip Spell ในสุสานให้ดึงกลับ ({(isPlayer ? "Player" : "Enemy")})");
            yield break;
        }

        if (isPlayer)
        {
            Debug.Log($"👤 Player turn - opening graveyard selection UI");
            yield return StartCoroutine(PlayerChooseEquipFromGraveyard(isPlayer: true));
        }
        else
        {
            Debug.Log($"🤖 Bot turn - auto-selecting first equip");
            yield return StartCoroutine(ReturnFirstEquipFromGraveyard(isPlayer: false));
        }

        Debug.Log($"✅✅✅ ApplyReturnEquipFromGraveyard COMPLETELY DONE - Control returns to caller ✅✅✅");
    }

    IEnumerator ReturnFirstEquipFromGraveyard(bool isPlayer)
    {
        var graveyard = isPlayer ? playerGraveyard : enemyGraveyard;
        var equipCard = graveyard.FirstOrDefault(card => card != null && card.type == CardType.EquipSpell);
        
        if (equipCard == null)
        {
            Debug.Log("⚠️ ReturnFirstEquipFromGraveyard: no Equip Spell found");
            yield break;
        }

        Debug.Log($"🪦 Bot auto-selecting: {equipCard.cardName}");
        Debug.Log($"📊 Graveyard before removal: {graveyard.Count} cards");
        
        // 🔥 ทำสำเนา CardData ก่อนลบออกจาก graveyard
        CardData cardDataCopy = equipCard;
        graveyard.Remove(equipCard);
        UpdateGraveyardCountUI();
        
        Debug.Log($"📊 Graveyard after removal: {graveyard.Count} cards");
        Debug.Log($"➕ Adding {cardDataCopy.cardName} to {(isPlayer ? "Player" : "Enemy")} hand");
        
        // 🔥 ใช้สำเนา ไม่ใช้ original
        cardAdditionComplete = false;
        AddCardToHandFromData(cardDataCopy, isPlayer);
        
        // 🔥 รอจนกว่าการเพิ่มการ์ดจะสิ้นสุด (max 5 seconds timeout)
        float timeout = Time.time + 5f;
        while (!cardAdditionComplete && Time.time < timeout)
        {
            yield return null;
        }
        
        if (Time.time >= timeout)
        {
            Debug.LogError("❌ TIMEOUT: AddCardToHandFromData ไม่จบใน 5 วินาที!");
            cardAdditionComplete = true;
        }
        
        AddBattleLog($"{(isPlayer ? "Player" : "Bot")} returned {cardDataCopy.cardName} from graveyard");
        
        yield return new WaitForEndOfFrame();
        Debug.Log($"✅ ReturnFirstEquipFromGraveyard completed for {(isPlayer ? "Player" : "Bot")}");
    }

    IEnumerator PlayerChooseEquipFromGraveyard(bool isPlayer)
    {
        var graveyard = isPlayer ? playerGraveyard : enemyGraveyard;
        if (!HasEquipInGraveyard(isPlayer)) yield break;

        GameObject panel = isPlayer ? playerGraveyardPanel : enemyGraveyardPanel;
        Transform root = isPlayer ? playerGraveyardListRoot : enemyGraveyardListRoot;

        if (panel == null || root == null)
        {
            Debug.LogError("❌ Graveyard panel/root is NULL! Cannot select equip.");
            yield break;
        }

        Debug.Log($"🪦 Opening graveyard selection for {(isPlayer ? "Player" : "Enemy")} - has {graveyard.Count} cards");

        isChoosingGraveyardEquip = true;
        graveyardEquipConfirmed = false;
        selectedGraveyardEquip = null;

        panel.SetActive(true);
        ClearListRoot(root);
        SetupGraveyardScroll(panel, root);
        PopulateGraveyardListForSelection(root, graveyard);

        Debug.Log($"⏳ Waiting for player to right-click a card...");
        while (!graveyardEquipConfirmed)
        {
            yield return null;
        }

        panel.SetActive(false);
        isChoosingGraveyardEquip = false;

        if (selectedGraveyardEquip != null)
        {
            Debug.Log($"✅ Player confirmed: {selectedGraveyardEquip.cardName}");
            Debug.Log($"📊 Graveyard before removal: {graveyard.Count} cards");
            
            // 🔥 ทำสำเนา CardData ก่อนลบออกจาก graveyard
            CardData cardDataCopy = selectedGraveyardEquip;
            graveyard.Remove(selectedGraveyardEquip);
            UpdateGraveyardCountUI();
            
            Debug.Log($"📊 Graveyard after removal: {graveyard.Count} cards");
            Debug.Log($"➕ Adding {cardDataCopy.cardName} to {(isPlayer ? "Player" : "Enemy")} hand");
            
            // 🔥 ใช้สำเนา ไม่ใช้ original
            cardAdditionComplete = false;
            AddCardToHandFromData(cardDataCopy, isPlayer);
            
            // 🔥 รอจนกว่าการเพิ่มการ์ดจะสิ้นสุด (max 5 seconds timeout)
            float timeout = Time.time + 5f;
            while (!cardAdditionComplete && Time.time < timeout)
            {
                yield return null;
            }
            
            if (Time.time >= timeout)
            {
                Debug.LogError("❌ TIMEOUT: AddCardToHandFromData ไม่จบใน 5 วินาที!");
                cardAdditionComplete = true;
            }
            
            AddBattleLog($"{(isPlayer ? "Player" : "Bot")} returned {cardDataCopy.cardName} from graveyard");
        }
        else
        {
            Debug.Log($"⚠️ Graveyard selection cancelled (None selected)");
        }
    }

    void AddCardToHandFromData(CardData cardData, bool isPlayer)
    {
        if (cardData == null || cardPrefab == null)
        {
            Debug.LogError($"❌ AddCardToHandFromData: cardData={cardData}, cardPrefab={cardPrefab}");
            cardAdditionComplete = true;
            return;
        }

        Transform targetHand = isPlayer ? handArea : enemyHandArea;
        if (targetHand == null)
        {
            Debug.LogError($"❌ targetHand is NULL for {(isPlayer ? "Player" : "Enemy")}! handArea={handArea}, enemyHandArea={enemyHandArea}");
            cardAdditionComplete = true;
            return;
        }

        cardAdditionInProgress = true;
        cardAdditionComplete = false;

        Debug.Log($"🔄 AddCardToHandFromData START: {cardData.cardName} to {(isPlayer ? "Player" : "Enemy")} hand");
        Debug.Log($"   targetHand.name={targetHand.name}, childCount before={targetHand.childCount}");

        var ui = Instantiate(cardPrefab, targetHand).GetComponent<BattleCardUI>();
        if (ui == null)
        {
            Debug.LogError("❌ cardPrefab ไม่มี BattleCardUI component!");
            cardAdditionInProgress = false;
            cardAdditionComplete = true;
            return;
        }

        Debug.Log($"✅ Card instantiated: {ui.gameObject.name}");
        Debug.Log($"   parent={ui.transform.parent.name}, parentChildCount={ui.transform.parent.childCount}");

        ui.Setup(cardData);
        ui.transform.localScale = Vector3.one;
        ui.transform.localPosition = Vector3.zero;

        Debug.Log($"✅ Card Setup done: localPos={ui.transform.localPosition}");

        if (!isPlayer)
        {
            var img = ui.GetComponent<Image>();
            if (img != null && cardBackPrefab != null)
            {
                var backImg = cardBackPrefab.GetComponent<Image>();
                if (backImg != null && backImg.sprite != null)
                {
                    img.sprite = backImg.sprite;
                }
            }
            ui.SetFrameVisible(false);

            var cg = ui.GetComponent<CanvasGroup>();
            if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 1f;
        }

        Debug.Log($"📍 Before Arrange: {(isPlayer ? "Player" : "Enemy")} hand has {targetHand.childCount} cards");
        
        try
        {
            if (isPlayer)
            {
                Debug.Log($"📍 Calling ArrangeCardsInHand()...");
                ArrangeCardsInHand();
            }
            else
            {
                Debug.Log($"📍 Calling ArrangeEnemyHand()...");
                ArrangeEnemyHand();
            }

            // 🔥 ตรวจสอบว่า card ยังคงมีชีวิต
            if (ui == null || ui.gameObject == null || !ui.gameObject.activeInHierarchy)
            {
                Debug.LogError($"❌ CRITICAL: Card destroyed during arrangement!");
                cardAdditionInProgress = false;
                cardAdditionComplete = true;
                return;
            }

            Debug.Log($"✅ {cardData.cardName} successfully added! Hand now has {targetHand.childCount} children");
            
            // 🔥 ตรวจสอบว่า card ยังคงเป็น child ของ targetHand หรือเปล่า
            if (ui.transform.parent != targetHand)
            {
                Debug.LogError($"❌ CRITICAL: Card parent changed! Was {targetHand.name}, now {ui.transform.parent?.name}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ EXCEPTION in AddCardToHandFromData: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            cardAdditionInProgress = false;
            cardAdditionComplete = true;
            Debug.Log($"✅ AddCardToHandFromData COMPLETE: {cardData.cardName}");
        }
    }

    // ========================================================
    // 🪦 GRAVEYARD UI HELPERS (สำหรับปุ่มใน HUD)
    // ========================================================

    void UpdateGraveyardCountUI()
    {
        if (playerGraveyardCountText != null)
            playerGraveyardCountText.text = playerGraveyard.Count.ToString();

        if (enemyGraveyardCountText != null)
            enemyGraveyardCountText.text = enemyGraveyard.Count.ToString();
    }

    public void TogglePlayerGraveyardPanel()
    {
        if (playerGraveyardPanel == null) return;
        bool show = !playerGraveyardPanel.activeSelf;
        playerGraveyardPanel.SetActive(show);
        if (show) RefreshPlayerGraveyardUI();
    }

    public void ClosePlayerGraveyardPanel()
    {
        if (playerGraveyardPanel != null)
            playerGraveyardPanel.SetActive(false);
    }

    public void ToggleEnemyGraveyardPanel()
    {
        if (enemyGraveyardPanel == null) return;
        bool show = !enemyGraveyardPanel.activeSelf;
        enemyGraveyardPanel.SetActive(show);
        if (show) RefreshEnemyGraveyardUI();
    }

    public void CloseEnemyGraveyardPanel()
    {
        if (enemyGraveyardPanel != null)
            enemyGraveyardPanel.SetActive(false);
    }

    /// <summary>ปิด graveyard panels ทั้งหมด (เรียกจาก background click หรือ ESC)</summary>
    public void CloseAllGraveyardPanels()
    {
        ClosePlayerGraveyardPanel();
        CloseEnemyGraveyardPanel();
    }

    // จัดสไตล์ให้ Log Panel ดูโปร่งและพอดีจอพร้อม Scroll
    void SetupLogPanelAppearance()
    {
        if (!autoStyleLogPanel || logPanel == null) return;

        var rt = logPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(logPanelMargin.x, logPanelMargin.y);
            rt.offsetMax = new Vector2(-logPanelMargin.x, -logPanelMargin.y);
        }

        // ตั้งค่าพื้นหลังโปร่ง
        var bg = logPanel.GetComponent<Image>();
        if (bg == null)
        {
            bg = logPanel.AddComponent<Image>();
        }
        bg.color = new Color(0f, 0f, 0f, Mathf.Clamp01(logPanelOpacity));
        bg.raycastTarget = true;

        // ตั้งค่า ScrollRect ถ้ายังไม่มี
        if (logScrollRect == null)
        {
            logScrollRect = logPanel.GetComponent<ScrollRect>();
        }

        if (logScrollRect != null && logText != null)
        {
            // ตั้งค่า Content เป็น logText
            var contentRT = logText.GetComponent<RectTransform>();
            if (contentRT != null)
            {
                logScrollRect.content = contentRT;

                // ตั้งค่า Content Layout
                var layoutGroup = logText.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = logText.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = 2f;

                // ตั้งค่า ContentSizeFitter
                var fitter = logText.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = logText.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // บังคับรีบิลด์เลย์เอาต์
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
            }

            // ตั้งค่า ScrollRect: เลื่อนแนวตั้งเท่านั้น
            logScrollRect.horizontal = false;
            logScrollRect.vertical = true;
            logScrollRect.movementType = ScrollRect.MovementType.Clamped;
            logScrollRect.elasticity = 0.1f;
            logScrollRect.inertia = true;

            Debug.Log("✅ Log Panel Scroll Setup Complete");
        }
    }

    // --------------------------------------------------------
    // ⏸️ PAUSE & LOG SYSTEM
    // --------------------------------------------------------

    void OnPausePressed()
    {
        if (isEnding) return;
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnResumePressed()
    {
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        if (logPanel) logPanel.SetActive(false);
    }

    void OnQuitBattlePressed()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(stageSceneName))
        {
            SceneManager.LoadScene(stageSceneName);
        }
    }

    void OnToggleLogPanel()
    {
        if (logPanel == null) return;
        bool show = !logPanel.activeSelf;
        logPanel.SetActive(show);
        if (show) UpdateLogText();
    }

    void AddBattleLog(string entry)
    {
        if (string.IsNullOrEmpty(entry)) return;
        if (battleLog.Count >= battleLogLimit)
            battleLog.RemoveAt(0);
        battleLog.Add($"T{turnCount}: {entry}");
        UpdateLogText();
    }

    void UpdateLogText()
    {
        if (logText == null) return;
        logText.text = string.Join("\n", battleLog);
    }

    public void RefreshPlayerGraveyardUI()
    {
        if (playerGraveyardCountText != null)
            playerGraveyardCountText.text = playerGraveyard.Count.ToString();

        ClearListRoot(playerGraveyardListRoot);
        SetupGraveyardScroll(playerGraveyardPanel, playerGraveyardListRoot);
        PopulateGraveyardList(playerGraveyardListRoot, playerGraveyard);
    }

    public void RefreshEnemyGraveyardUI()
    {
        if (enemyGraveyardCountText != null)
            enemyGraveyardCountText.text = enemyGraveyard.Count.ToString();

        ClearListRoot(enemyGraveyardListRoot);
        SetupGraveyardScroll(enemyGraveyardPanel, enemyGraveyardListRoot);
        PopulateGraveyardList(enemyGraveyardListRoot, enemyGraveyard);
    }

    void SetupGraveyardScroll(GameObject panel, Transform content)
    {
        if (panel == null || content == null) return;

        // หา ScrollRect component
        var scrollRect = panel.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = panel.AddComponent<ScrollRect>();
        }

        // ตั้งค่า content
        scrollRect.content = content as RectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        // เพิ่ม Mask เพื่อซ่อนการ์ดที่ออกนอกขอบ
        var mask = panel.GetComponent<Mask>();
        if (mask == null)
        {
            mask = panel.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }

        // ต้องมี Image component สำหรับ Mask
        var img = panel.GetComponent<Image>();
        if (img == null)
        {
            img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f); // พื้นหลังสีดำโปร่งใส
        }

        Debug.Log($"✅ Setup Scroll for {panel.name}");
    }

    void PopulateGraveyardList(Transform root, List<CardData> cards)
    {
        if (root == null)
        {
            Debug.LogError("❌ Graveyard List Root ยังไม่ได้ assign ใน Inspector!");
            return;
        }

        if (cards == null) return;

        Debug.Log($"🪦 PopulateGraveyardList: root={root.name}, cardCount={cards.Count}");

        // ลบ VerticalLayoutGroup ถ้ามี
        var oldLayout = root.GetComponent<VerticalLayoutGroup>();
        if (oldLayout != null) DestroyImmediate(oldLayout);

        // ใช้ GridLayoutGroup เพื่อให้มีการ์ดหลายใบต่อแถว
        var gridLayout = root.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = root.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.spacing = new Vector2(8f, 8f); // ระยะห่างระหว่างการ์ด
        gridLayout.cellSize = new Vector2(180f, 220f); // ขนาดของแต่ละช่อง
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // จำกัดจำนวนคอลัมน์ต่อแถว
        gridLayout.constraintCount = 6; // 6 การ์ดต่อแถว

        var fitter = root.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = root.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (graveyardListItemPrefab != null)
        {
            // ใช้ prefab ที่กำหนด
            // ปรับ scale อัตโนมัติเพื่อไม่ให้ล้นจอเมื่อมีการ์ดจำนวนมาก
            float scaleFactor = 0.75f;
            if (cards.Count > 18) scaleFactor = 0.55f;
            else if (cards.Count > 12) scaleFactor = 0.65f;

            int successCount = 0;
            foreach (var card in cards)
            {
                var item = Instantiate(graveyardListItemPrefab, root);
                item.transform.localScale = Vector3.one * scaleFactor;
                item.name = $"Graveyard_{card.cardName}";

                var ui = item.GetComponent<BattleCardUI>();
                if (ui != null)
                {
                    ui.Setup(card);

                    // ตั้ง CanvasGroup ให้คลิกได้แน่นอน
                    var cg = ui.GetComponent<CanvasGroup>();
                    if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    cg.alpha = 1f;

                    // ตั้งค่า Image ของ Button ให้รับ raycast
                    var img = item.GetComponent<Image>();
                    if (img != null)
                    {
                        img.raycastTarget = true;
                    }

                    CardData cardData = card; // capture ค่าสำหรับ lambda

                    // ใช้ EventTrigger (PointerClick) แทน Button ที่อาจไม่ทำงาน
                    var eventTrigger = item.GetComponent<EventTrigger>();
                    if (eventTrigger == null) eventTrigger = item.AddComponent<EventTrigger>();

                    // ล้างเหตุการณ์เก่า
                    eventTrigger.triggers.Clear();

                    // เพิ่ม PointerClick trigger
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerClick;
                    entry.callback.AddListener((data) =>
                    {
                        if (cardDetailView != null)
                        {
                            cardDetailView.Open(cardData);
                            Debug.Log($"🔓 เปิดรายละเอียด: {cardData.cardName}");
                        }
                        else
                        {
                            Debug.LogError("❌ CardDetailView ยังไม่ได้ตั้งค่าใน BattleManager!");
                        }
                    });
                    eventTrigger.triggers.Add(entry);

                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Graveyard item ไม่มี BattleCardUI component: {card.cardName}");
                }
            }
            Debug.Log($"✅ สร้างการ์ดสุสาน {successCount}/{cards.Count} ใบ");

            // รีเฟรชเลย์เอาต์เพื่อให้ ScrollRect ปรับขนาดทันที
            LayoutRebuilder.ForceRebuildLayoutImmediate(root as RectTransform);
            return;
        }

        // Fallback: สร้าง Text แถวง่ายๆ ถ้าไม่มี prefab
        Debug.LogWarning("⚠️ graveyardListItemPrefab ไม่ได้ assign - ใช้ Fallback Text");
        foreach (var card in cards)
        {
            var go = new GameObject("GraveyardItem");
            go.transform.SetParent(root, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = $"• {card.cardName} (ATK:{card.atk} HP:{card.hp})";
            text.fontSize = 20f;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
        }

        // รีเฟรชเลย์เอาต์เพื่อให้ ScrollRect ปรับขนาดทันที
        LayoutRebuilder.ForceRebuildLayoutImmediate(root as RectTransform);
    }

    void PopulateGraveyardListForSelection(Transform root, List<CardData> cards)
    {
        if (root == null)
        {
            Debug.LogError("❌ Graveyard List Root ยังไม่ได้ assign ใน Inspector!");
            return;
        }

        if (cards == null) return;

        var oldLayout = root.GetComponent<VerticalLayoutGroup>();
        if (oldLayout != null) DestroyImmediate(oldLayout);

        var gridLayout = root.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = root.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.spacing = new Vector2(8f, 8f);
        gridLayout.cellSize = new Vector2(180f, 220f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 6;

        var fitter = root.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = root.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (graveyardListItemPrefab == null)
        {
            Debug.LogError("❌ graveyardListItemPrefab ยังไม่ได้ตั้งค่า!");
            return;
        }

        float scaleFactor = 0.75f;
        if (cards.Count > 18) scaleFactor = 0.55f;
        else if (cards.Count > 12) scaleFactor = 0.65f;

        foreach (var card in cards)
        {
            if (card == null) continue;
            var item = Instantiate(graveyardListItemPrefab, root);
            item.transform.localScale = Vector3.one * scaleFactor;
            item.name = $"Graveyard_{card.cardName}";

            var ui = item.GetComponent<BattleCardUI>();
            if (ui != null)
            {
                ui.Setup(card);

                var cg = ui.GetComponent<CanvasGroup>();
                if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.alpha = 1f;

                var img = item.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;

                CardData cardData = card;

                var eventTrigger = item.GetComponent<EventTrigger>();
                if (eventTrigger == null) eventTrigger = item.AddComponent<EventTrigger>();
                eventTrigger.triggers.Clear();

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) =>
                {
                    PointerEventData pointerData = (PointerEventData)data;
                    
                    Debug.Log($"🎯 GraveyardEquip EventTrigger: button={pointerData.button}, cardType={cardData.type}, isChoosingGraveyardEquip={isChoosingGraveyardEquip}");
                    
                    // 🔥 Right-click: เลือกการ์ด, Left-click: ดูรายละเอียด
                    if (pointerData.button == PointerEventData.InputButton.Right)
                    {
                        Debug.Log($"📍 Right-click on {cardData.cardName}, type={cardData.type}");
                        if (cardData.type == CardType.EquipSpell)
                        {
                            if (isChoosingGraveyardEquip)
                            {
                                Debug.Log($"✅ Right-click selected: {cardData.cardName}");
                                OnGraveyardEquipSelected(cardData);
                            }
                            else
                            {
                                Debug.LogWarning($"⚠️ isChoosingGraveyardEquip ยังเป็น false!");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ Card ไม่ใช่ EquipSpell: {cardData.type}");
                        }
                    }
                    else if (pointerData.button == PointerEventData.InputButton.Left)
                    {
                        Debug.Log($"📍 Left-click on {cardData.cardName}");
                        if (cardDetailView != null)
                        {
                            cardDetailView.Open(cardData);
                            Debug.Log($"🔓 เปิดรายละเอียด: {cardData.cardName}");
                        }
                    }
                });
                eventTrigger.triggers.Add(entry);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(root as RectTransform);
    }

    void OnGraveyardEquipSelected(CardData cardData)
    {
        if (!isChoosingGraveyardEquip || cardData == null)
        {
            Debug.LogWarning($"⚠️ OnGraveyardEquipSelected: invalid state (choosing={isChoosingGraveyardEquip}, card={cardData})");
            return;
        }
        if (cardData.type != CardType.EquipSpell)
        {
            Debug.LogWarning($"⚠️ OnGraveyardEquipSelected: not an EquipSpell! type={cardData.type}");
            return;
        }

        selectedGraveyardEquip = cardData;
        graveyardEquipConfirmed = true;
        Debug.Log($"✅ Right-click selected equip from graveyard: {cardData.cardName}");
    }

    void ClearListRoot(Transform root)
    {
        if (root == null) return;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i);
            if (child != null) Destroy(child.gameObject);
        }
    }

    // ========================================================
    // 👁️ HAND REVEAL SYSTEM (ดูการ์ดบนมือฝ่ายตรงข้าม)
    // ========================================================

    /// <summary>เปิด Panel แสดงการ์ดบนมือที่ดูได้</summary>
    void ShowHandRevealPanel(List<CardData> cards, string targetName)
    {
        if (handRevealPanel == null)
        {
            Debug.LogError("❌ handRevealPanel ยังไม่ถูกตั้ง!");
            return;
        }

        if (cards == null || cards.Count == 0)
        {
            Debug.Log("⚠️ ไม่มีการ์ดที่จะแสดง");
            return;
        }

        // 🔥 เพิ่มการ์ดทั้งหมดลงใน revealedEnemyCards เพื่อให้คลิกดูรายละเอียดได้
        foreach (var card in cards)
        {
            if (card != null && !revealedEnemyCards.ContainsKey(card.card_id))
            {
                revealedEnemyCards[card.card_id] = card;
                Debug.Log($"👁️ Marked as revealed: {card.cardName}");
            }
        }

        // ตั้งชื่อ Panel
        if (handRevealTitleText != null)
        {
            handRevealTitleText.text = $"🔍 {targetName} ({cards.Count} ใบ)";
        }

        // ตั้งปุ่มปิด
        if (handRevealCloseButton != null)
        {
            handRevealCloseButton.onClick.RemoveAllListeners();
            handRevealCloseButton.onClick.AddListener(CloseHandRevealPanel);
        }

        // ล้างการ์ดเก่า
        ClearListRoot(handRevealListRoot);

        // Setup Scroll
        SetupHandRevealScroll();

        // สร้างการ์ดใหม่
        PopulateHandRevealList(cards);

        // เปิด Panel
        handRevealPanel.SetActive(true);
        Debug.Log($"✅ เปิด Hand Reveal Panel: {cards.Count} ใบ");
    }

    /// <summary>ปิด Panel แสดงการ์ดบนมือ</summary>
    void CloseHandRevealPanel()
    {
        if (handRevealPanel != null)
        {
            handRevealPanel.SetActive(false);
            Debug.Log("✅ ปิด Hand Reveal Panel");
        }
    }

    /// <summary>ตั้งค่า ScrollRect สำหรับ Hand Reveal Panel</summary>
    void SetupHandRevealScroll()
    {
        if (handRevealPanel == null || handRevealListRoot == null) return;

        // หา ScrollRect component
        var scrollRect = handRevealPanel.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = handRevealPanel.AddComponent<ScrollRect>();
        }

        // ตั้งค่า content
        scrollRect.content = handRevealListRoot as RectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;

        // ใช้ parent ของ list root เป็น viewport เพื่อ clip การ์ดไม่ให้ล้นทะลุ
        RectTransform viewportRect = handRevealListRoot.parent as RectTransform;
        if (viewportRect == null)
        {
            viewportRect = handRevealPanel.transform as RectTransform;
        }

        scrollRect.viewport = viewportRect;

        var rectMask = viewportRect.GetComponent<RectMask2D>();
        if (rectMask == null)
        {
            rectMask = viewportRect.gameObject.AddComponent<RectMask2D>();
        }

        // ต้องมี Image component เพื่อให้ viewport รับ raycast ได้ตามปกติ
        var img = viewportRect.GetComponent<Image>();
        if (img == null)
        {
            img = viewportRect.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f); // พื้นหลังสีดำโปร่งใส
        }

        Debug.Log($"✅ Setup Scroll for Hand Reveal Panel");
    }

    int GetHandRevealColumnCount(float cellWidth, int maxColumns = 8)
    {
        if (handRevealListRoot == null || handRevealListRoot.parent == null)
        {
            return Mathf.Clamp(maxColumns, 1, 8);
        }

        RectTransform viewportRect = handRevealListRoot.parent as RectTransform;
        if (viewportRect == null)
        {
            return Mathf.Clamp(maxColumns, 1, 8);
        }

        float viewportWidth = viewportRect.rect.width;
        if (viewportWidth <= 0f)
        {
            return Mathf.Clamp(maxColumns, 1, 8);
        }

        int fitColumns = Mathf.FloorToInt(viewportWidth / Mathf.Max(1f, cellWidth));
        return Mathf.Clamp(fitColumns, 1, maxColumns);
    }

    /// <summary>สร้างการ์ดและแสดงใน Hand Reveal Panel</summary>
    void PopulateHandRevealList(List<CardData> cards)
    {
        if (handRevealListRoot == null)
        {
            Debug.LogError("❌ handRevealListRoot ยังไม่ได้ assign ใน Inspector!");
            return;
        }

        if (cards == null || cards.Count == 0) return;

        Debug.Log($"👁️ PopulateHandRevealList: root={handRevealListRoot.name}, cardCount={cards.Count}");

        // ใช้ GridLayoutGroup เพื่อให้มีการ์ดหลายใบต่อแถว
        var gridLayout = handRevealListRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = handRevealListRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.spacing = new Vector2(10f, 10f);
        gridLayout.cellSize = new Vector2(200f, 280f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = GetHandRevealColumnCount(gridLayout.cellSize.x + gridLayout.spacing.x, 6);

        var fitter = handRevealListRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = handRevealListRoot.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (cardPrefab != null)
        {
            // สร้างการ์ดจาก prefab
            float scaleFactor = 1.0f;
            if (cards.Count > 10) scaleFactor = 0.85f;

            int successCount = 0;
            foreach (var card in cards)
            {
                var item = Instantiate(cardPrefab, handRevealListRoot);
                item.transform.localScale = Vector3.one * scaleFactor;
                item.name = $"Revealed_{card.cardName}";

                var ui = item.GetComponent<BattleCardUI>();
                if (ui != null)
                {
                    ui.Setup(card);

                    // ปิด interaction ของ BattleCardUI บนการ์ด preview ใน panel นี้
                    // เพื่อไม่ให้คลิกขวาไปเข้าลอจิกเล่นการ์ด/โจมตี/ลาก
                    ui.enabled = false;

                    // 🔥 แสดงรูปการ์ด
                    var img = item.GetComponent<Image>();
                    if (img != null)
                    {
                        if (card.artwork != null)
                        {
                            img.sprite = card.artwork;
                            img.color = Color.white; // ไม่โปร่งใส
                        }
                        img.raycastTarget = true;
                    }

                    // ตั้ง CanvasGroup ให้คลิกได้ (แต่ไม่ให้ลากได้)
                    var cg = ui.GetComponent<CanvasGroup>();
                    if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    cg.alpha = 1f;

                    CardData cardData = card; // capture ค่าสำหรับ lambda

                    // 🔥 เอาปุ่มออกถ้ามี (ป้องกันบัตรสำหรับ click conflict)
                    var btn = item.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                    }

                    // 🔥 เพิ่มลิสเนอร์คลิกแบบง่ายๆ สำหรับการ์ดบนมือ
                    var pointerClickHandler = item.GetComponent<PointerClickHandler>();
                    if (pointerClickHandler == null)
                    {
                        pointerClickHandler = item.AddComponent<PointerClickHandler>();
                    }
                    pointerClickHandler.OnClickAction = () =>
                    {
                        if (cardDetailView != null)
                        {
                            cardDetailView.Open(cardData);
                            Debug.Log($"🔓 เปิดรายละเอียดการ์ดบนมือ: {cardData.cardName}");
                        }
                        else
                        {
                            Debug.LogError("❌ CardDetailView ยังไม่ได้ตั้งค่าใน BattleManager!");
                        }
                    };

                    Debug.Log($"✅ Card setup: {card.cardName} | Image: {(img != null && img.sprite != null ? "OK" : "MISSING")} | PointerClickHandler: OK");

                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Hand Reveal item ไม่มี BattleCardUI component: {card.cardName}");
                }
            }
            Debug.Log($"✅ สร้างการ์ด Hand Reveal {successCount}/{cards.Count} ใบ");

            // รีเฟรชเลย์เอาต์เพื่อให้ ScrollRect ปรับขนาดทันที
            LayoutRebuilder.ForceRebuildLayoutImmediate(handRevealListRoot as RectTransform);
            return;
        }

        // Fallback: สร้าง Text แถวง่ายๆ ถ้าไม่มี prefab
        Debug.LogWarning("⚠️ cardPrefab ไม่ได้ assign - ใช้ Fallback Text");
        foreach (var card in cards)
        {
            var go = new GameObject("HandRevealItem");
            go.transform.SetParent(handRevealListRoot, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = $"• {card.cardName} (ATK:{card.atk} Cost:{card.cost})";
            text.fontSize = 22f;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
        }

        // รีเฟรชเลย์เอาต์
        LayoutRebuilder.ForceRebuildLayoutImmediate(handRevealListRoot as RectTransform);
    }

    // ========================================================
    // 🎯 TARGET SELECTION SYSTEM (สำหรับ Spell)
    // ========================================================

    /// <summary>เปิดระบบให้ผู้เล่นเลือกเป้าหมาย</summary>
    void StartSelectingTarget(List<BattleCardUI> targets, int selectCount, System.Action<List<BattleCardUI>> onComplete, bool allowCancel = false)
    {
        isSelectingTarget = true;
        availableTargets = new List<BattleCardUI>(targets);
        selectedTargets.Clear();

        if (targetSelectionPanel == null)
        {
            Debug.LogError("❌ targetSelectionPanel ยังไม่ถูกตั้ง! ไม่สามารถเลือกเป้าหมายได้");
            onComplete?.Invoke(new List<BattleCardUI>());
            return;
        }

        // ตั้งค่า UI
        if (targetSelectionText)
            targetSelectionText.text = $"เลือกเป้าหมาย ({selectedTargets.Count}/{selectCount})";

        if (targetSelectionCancelButton)
        {
            targetSelectionCancelButton.onClick.RemoveAllListeners();

            if (allowCancel)
            {
                targetSelectionCancelButton.interactable = true;
                targetSelectionCancelButton.gameObject.SetActive(true);
                targetSelectionCancelButton.onClick.AddListener(() =>
                {
                    foreach (var t in availableTargets)
                    {
                        if (t != null)
                        {
                            t.SetHighlight(false);
                            var btn = t.GetComponent<Button>();
                            if (btn != null)
                            {
                                btn.onClick.RemoveAllListeners();
                                Destroy(btn);
                            }
                        }
                    }

                    isSelectingTarget = false;
                    targetSelectionPanel.SetActive(false);
                    availableTargets.Clear();

                    onComplete?.Invoke(new List<BattleCardUI>());
                    Debug.Log("❌ ยกเลิกการเลือกเป้าหมาย");
                });
            }
            else
            {
                // เวทต้องเลือกเป้าหมายให้จบ ห้ามยกเลิก
                targetSelectionCancelButton.interactable = false;
                targetSelectionCancelButton.gameObject.SetActive(false);
            }
        }

        // ฮาइไลท์การ์ดที่เลือกได้
        foreach (var target in availableTargets)
        {
            if (target != null)
            {
                target.SetHighlight(true); // ฮาइไลท์

                // เพิ่ม Listener ให้คลิกได้
                var btn = target.GetComponent<Button>();
                if (btn == null) btn = target.gameObject.AddComponent<Button>();

                btn.onClick.RemoveAllListeners();
                BattleCardUI selectedTarget = target;
                btn.onClick.AddListener(() => HandleTargetClick(selectedTarget, selectCount, onComplete));

                Debug.Log($"✅ เลือกได้: {target.GetData().cardName}");
            }
        }

        // เปิด Panel
        targetSelectionPanel.SetActive(true);
        Debug.Log($"🎯 เปิดระบบเลือกเป้าหมาย: {selectCount} ใบจาก {availableTargets.Count}");
    }

    /// <summary>เลือกเป้าหมายเดียว หรือสะสมหลายใบตามจำนวน</summary>
    void HandleTargetClick(BattleCardUI target, int selectCount, System.Action<List<BattleCardUI>> onComplete)
    {
        if (target == null) return;

        // โหมดเลือก 1 ใบ (เช่น a01/a02/a03: เลือก 1 ทำลาย 3)
        if (selectCount == 1)
        {
            Debug.Log($"🎯 ผู้เล่นเลือก: {target.GetData().cardName}");

            // 🔥 ลบปุ่มทั้งหมดจากการ์ด
            foreach (var t in availableTargets)
            {
                if (t != null)
                {
                    t.SetHighlight(false);
                    var btn = t.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                        Destroy(btn);
                    }
                }
            }

            // ปิด Panel
            isSelectingTarget = false;
            targetSelectionPanel.SetActive(false);
            availableTargets.Clear();

            // ส่งเฉพาะใบที่เลือก
            var result = new List<BattleCardUI> { target };
            onComplete?.Invoke(result);
            Debug.Log($"✅ เสร็จการเลือก 1 เป้าหมาย");
        }
        else
        {
            // โหมดเลือกหลายใบ (รองรับสะสม)
            if (!selectedTargets.Contains(target))
            {
                selectedTargets.Add(target);
                Debug.Log($"🎯 ผู้เล่นเลือก: {target.GetData().cardName} ({selectedTargets.Count}/{selectCount})");
            }

            // อัพเดทข้อความ
            if (targetSelectionText)
                targetSelectionText.text = $"เลือกเป้าหมาย ({selectedTargets.Count}/{selectCount})";

            // ครบจำนวน → ส่งผลลัพธ์
            if (selectedTargets.Count >= selectCount)
            {
                // 🔥 ลบปุ่มทั้งหมดจากการ์ด
                foreach (var t in availableTargets)
                {
                    if (t != null)
                    {
                        t.SetHighlight(false);
                        var btn = t.GetComponent<Button>();
                        if (btn != null)
                        {
                            btn.onClick.RemoveAllListeners();
                            Destroy(btn);
                        }
                    }
                }

                isSelectingTarget = false;
                targetSelectionPanel.SetActive(false);
                availableTargets.Clear();

                onComplete?.Invoke(new List<BattleCardUI>(selectedTargets));
                selectedTargets.Clear();
                Debug.Log($"✅ เสร็จการเลือก {selectCount} เป้าหมาย");
            }
        }
    }
    /// <summary>ยกเลิกการเลือกเป้าหมาย</summary>
    void CancelTargetSelection()
    {
        // ป้องกันการยกเลิกระหว่างการใช้เวท
        if (isSelectingTarget)
        {
            Debug.LogWarning("❌ ไม่สามารถยกเลิกการเลือกเป้าหมายขณะใช้เวท");
            return;
        }

        // กรณีถูกเรียกใช้นอกระบบเวท: ปิด UI อย่างปลอดภัย
        Debug.Log("❌ ยกเลิกการเลือกเป้าหมาย (นอกระบบเวท)");
        foreach (var target in availableTargets)
        {
            if (target != null) target.SetHighlight(false);
        }
        targetSelectionPanel.SetActive(false);
        availableTargets.Clear();
    }

    /// <summary>
    /// คำนวณดาวจาก BattleStatistics สำหรับ Stage ปัจจุบัน
    /// </summary>
    private int CalculateStarsForCurrentStage(BattleStatistics stats, string stageID)
    {
        // ค้นหา StageData จาก StageManager ที่อยู่ใน Scene Stage Selection
        // เนื่องจากอยู่คนละ Scene ต้องใช้ข้อมูลที่เก็บไว้หรือ Load จาก Resources

        // วิธีที่ 1: ใช้ข้อมูลจาก GameManager (ถ้ามี)
        // วิธีที่ 2: ใช้ค่าเริ่มต้น เช่น ชนะ = 1 ดาว, เงื่อนไขเพิ่มเติม = +1, +1

        // ตอนนี้ใช้วิธีง่ายๆ ก่อน: ให้ตรวจสอบ Achievement flags
        int stars = 0;

        // ดาวที่ 1: ชนะ
        if (stats.victory)
            stars++;

        // ดาวที่ 2: เทิร์นน้อย (ต้องชนะภายใน 12 เทิร์น)
        if (stats.victory && stats.turnsPlayed <= 12)
            stars++;

        // ดาวที่ 3: ใช้ Spell อย่างน้อย 3 ครั้ง
        if (stats.victory && stats.spellsCast >= 3)
            stars++;

        Debug.Log($"[STARS] Condition 1 (Victory): {stats.victory}");
        Debug.Log($"[STARS] Condition 2 (Turns <= 12): {(stats.victory && stats.turnsPlayed <= 12)} (Turns: {stats.turnsPlayed})");
        Debug.Log($"[STARS] Condition 3 (Spells >= 3): {(stats.victory && stats.spellsCast >= 3)} (Spells: {stats.spellsCast})");
        Debug.Log($"[STARS] Total: {stars}/3 Stars");

        return Mathf.Clamp(stars, 0, 3);
    }

    /// <summary>ตรวจสอบว่าผู้โจมตีสามารถข้ามการกันของโล่ได้หรือไม่</summary>
    bool CanAttackerBypassShield(BattleCardUI attacker, BattleCardUI shield)
    {
        if (attacker == null || !attacker.canBypassIntercept) return false;
        if (shield == null || shield.GetData() == null) return false;

        CardData shieldData = shield.GetData();
        int costThreshold = attacker.bypassCostThreshold;
        int shieldCost = shieldData.cost;
        MainCategory allowedMainCat = attacker.bypassAllowedMainCat;
        SubCategory allowedSubCat = attacker.bypassAllowedSubCat;

        Debug.Log($"🔍 Check Bypass: Shield={shieldData.cardName} (Cost={shieldCost}, MainCat={shieldData.mainCategory}, SubCat={shieldData.subCategory}) | Threshold={costThreshold}, AllowedMainCat={allowedMainCat}, AllowedSubCat={allowedSubCat}");

        // value = 0 → ไม่ข้ามไม่ได้เลย
        if (costThreshold == 0)
        {
            Debug.Log($"→ Threshold=0, CANNOT bypass");
            return false;
        }

        // value = -1 → ข้ามทั้งหมด (ไม่เช็ค category)
        if (costThreshold == -1)
        {
            Debug.Log($"→ Threshold=-1, CAN bypass all");
            return true;
        }

        // 🔥 ตรวจสอบว่าโล่ตรงกับ Category ที่อนุญาต → ถ้าตรง = ไม่ถูกข้าม (สามารถ Intercept ได้)
        if (allowedMainCat != MainCategory.General && shieldData.mainCategory == allowedMainCat)
        {
            Debug.Log($"→ Shield matches AllowedMainCat={allowedMainCat}, CANNOT bypass (Shield can intercept)");
            return false;
        }

        if (allowedSubCat != SubCategory.General && shield.GetModifiedSubCategory() == allowedSubCat)
        {
            Debug.Log($"→ Shield matches AllowedSubCat={allowedSubCat}, CANNOT bypass (Shield can intercept)");
            return false;
        }

        // value > 0 → ข้ามได้เฉพาะ shield ที่ cost < threshold และไม่ใช่ Category ที่อนุญาต
        bool canBypass = shieldCost < costThreshold;
        Debug.Log($"→ Cost check: {shieldCost} < {costThreshold} = {canBypass}, Result: {(canBypass ? "CAN bypass" : "CANNOT bypass")}");
        return canBypass;
    }

    bool IsInterceptTypeMatched(BattleCardUI attacker, BattleCardUI blocker, bool blockerIsPlayer)
    {
        if (attacker == null || blocker == null) return false;
        if (attacker.GetData() == null || blocker.GetData() == null) return false;

        if (attacker.GetModifiedSubCategory() == blocker.GetModifiedSubCategory())
        {
            return true;
        }

        if (DoesBlockerAlwaysMatchTypeOnIntercept(blocker, blockerIsPlayer))
        {
            Debug.Log($"✅ Intercept type override: {blocker.GetData().cardName} counts as matching type");
            return true;
        }

        return false;
    }

    bool DoesBlockerAlwaysMatchTypeOnIntercept(BattleCardUI blocker, bool blockerIsPlayer)
    {
        if (blocker == null || blocker.GetData() == null) return false;

        CardData blockerData = blocker.GetData();
        if (blockerData.effects == null || blockerData.effects.Count == 0) return false;

        foreach (CardEffect effect in blockerData.effects)
        {
            if (effect.trigger != EffectTrigger.Continuous) continue;
            if (effect.action != ActionType.InterceptAlwaysTypeMatch) continue;

            if (IsEffectSuppressedByOpponentContinuousAura(blocker, effect, EffectTrigger.Continuous, blockerIsPlayer))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>ไฮไลท์โล่ที่สามารถกันได้ (สีเหลือง)</summary>
    void HighlightInterceptableShields(BattleCardUI attacker)
    {
        if (attacker == null) return;

        foreach (Transform slot in playerEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var shield = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (shield != null && shield.GetData() != null && shield.GetData().type == CardType.EquipSpell && !shield.cannotIntercept)
                {
                    // เช็คว่าโล่นี้สามารถกันได้หรือไม่
                    bool canIntercept = true;
                    if (attacker.canBypassIntercept)
                    {
                        canIntercept = !CanAttackerBypassShield(attacker, shield);
                    }

                    if (canIntercept)
                    {
                        shield.SetHighlight(true);
                        Debug.Log($"💛 Highlight: {shield.GetData().cardName} (can intercept)");
                    }
                }
            }
        }
    }

    /// <summary>ปิดไฮไลท์โล่ทั้งหมด</summary>
    void ClearAllShieldHighlights()
    {
        foreach (Transform slot in playerEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var shield = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (shield != null)
                {
                    shield.SetHighlight(false);
                }
            }
        }
    }

    // =================================================================================
    // 👁️ REVEAL DRAWN CARD SYSTEM (สำหรับ [Cont.] RevealHand)
    // =================================================================================

    /// <summary>เช็คว่าผู้เล่นมีการ์ดที่มี [Cont.] RevealHand effect บนสนามหรือไม่</summary>
    bool HasPlayerContinuousRevealHandEffect()
    {
        if (IsHandRevealBlockedByContinuousEffect(protectedSideIsPlayer: false))
        {
            Debug.Log("🔒 Enemy hand is protected by continuous effect, skip reveal-drawn-card");
            return false;
        }

        // เช็ค Monster Slots
        foreach (Transform slot in playerMonsterSlots)
        {
            if (slot.childCount > 0)
            {
                var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (card != null && card.GetData() != null)
                {
                    var data = card.GetData();
                    foreach (var effect in data.effects)
                    {
                        if (effect.trigger == EffectTrigger.Continuous && 
                            effect.action == ActionType.RevealHand && 
                            effect.targetType == TargetType.EnemyHand)
                        {
                            if (IsEffectSuppressedByOpponentContinuousAura(card, effect, EffectTrigger.Continuous, sourceIsPlayer: true))
                            {
                                continue;
                            }

                            Debug.Log($"👁️ Found [Cont.] RevealHand: {data.cardName}");
                            return true;
                        }
                    }
                }
            }
        }

        // เช็ค Equip Slots
        foreach (Transform slot in playerEquipSlots)
        {
            if (slot.childCount > 0)
            {
                var card = slot.GetChild(0).GetComponent<BattleCardUI>();
                if (card != null && card.GetData() != null)
                {
                    var data = card.GetData();
                    foreach (var effect in data.effects)
                    {
                        if (effect.trigger == EffectTrigger.Continuous && 
                            effect.action == ActionType.RevealHand && 
                            effect.targetType == TargetType.EnemyHand)
                        {
                            if (IsEffectSuppressedByOpponentContinuousAura(card, effect, EffectTrigger.Continuous, sourceIsPlayer: true))
                            {
                                continue;
                            }

                            Debug.Log($"👁️ Found [Cont.] RevealHand: {data.cardName}");
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>เช็คว่าการ์ดเป็นการ์ดที่ reveal จากการ์ด RevealHand หรือไม่</summary>
    public bool IsCardRevealed(CardData cardData)
    {
        if (cardData == null) 
        {
            return false;
        }
        bool isRevealed = revealedEnemyCards.ContainsKey(cardData.card_id);
        return isRevealed;
    }

    /// <summary>จบเทิร์บอท - ซ่อนการ์ดที่ reveal กลับไป</summary>
    void EndEnemyTurn()
    {
        // ซ่อนการ์ดที่ reveal กลับไป
        if (revealedEnemyCards.Count > 0)
        {
            foreach (var card in enemyHandArea.GetComponentsInChildren<BattleCardUI>())
            {
                if (card.GetData() != null && revealedEnemyCards.ContainsKey(card.GetData().card_id))
                {
                    // เปลี่ยนกลับเป็นหลังการ์ด
                    var img = card.GetComponent<Image>();
                    if (img != null && cardBackPrefab != null)
                    {
                        var backImg = cardBackPrefab.GetComponent<Image>();
                        if (backImg != null && backImg.sprite != null)
                        {
                            img.sprite = backImg.sprite;
                        }
                    }
                    card.SetFrameVisible(false);
                    Debug.Log($"🔄 Hiding {card.GetData().cardName}");
                }
            }
            revealedEnemyCards.Clear();
        }
    }
}
