using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro; // 🔥 อย่าลืม

public class CardUISlot : MonoBehaviour, IPointerClickHandler
{
    public Image cardImage; 
    public Button btn;
    public TextMeshProUGUI amountText; // 🔥 ช่องใส่ตัวเลข

    private CardData _data;
    private UnityAction<CardData> _onLeftClick;
    private UnityAction<CardData> _onRightClick;

    // เพิ่ม int amount เข้ามาใน Setup
    public void Setup(CardData data, int amount, UnityAction<CardData> leftClick, UnityAction<CardData> rightClick)
    {
        _data = data;
        _onLeftClick = leftClick;
        _onRightClick = rightClick;
        
        if (data.artwork != null) {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        } else {
            cardImage.color = Color.red; 
        }

        // 🔥 โชว์จำนวน (ถ้าส่งมา -1 แปลว่าไม่ต้องโชว์ เช่นอยู่ในเด็คแล้ว)
        if (amountText != null)
        {
            if (amount >= 0) amountText.text = $"x{amount}";
            else amountText.text = "";
        }

        // ทำให้การ์ดเทาๆ ถ้าจำนวนเป็น 0 (ไม่มีของ)
        if (amount == 0) cardImage.color = Color.gray;

        btn.onClick.RemoveAllListeners();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) _onLeftClick?.Invoke(_data);
        else if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke(_data);
    }
}