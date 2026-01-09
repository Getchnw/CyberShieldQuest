using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    // กำหนดใน Inspector: ช่องนี้รับ Monster หรือ EquipSpell
    public CardType allowedType; 

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        BattleCardUI card = droppedObj.GetComponent<BattleCardUI>();

        if (card != null)
        {
            // 🔥 ถ้าเป็นโหมด Mulligan ให้รับการ์ดได้เสมอ (ไม่เช็คประเภท)
            if (BattleManager.Instance != null && BattleManager.Instance.IsMulliganPhase())
            {
                // ช่องนี้ว่าง → รับได้
                if (transform.childCount == 0)
                {
                    card.transform.SetParent(transform);
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localScale = Vector3.one;
                }
                return;
            }

            // โหมดปกติ: ส่ง "การ์ด" และ "ช่องนี้ (this)" ไปให้ Manager เช็ค
            BattleManager.Instance.TrySummonCard(card, this);
        }
    }
}