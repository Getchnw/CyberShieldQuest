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

            // โหมดปกติ
            // 🔥 เช็คว่าช่องนี้มีการ์ดอยู่แล้วหรือไม่
            if (transform.childCount > 0)
            {
                // ช่องมีการ์ดแล้ว → ลองทำ Sacrifice
                BattleCardUI targetCard = transform.GetChild(0).GetComponent<BattleCardUI>();
                if (targetCard != null && BattleManager.Instance != null)
                {
                    // ห้ามสังเวยการ์ดใบเดียวกัน (ลากทับตัวเอง)
                    if (ReferenceEquals(card, targetCard)) return;

                    var newType = card.GetData().type;
                    var targetType = targetCard.GetData().type;

                    // 🔥 Token ให้นับเป็น Monster เพื่อให้สังเวยได้
                    if (newType == CardType.Token) newType = CardType.Monster;
                    if (targetType == CardType.Token) targetType = CardType.Monster;

                    // ป้องกันลากการ์ดคนละประเภทลงช่องผิด
                    if (newType != allowedType || targetType != allowedType || newType != targetType)
                    {
                        Debug.Log("⚠️ Sacrifice ไม่ได้: ประเภทการ์ดไม่ตรงกับช่อง");
                        return;
                    }

                    // ต้องมีการ์ดเก่าอยู่บนสนามจริง ๆ
                    if (!targetCard.isOnField)
                    {
                        Debug.Log("⚠️ Sacrifice ไม่ได้: การ์ดเป้าหมายยังไม่อยู่บนสนาม");
                        return;
                    }

                    // การ์ดใหม่ต้องมาจากมือ ไม่ใช่การ์ดบนสนาม
                    if (card.isOnField)
                    {
                        Debug.Log("⚠️ Sacrifice ไม่ได้: การ์ดใหม่อยู่บนสนามแล้ว");
                        return;
                    }

                    // ตรวจสอบว่ากำลังเล่นตาผู้เล่น
                    if (BattleManager.Instance.state == BattleState.PLAYERTURN)
                    {
                        // เปิด popup ยืนยัน
                        BattleManager.Instance.ShowSacrificeConfirmPopup(card, targetCard);
                        Debug.Log($"🔄 เสนอ Sacrifice: {card.GetData().cardName} → {targetCard.GetData().cardName}");
                    }
                    else
                    {
                        Debug.Log("⚠️ ไม่ใช่เทิร์นผู้เล่น ไม่สามารถ Sacrifice ได้");
                    }
                }
            }
            else
            {
                // ช่องว่าง → ลงการ์ดตามปกติ
                BattleManager.Instance.TrySummonCard(card, this);
            }
        }
    }
}