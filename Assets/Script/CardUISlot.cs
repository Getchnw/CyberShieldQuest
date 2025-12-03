using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CardUISlot : MonoBehaviour
{
    public Image cardImage; // ช่องเอารูปมาใส่
    public Button btn;      // ปุ่มกด

    private CardData _data;

    public void Setup(CardData data, UnityAction<CardData> onClick)
    {
        _data = data;

        // ถ้ามีรูป ให้โชว์รูป
        if (data.artwork != null)
        {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        }
        else
        {
            cardImage.color = Color.red; // ไม่มีรูปให้เป็นสีแดง
        }

        // ตั้งค่าปุ่มกด
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick(_data));
    }
}