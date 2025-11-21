using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ตัวแปรสาธารณะเพื่อเก็บ "คำตอบ" ของไอเท็มนี้
    [Tooltip("สำหรับ Fill-in-the-Blank (เทียบข้อความ)")]
    public string answerID; // เช่น "พรางตา (GUID)"

    [Tooltip("สำหรับ Matching (เทียบ Index)")]
    public int answerIndex = -1; // เช่น 0, 1, 2...

    [HideInInspector]
    public Transform originalParent; // ที่อยู่เดิม (เช่น คลังคำ)

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas mainCanvas;

    public static event System.Action OnItemBeginDrag;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // 2. หา Canvas ตัวแม่สุดที่คุม UI นี้อยู่
        mainCanvas = GetComponentInParent<Canvas>();

        // (กันเหนียว) ถ้าหาไม่เจอ ให้ลองหาจาก root
        if (mainCanvas == null)
        {
            mainCanvas = transform.root.GetComponent<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // จำที่อยู่เดิมไว้

        // ถ้าดึงออกจาก DropZone -> ต้องรีเซ็ตค่าใน DropZone นั้น
        DropZone oldDropZone = originalParent.GetComponent<DropZone>();
        if (oldDropZone != null)
        {
            oldDropZone.ResetAnswer(); // เดี๋ยวเราไปสร้างฟังก์ชันนี้ใน DropZone
        }

        transform.SetParent(transform.root); // ดึงออกมาอยู่ชั้นบนสุด (เพื่อให้ลากทับ UI อื่นได้)
        canvasGroup.blocksRaycasts = false; // "ทะลุ" เพื่อให้ DropZone ตรวจจับเมาส์ได้

        OnItemBeginDrag?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ย้ายตำแหน่งตามเมาส์
        if (mainCanvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // กลับมาทึบแสงเหมือนเดิม

        // ถ้ามันไม่ได้ถูกย้ายไปที่ DropZone (ยังอยู่ชั้นบนสุด)
        if (transform.parent == transform.root)
        {
            // เช็คว่า "ที่เดิม" คือ DropZone หรือเปล่า?
            DropZone oldDropZone = originalParent.GetComponent<DropZone>();

            if (oldDropZone != null)
            {
                // ถ้าใช่: แปลว่าเราดึงมัน "ออก" จากช่อง
                // อย่ากลับไปที่เดิม! ให้กลับไป "คลังคำ" แทน

                // (คุณต้องหาวิธีอ้างอิงถึง "คลังคำ" ให้ได้)
                // วิธีง่ายสุด: หา GameObject ที่ชื่อ "List Answer Box" หรือ "Word Bank Container"
                // (แต่ต้องระวังเรื่องชื่อต้องตรงเป๊ะๆ หรือใช้ Tag ก็ได้)
                GameObject wordBank = GameObject.Find("List Answer Box");

                if (wordBank != null)
                {
                    transform.SetParent(wordBank.transform); // ย้ายไปคลัง
                }
                else
                {
                    // ถ้าหาคลังไม่เจอ ก็กลับที่เดิมไปก่อน
                    transform.SetParent(originalParent);
                }
            }
            else
            {
                // ถ้าที่เดิมคือคลังอยู่แล้ว ก็กลับไป
                transform.SetParent(originalParent);
            }

            rectTransform.anchoredPosition = Vector2.zero;
        }
        // ส่งสัญญาณบอกว่าวางแล้ว (ไม่ว่าจะวางลงช่องหรือเด้งกลับ)
        OnItemBeginDrag?.Invoke();
    }
}