using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Question Identifiers")]
    [Tooltip("สำหรับ Fill-in-the-Blank (เทียบข้อความ)")]
    public string promptID; // เช่น "ข้อ 5"

    [Tooltip("สำหรับ Matching (เทียบ Index)")]
    public int promptIndex = -1; // เช่น 0, 1, 2...

    [Header("Options")]
    [Tooltip("ติ๊กถูก (true) ถ้าเป็น 'Fill in the Blank' เพื่อให้ช่องขยายตามคำ")]
    public bool resizeOnDrop = false;

    [Header("Current Answer (Read-Only)")]
    [HideInInspector]
    public string currentDroppedAnswerID;
    [HideInInspector]
    public int currentDroppedAnswerIndex = -1;

    private LayoutElement dropZoneLayout;

    public static event System.Action OnDropZoneChanged;
    private float defaultWidth;

    void Awake()
    {
        if (resizeOnDrop)
        {
            dropZoneLayout = GetComponent<LayoutElement>();
            if (dropZoneLayout == null)
            {
                defaultWidth = dropZoneLayout.preferredWidth;
            }
        }
    }

    // ฟังก์ชันสำหรับล้างค่า (เรียกจาก DraggableItem)
    public void ResetAnswer()
    {
        currentDroppedAnswerID = "";
        currentDroppedAnswerIndex = -1;
        // คืนขนาดเดิม
        if (resizeOnDrop && dropZoneLayout != null)
        {
            dropZoneLayout.preferredWidth = defaultWidth;
        }

        OnDropZoneChanged?.Invoke(); // แจ้งเตือน!
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 1. ตรวจสอบว่าสิ่งที่ลากมาคือ DraggableItem
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem == null) return;

        // เก็บ DropZone เก่าไว้ก่อน (ที่ที่ draggableItem เคยอยู่)
        DropZone previousDropZone = draggableItem.originalParent.GetComponent<DropZone>();

        // 2. ถ้าในช่องนี้มี "ของเก่า" วางอยู่
        if (transform.childCount > 0)
        {
            // ดึง "ของเก่า" ออกมา
            DraggableItem oldItem = transform.GetChild(0).GetComponent<DraggableItem>();
            if (oldItem != null)
            {
                // ส่งของเก่า (B) กลับไปที่บ้านของ (A)
                oldItem.transform.SetParent(draggableItem.originalParent);
                oldItem.transform.localPosition = Vector2.zero;

                // ถ้าบ้านเก่าเป็น DropZone เราต้องอัปเดตข้อมูลให้มันด้วย!
                if (previousDropZone != null)
                {
                    // บ้านเก่า (A) ตอนนี้มี (B) อยู่แทน
                    previousDropZone.currentDroppedAnswerID = oldItem.answerID;
                    previousDropZone.currentDroppedAnswerIndex = oldItem.answerIndex;
                    // (ถ้าใช้ resizeOnDrop ก็ต้อง update size บ้านเก่าด้วย)
                }
            }
        }
        else
        {
            // ถ้าไม่มีของเก่า (A ย้ายมาที่ว่าง)
            // บ้านเก่าของ A จะต้อง "ว่างลง"
            if (previousDropZone != null)
            {
                previousDropZone.ResetAnswer(); // สั่งเคลียร์บ้านเก่า
            }
        }

        // 3. ย้าย "ของใหม่" (ที่เพิ่งลากมา) เข้ามาในช่องนี้
        draggableItem.transform.SetParent(this.transform);
        draggableItem.transform.localPosition = Vector2.zero;

        // 4. ปรับขนาด DropZone ให้พอดีกับ "คำ"
        // 4a. ดึง LayoutElement ของ "คำ" ที่เพิ่งลากมา
        if (resizeOnDrop && dropZoneLayout != null)
        {
            LayoutElement wordLayout = draggableItem.GetComponent<LayoutElement>();
            if (wordLayout != null && dropZoneLayout != null)
            {
                // 4b. สั่งให้ "ช่อง" นี้มี "ขนาดที่ต้องการ" (Preferred Width)
                dropZoneLayout.preferredWidth = wordLayout.preferredWidth;
            }
            else
            {
                // (กรณีสำรอง ถ้า "คำ" ไม่มี Layout Element ให้ใช้ขนาดจาก RectTransform)
                RectTransform wordRect = draggableItem.GetComponent<RectTransform>();
                dropZoneLayout.preferredWidth = wordRect.rect.width;
            }
        }

        // 5. บันทึกคำตอบ
        currentDroppedAnswerID = draggableItem.answerID;
        currentDroppedAnswerIndex = draggableItem.answerIndex;

        //แจ้งเตือนว่า "มีคนมาวางแล้วนะ!"
        OnDropZoneChanged?.Invoke();
    }
}