using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems;

// เพิ่ม Interface ให้ครบ ทั้งการคลิกและการลาก
public class BattleCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    private Image artworkImage;
    
    private CardData _cardData;
    private CanvasGroup canvasGroup; // ตัวช่วยให้เมาส์ทะลุการ์ดตอนลาก

    // 🔥 ตัวแปรเช็คสถานะ
    public bool isOnField = false; 
    public Transform parentAfterDrag; // จำตำแหน่งเดิมก่อนลาก
    public bool hasAttacked = false;
    private bool mulliganSelected = false;
    
    // 🎈 ตัวแปรสำหรับอนิเมชั่นลอย
    private float floatTime = 0f;
    private Vector3 originalPosition = Vector3.zero;
    private bool isFloating = false;
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
    }

    void CreateUIElementsIfNeeded()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) rectTransform = gameObject.AddComponent<RectTransform>();
        
        // กำหนดขนาดมาตรฐานการ์ด
        rectTransform.sizeDelta = new Vector2(140, 200); 

        if (artworkImage == null)
        {
            artworkImage = GetComponent<Image>();
            if (artworkImage == null) artworkImage = gameObject.AddComponent<Image>();
            
            artworkImage.color = Color.white;
            artworkImage.raycastTarget = true; 
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
        
        // 🎈 หยุดอนิเมชั่นลอยเพื่อไม่รบกวน HorizontalLayoutGroup
        floatTime = 0f;
        originalPosition = transform.localPosition;
        isFloating = false; // 🔥 ปิดลอยในมือ
    }

    // --- Helper Functions ---
    public int GetCost()
    {
        return _cardData != null ? _cardData.cost : 0;
    }

    public CardData GetData()
    {
        return _cardData;
    }

    // --- 🖱️ ส่วนการลาก (Drag System) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
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
        if (isOnField) return;
        // ขยับการ์ดตามเมาส์
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isOnField) return;

        // 1. เปิดการมองเห็นคืน
        canvasGroup.blocksRaycasts = true;

        // 2. ถ้าหลุดมือแล้วยังไม่มีที่อยู่ใหม่ (ไม่ได้ลง Slot) ให้เด้งกลับที่เดิม
        if (transform.parent == transform.root)
        {
            transform.SetParent(parentAfterDrag);
            
            // 🔥 ถ้ากลับไป handArea อย่า snap ศูนย -> ปล่อยให้ layout จัด
            if (parentAfterDrag != null && (parentAfterDrag.name == "HandArea" || parentAfterDrag.name == "handArea"))
            {
                // ไม่ต้อง snap - ปล่อยให้ HorizontalLayoutGroup จัด
            }
            else
            {
                transform.localPosition = Vector3.zero; // Snap กลับช่อง
            }
        }
        else
        {
            // วางสำเร็จ → จัดตำแหน่งให้อยู่กลางช่อง
            // 🔥 ถ้า parent ใหม่เป็น handArea อย่า snap
            if (transform.parent.name == "HandArea" || transform.parent.name == "handArea")
            {
                // ไม่ต้อง snap
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }
        
        // 🎈 หยุดอนิเมชั่นลอยในมือ
        if (!isOnField)
        {
            floatTime = 0f;
            originalPosition = transform.localPosition;
            isFloating = false; // 🔥 ปิดลอยในมือ
        }
    }

    // --- Interaction (คลิก / เมาส์ชี้) ---

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_cardData == null) return;

        bool isPrimary = eventData.button == PointerEventData.InputButton.Left;
        bool isSecondary = eventData.button == PointerEventData.InputButton.Right;

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
            if (BattleManager.Instance.state == BattleState.PLAYERTURN && _cardData.type == CardType.Monster)
            {
                if (!hasAttacked)
                {
                    BattleManager.Instance.OnPlayerAttack(this);
                    Debug.Log($"⚔️ โจมตี: {_cardData.cardName}");
                }
                else
                {
                    Debug.Log("⚠️ การ์ดนี้โจมตีแล้ว");
                }
            }
            else if (BattleManager.Instance.state == BattleState.DEFENDER_CHOICE)
            {
                if (_cardData.type == CardType.EquipSpell)
                {
                    // 🔥 ให้เลือกกัน เสมอ (ไม่ว่า subCategory ตรงหรือต่างก็ได้)
                    // OnPlayerSelectBlocker จะจัดการตรรมชาติการป้องกันเอง
                    var currentAttackerData = BattleManager.Instance.GetCurrentAttackerData();
                    if (currentAttackerData != null)
                    {
                        Debug.Log($"🛡️ เลือกกันด้วย {_cardData.cardName} ({_cardData.subCategory}) ต่อต้าน โจมตี ({currentAttackerData.subCategory})");
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
        if (artworkImage)
        {
            artworkImage.color = mulliganSelected ? Color.yellow : Color.white;
        }
    }
    
    // ฟังก์ชันรีเซ็ตตอนเริ่มเทิร์น (ให้โจมตีใหม่ได้)
    public void ResetAttackState()
    {
        hasAttacked = false;
        // เปลี่ยนสีกลับเป็นปกติ (ถ้าตอนตีเปลี่ยนสีไว้)
        if(artworkImage) artworkImage.color = Color.white;
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