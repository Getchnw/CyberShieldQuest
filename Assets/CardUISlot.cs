using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUISlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image cardImage;
    public Button btn;
    public TextMeshProUGUI amountText;

    private CardData _data;
    private UnityAction<CardData> _onLeftClick;
    private UnityAction<CardData> _onRightClick;

    private Vector3 originalScale;
    private Coroutine currentAnim;
    
    // 🔥 เพิ่มตัวแปร Canvas
    private Canvas myCanvas;

    void Awake()
    {
        originalScale = transform.localScale;
        // หา Canvas ในตัวมันเอง (ถ้าลืมใส่ใน Prefab โค้ดนี้จะช่วยกัน Error)
        myCanvas = GetComponent<Canvas>();
    }

    public void Setup(CardData data, int amount, UnityAction<CardData> leftClick, UnityAction<CardData> rightClick)
    {
        _data = data;
        _onLeftClick = leftClick;
        _onRightClick = rightClick;

        if (currentAnim != null) StopCoroutine(currentAnim);
        transform.localScale = originalScale;

        // Reset Sorting (กันค้าง)
        if (myCanvas != null) myCanvas.sortingOrder = 0;

        if (data.artwork != null) {
            cardImage.sprite = data.artwork;
            cardImage.color = Color.white;
        } else {
            cardImage.color = Color.red; 
        }

        if (amountText != null) amountText.text = (amount >= 0) ? $"x{amount}" : "";
        if (amount == 0) cardImage.color = Color.gray;

        btn.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale * 1.15f, 0.1f));
        
        // 🔥 เทคนิคพิเศษ: ดัน Canvas ขึ้นมาทับคนอื่น
        if (myCanvas != null) myCanvas.sortingOrder = 10; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimateScale(originalScale, 0.1f));

        // 🔥 คืนค่ากลับไปปกติ
        if (myCanvas != null) myCanvas.sortingOrder = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        StartCoroutine(ClickEffect());

        if (eventData.button == PointerEventData.InputButton.Left) _onLeftClick?.Invoke(_data);
        else if (eventData.button == PointerEventData.InputButton.Right) _onRightClick?.Invoke(_data);
    }

    IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.SmoothStep(0, 1, t); 
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator ClickEffect()
    {
        yield return StartCoroutine(AnimateScale(originalScale * 0.9f, 0.05f));
        yield return StartCoroutine(AnimateScale(originalScale * 1.1f, 0.05f));
    }
}