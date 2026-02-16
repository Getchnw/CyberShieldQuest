using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 🔥 ตัวจัดการคลิกแบบง่ายๆ สำหรับการ์ดในมือที่ reveal
/// ใช้สำหรับเปลี่ยนการ์ดเพื่อดูรายละเอียด โดยไม่มีการลาก
/// </summary>
public class PointerClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Action OnClickAction;

    public void OnPointerClick(PointerEventData eventData)
    {
        // เฉพาะคลิกซ้ายเท่านั้น
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Debug.Log($"✅ PointerClickHandler.OnPointerClick: {gameObject.name}");
        OnClickAction?.Invoke();
    }
}
