using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CardUISlot : MonoBehaviour
{
    public Image cardImage; // รูปการ์ด
    public Button btn;      // ปุ่มกด

    private CardData _data;

    // ฟังก์ชันนี้จะถูกเรียกตอนสร้างการ์ด
    public void Setup(CardData data, UnityAction<CardData> onClick)
    {
        _data = data;
        
        // เอารูปจาก CardData มาใส่ (ถ้ามี)
        if (data.artwork != null)
        {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        }
        else
        {
            cardImage.color = Color.gray; // ไม่มีรูปให้เป็นสีเทา
        }

        // ตั้งค่าปุ่มกด
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick(_data));
    }
}